using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Service.Implemention
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork unitOfWork;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Task<IEnumerable<InventoryItem>> GetItemsAsync(
            bool activeOnly, bool lowStockOnly, CancellationToken cancellationToken = default)
            => unitOfWork.InventoryItemsRepository.GetAllAsync(activeOnly, lowStockOnly, cancellationToken);

        public async Task<InventoryItem> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await unitOfWork.InventoryItemsRepository.GetByIdAsync(id, cancellationToken);
            if (item == null)
                throw new NotFoundException($"Inventory item with ID {id} not found.");
            return item;
        }

        public async Task<InventoryItem> CreateItemAsync(
            string sku,
            string name,
            string category,
            string unit,
            decimal minimumStock,
            decimal initialQuantity,
            bool isActive,
            string? recordedByUserId,
            CancellationToken cancellationToken = default)
        {
            var normalizedSku = NormalizeSku(sku);
            var existing = await unitOfWork.InventoryItemsRepository.GetBySkuAsync(normalizedSku, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Ya existe un ítem con SKU '{normalizedSku}'.");

            if (minimumStock < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            if (initialQuantity < 0)
                throw new InvalidOperationException("La cantidad inicial no puede ser negativa.");

            var item = new InventoryItem
            {
                Sku = normalizedSku,
                Name = name.Trim(),
                Category = category.Trim().ToUpperInvariant(),
                Unit = NormalizeUnit(unit),
                MinimumStock = RoundQty(minimumStock),
                QuantityOnHand = 0,
                IsActive = isActive
            };

            await unitOfWork.InventoryItemsRepository.AddAsync(item, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            if (initialQuantity > 0)
            {
                await ApplyMovementAsync(
                    item,
                    StockMovementType.In,
                    RoundQty(initialQuantity),
                    "Stock inicial",
                    null,
                    "InventoryItem",
                    item.Id.ToString(),
                    recordedByUserId,
                    cancellationToken);
            }

            return item;
        }

        public async Task<InventoryItem> UpdateItemAsync(
            int id,
            string sku,
            string name,
            string category,
            string unit,
            decimal minimumStock,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var item = await GetTrackedItemAsync(id, cancellationToken);
            var normalizedSku = NormalizeSku(sku);
            var existing = await unitOfWork.InventoryItemsRepository.GetBySkuAsync(normalizedSku, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Ya existe un ítem con SKU '{normalizedSku}'.");

            if (minimumStock < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");

            item.Sku = normalizedSku;
            item.Name = name.Trim();
            item.Category = category.Trim().ToUpperInvariant();
            item.Unit = NormalizeUnit(unit);
            item.MinimumStock = RoundQty(minimumStock);
            item.IsActive = isActive;

            unitOfWork.InventoryItemsRepository.Update(item, cancellationToken);
            return item;
        }

        public async Task SoftDeleteItemAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await GetTrackedItemAsync(id, cancellationToken);
            item.IsDeleted = true;
            item.DeletedAt = DateTime.Now;
            unitOfWork.InventoryItemsRepository.Update(item, cancellationToken);
        }

        public async Task<StockMovement> RegisterEntryAsync(
            int inventoryItemId,
            decimal quantity,
            string? reason,
            string? notes,
            string? recordedByUserId,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("La cantidad de entrada debe ser mayor que cero.");

            var item = await GetTrackedItemAsync(inventoryItemId, cancellationToken);
            return await ApplyMovementAsync(
                item,
                StockMovementType.In,
                RoundQty(quantity),
                reason ?? "Entrada de inventario",
                notes,
                "Purchase",
                null,
                recordedByUserId,
                cancellationToken);
        }

        public async Task<StockMovement> RegisterAdjustmentAsync(
            int inventoryItemId,
            decimal newQuantityOnHand,
            string? reason,
            string? notes,
            string? recordedByUserId,
            CancellationToken cancellationToken = default)
        {
            if (newQuantityOnHand < 0)
                throw new InvalidOperationException("El stock resultante no puede ser negativo.");

            var item = await GetTrackedItemAsync(inventoryItemId, cancellationToken);
            var target = RoundQty(newQuantityOnHand);
            var delta = target - item.QuantityOnHand;
            if (delta == 0)
                throw new InvalidOperationException("El ajuste no cambia el stock actual.");

            return await ApplyMovementAsync(
                item,
                StockMovementType.Adjustment,
                Math.Abs(delta),
                reason ?? "Ajuste de inventario",
                notes,
                "Adjustment",
                null,
                recordedByUserId,
                cancellationToken,
                forcedAfter: target);
        }

        public Task<List<StockMovement>> GetMovementsAsync(
            int inventoryItemId, int take = 50, CancellationToken cancellationToken = default)
            => unitOfWork.StockMovementsRepository.GetByItemIdAsync(inventoryItemId, take, cancellationToken);

        public Task<List<ProcedureMaterial>> GetProcedureBomAsync(
            int treatmentProcedureId, CancellationToken cancellationToken = default)
            => unitOfWork.ProcedureMaterialsRepository.GetByProcedureIdWithItemsAsync(treatmentProcedureId, cancellationToken);

        public async Task ReplaceProcedureBomAsync(
            int treatmentProcedureId,
            IEnumerable<ProcedureMaterialInput> materials,
            CancellationToken cancellationToken = default)
        {
            var procedure = await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(treatmentProcedureId, cancellationToken);
            if (procedure == null)
                throw new NotFoundException($"Treatment procedure with ID {treatmentProcedureId} not found.");

            var incoming = (materials ?? [])
                .GroupBy(m => m.InventoryItemId)
                .Select(g => g.Last())
                .ToList();

            foreach (var line in incoming)
            {
                if (line.DefaultQuantity <= 0)
                    throw new InvalidOperationException("La cantidad BOM debe ser mayor que cero.");
                var item = await unitOfWork.InventoryItemsRepository.GetByIdAsync(line.InventoryItemId, cancellationToken);
                if (item == null)
                    throw new NotFoundException($"Inventory item with ID {line.InventoryItemId} not found.");
            }

            var existing = await unitOfWork.ProcedureMaterialsRepository.FindAsync(
                p => p.TreatmentProcedureId == treatmentProcedureId, cancellationToken);
            foreach (var row in existing)
                unitOfWork.ProcedureMaterialsRepository.Delete(row, cancellationToken);

            var sort = 0;
            foreach (var line in incoming.OrderBy(m => m.SortOrder).ThenBy(m => m.InventoryItemId))
            {
                await unitOfWork.ProcedureMaterialsRepository.AddAsync(new ProcedureMaterial
                {
                    TreatmentProcedureId = treatmentProcedureId,
                    InventoryItemId = line.InventoryItemId,
                    DefaultQuantity = RoundQty(line.DefaultQuantity),
                    IsOptional = line.IsOptional,
                    SortOrder = line.SortOrder > 0 ? line.SortOrder : ++sort
                }, cancellationToken);
            }
        }

        public async Task<MaterialConsumptionProposalDTO> ProposeConsumptionAsync(
            int dentalTreatmentId, CancellationToken cancellationToken = default)
        {
            var treatment = await unitOfWork.DentalTreatmentsRepository.GetByIdAsync(dentalTreatmentId, cancellationToken);
            if (treatment == null)
                throw new NotFoundException($"Dental treatment with ID {dentalTreatmentId} not found.");

            var existing = await unitOfWork.TreatmentMaterialConsumptionsRepository
                .GetByTreatmentIdWithLinesAsync(dentalTreatmentId, cancellationToken);

            var proposal = new MaterialConsumptionProposalDTO
            {
                DentalTreatmentId = treatment.Id,
                TreatmentProcedureId = treatment.TreatmentProcedureId,
                ProcedureName = treatment.ProcedureName,
                AlreadyConsumed = existing != null
            };

            if (existing != null)
            {
                proposal.HasBom = true;
                proposal.Lines = existing.Lines
                    .OrderBy(l => l.Id)
                    .Select(l => new MaterialConsumptionLineDTO
                    {
                        InventoryItemId = l.InventoryItemId,
                        Sku = l.InventoryItem.Sku,
                        Name = l.InventoryItem.Name,
                        Unit = l.InventoryItem.Unit,
                        ProposedQuantity = l.ProposedQuantity,
                        ActualQuantity = l.ActualQuantity,
                        IsIncluded = l.IsIncluded,
                        QuantityOnHand = l.InventoryItem.QuantityOnHand,
                        InsufficientStock = false
                    }).ToList();
                return proposal;
            }

            if (!treatment.TreatmentProcedureId.HasValue)
                return proposal;

            var bom = await unitOfWork.ProcedureMaterialsRepository
                .GetByProcedureIdWithItemsAsync(treatment.TreatmentProcedureId.Value, cancellationToken);
            proposal.HasBom = bom.Count > 0;
            proposal.Lines = bom.Select(b =>
            {
                var qty = b.DefaultQuantity;
                return new MaterialConsumptionLineDTO
                {
                    InventoryItemId = b.InventoryItemId,
                    Sku = b.InventoryItem.Sku,
                    Name = b.InventoryItem.Name,
                    Unit = b.InventoryItem.Unit,
                    ProposedQuantity = qty,
                    ActualQuantity = qty,
                    IsIncluded = !b.IsOptional,
                    IsOptional = b.IsOptional,
                    QuantityOnHand = b.InventoryItem.QuantityOnHand,
                    InsufficientStock = b.InventoryItem.QuantityOnHand < qty
                };
            }).ToList();

            return proposal;
        }

        public async Task<TreatmentMaterialConsumption> ConfirmConsumptionAsync(
            int dentalTreatmentId,
            IEnumerable<MaterialConsumptionLineInput>? lines,
            string? notes,
            string? recordedByUserId,
            bool allowInsufficientStock = false,
            CancellationToken cancellationToken = default)
        {
            var existing = await unitOfWork.TreatmentMaterialConsumptionsRepository
                .GetByTreatmentIdAsync(dentalTreatmentId, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException("Este tratamiento ya tiene un consumo de materiales registrado.");

            var treatment = await unitOfWork.DentalTreatmentsRepository.GetByIdAsync(dentalTreatmentId, cancellationToken);
            if (treatment == null)
                throw new NotFoundException($"Dental treatment with ID {dentalTreatmentId} not found.");

            var resolvedLines = await ResolveConsumptionLinesAsync(treatment, lines, cancellationToken);
            return await PersistConsumptionAsync(
                treatment,
                resolvedLines,
                notes,
                recordedByUserId,
                allowInsufficientStock,
                cancellationToken);
        }

        public Task<TreatmentMaterialConsumption?> GetConsumptionByTreatmentAsync(
            int dentalTreatmentId, CancellationToken cancellationToken = default)
            => unitOfWork.TreatmentMaterialConsumptionsRepository.GetByTreatmentIdWithLinesAsync(dentalTreatmentId, cancellationToken);

        public async Task<TreatmentMaterialConsumption> ReplaceConsumptionAsync(
            int dentalTreatmentId,
            IEnumerable<MaterialConsumptionLineInput> lines,
            string? notes,
            string? recordedByUserId,
            bool allowInsufficientStock = false,
            CancellationToken cancellationToken = default)
        {
            var treatment = await unitOfWork.DentalTreatmentsRepository.GetByIdAsync(dentalTreatmentId, cancellationToken);
            if (treatment == null)
                throw new NotFoundException($"Dental treatment with ID {dentalTreatmentId} not found.");

            var existing = await unitOfWork.TreatmentMaterialConsumptionsRepository
                .GetByTreatmentIdForUpdateAsync(dentalTreatmentId, cancellationToken);
            if (existing == null)
                throw new NotFoundException("No hay consumo registrado para este tratamiento.");

            foreach (var line in existing.Lines.Where(l => l.IsIncluded && l.StockMovementId.HasValue))
            {
                var movement = await unitOfWork.StockMovementsRepository.GetByIdAsync(line.StockMovementId!.Value, cancellationToken);
                if (movement == null || movement.Type != StockMovementType.Out)
                    continue;

                var item = await GetTrackedItemAsync(line.InventoryItemId, cancellationToken);
                await ApplyMovementAsync(
                    item,
                    StockMovementType.Reversal,
                    movement.Quantity,
                    "Reverso por modificación de consumo",
                    notes,
                    nameof(DentalTreatment),
                    dentalTreatmentId.ToString(),
                    recordedByUserId,
                    cancellationToken,
                    reversesMovementId: movement.Id);
            }

            unitOfWork.TreatmentMaterialConsumptionsRepository.Delete(existing, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            var resolvedLines = await ResolveConsumptionLinesAsync(treatment, lines, cancellationToken);
            return await PersistConsumptionAsync(
                treatment,
                resolvedLines,
                notes,
                recordedByUserId,
                allowInsufficientStock,
                cancellationToken);
        }

        public async Task<LowStockAlertDTO> GetLowStockAsync(CancellationToken cancellationToken = default)
        {
            var items = (await unitOfWork.InventoryItemsRepository.GetAllAsync(true, true, cancellationToken)).ToList();
            return new LowStockAlertDTO
            {
                Count = items.Count,
                Items = ToItemDtos(items)
            };
        }

        public InventoryItemDTO ToItemDto(InventoryItem item) => new()
        {
            Id = item.Id,
            Sku = item.Sku,
            Name = item.Name,
            Category = item.Category,
            Unit = item.Unit,
            QuantityOnHand = item.QuantityOnHand,
            MinimumStock = item.MinimumStock,
            IsActive = item.IsActive,
            IsLowStock = item.QuantityOnHand <= item.MinimumStock,
            CreatedAt = item.CreatedAt
        };

        public List<InventoryItemDTO> ToItemDtos(IEnumerable<InventoryItem> items)
            => items.Select(ToItemDto).ToList();

        public StockMovementDTO ToMovementDto(StockMovement movement, string? itemName = null) => new()
        {
            Id = movement.Id,
            InventoryItemId = movement.InventoryItemId,
            InventoryItemName = itemName ?? movement.InventoryItem?.Name ?? string.Empty,
            Type = movement.Type.ToString(),
            Quantity = movement.Quantity,
            QuantityBefore = movement.QuantityBefore,
            QuantityAfter = movement.QuantityAfter,
            SignedDelta = movement.SignedDelta,
            Reason = movement.Reason,
            Notes = movement.Notes,
            ReferenceType = movement.ReferenceType,
            ReferenceId = movement.ReferenceId,
            CreatedAt = movement.CreatedAt
        };

        public ProcedureMaterialDTO ToBomDto(ProcedureMaterial material) => new()
        {
            Id = material.Id,
            TreatmentProcedureId = material.TreatmentProcedureId,
            InventoryItemId = material.InventoryItemId,
            InventoryItemSku = material.InventoryItem?.Sku ?? string.Empty,
            InventoryItemName = material.InventoryItem?.Name ?? string.Empty,
            Unit = material.InventoryItem?.Unit ?? "u",
            DefaultQuantity = material.DefaultQuantity,
            IsOptional = material.IsOptional,
            SortOrder = material.SortOrder,
            QuantityOnHand = material.InventoryItem?.QuantityOnHand ?? 0
        };

        public TreatmentMaterialConsumptionDTO ToConsumptionDto(TreatmentMaterialConsumption consumption) => new()
        {
            Id = consumption.Id,
            DentalTreatmentId = consumption.DentalTreatmentId,
            Notes = consumption.Notes,
            ConfirmedAt = consumption.ConfirmedAt,
            Lines = consumption.Lines.Select(l => new MaterialConsumptionLineDTO
            {
                InventoryItemId = l.InventoryItemId,
                Sku = l.InventoryItem?.Sku ?? string.Empty,
                Name = l.InventoryItem?.Name ?? string.Empty,
                Unit = l.InventoryItem?.Unit ?? "u",
                ProposedQuantity = l.ProposedQuantity,
                ActualQuantity = l.ActualQuantity,
                IsIncluded = l.IsIncluded,
                QuantityOnHand = l.InventoryItem?.QuantityOnHand ?? 0
            }).ToList()
        };

        private async Task<List<(MaterialConsumptionLineInput Input, InventoryItem Item, decimal Proposed)>> ResolveConsumptionLinesAsync(
            DentalTreatment treatment,
            IEnumerable<MaterialConsumptionLineInput>? lines,
            CancellationToken cancellationToken)
        {
            Dictionary<int, decimal> bomProposed = [];
            if (treatment.TreatmentProcedureId.HasValue)
            {
                var bom = await unitOfWork.ProcedureMaterialsRepository
                    .GetByProcedureIdAsync(treatment.TreatmentProcedureId.Value, cancellationToken);
                bomProposed = bom.ToDictionary(b => b.InventoryItemId, b => b.DefaultQuantity);
            }

            List<MaterialConsumptionLineInput> effective;
            if (lines == null)
            {
                if (!treatment.TreatmentProcedureId.HasValue)
                    return [];

                var bomLines = await unitOfWork.ProcedureMaterialsRepository
                    .GetByProcedureIdWithItemsAsync(treatment.TreatmentProcedureId.Value, cancellationToken);
                effective = bomLines.Select(b => new MaterialConsumptionLineInput
                {
                    InventoryItemId = b.InventoryItemId,
                    Quantity = b.DefaultQuantity,
                    IsIncluded = !b.IsOptional,
                    ProposedQuantity = b.DefaultQuantity
                }).ToList();
            }
            else
            {
                effective = lines
                    .GroupBy(l => l.InventoryItemId)
                    .Select(g => g.Last())
                    .ToList();
            }

            var result = new List<(MaterialConsumptionLineInput, InventoryItem, decimal)>();
            foreach (var line in effective)
            {
                var item = await GetTrackedItemAsync(line.InventoryItemId, cancellationToken);
                var proposed = line.ProposedQuantity
                    ?? (bomProposed.TryGetValue(line.InventoryItemId, out var bomQty) ? bomQty : line.Quantity);
                result.Add((line, item, RoundQty(proposed)));
            }

            return result;
        }

        private async Task<TreatmentMaterialConsumption> PersistConsumptionAsync(
            DentalTreatment treatment,
            List<(MaterialConsumptionLineInput Input, InventoryItem Item, decimal Proposed)> resolvedLines,
            string? notes,
            string? recordedByUserId,
            bool allowInsufficientStock,
            CancellationToken cancellationToken)
        {
            var consumption = new TreatmentMaterialConsumption
            {
                DentalTreatmentId = treatment.Id,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                ConfirmedByUserId = recordedByUserId,
                ConfirmedAt = DateTime.UtcNow
            };

            foreach (var (input, item, proposed) in resolvedLines)
            {
                var included = input.IsIncluded && input.Quantity > 0;
                var actual = included ? RoundQty(input.Quantity) : 0;

                StockMovement? movement = null;
                if (included)
                {
                    if (!allowInsufficientStock && item.QuantityOnHand < actual)
                        throw new InvalidOperationException(
                            $"Stock insuficiente para '{item.Name}'. Disponible: {item.QuantityOnHand} {item.Unit}, requerido: {actual} {item.Unit}.");

                    movement = await ApplyMovementAsync(
                        item,
                        StockMovementType.Out,
                        actual,
                        "Consumo por tratamiento",
                        null,
                        nameof(DentalTreatment),
                        treatment.Id.ToString(),
                        recordedByUserId,
                        cancellationToken);
                }

                consumption.Lines.Add(new TreatmentMaterialConsumptionLine
                {
                    InventoryItemId = item.Id,
                    ProposedQuantity = proposed,
                    ActualQuantity = actual,
                    IsIncluded = included,
                    StockMovement = movement
                });
            }

            await unitOfWork.TreatmentMaterialConsumptionsRepository.AddAsync(consumption, cancellationToken);
            return consumption;
        }

        private async Task<InventoryItem> GetTrackedItemAsync(int id, CancellationToken cancellationToken)
        {
            var item = await unitOfWork.InventoryItemsRepository.GetByIdForUpdateAsync(id, cancellationToken);
            if (item == null)
                throw new NotFoundException($"Inventory item with ID {id} not found.");
            return item;
        }

        private async Task<StockMovement> ApplyMovementAsync(
            InventoryItem item,
            StockMovementType type,
            decimal quantity,
            string? reason,
            string? notes,
            string? referenceType,
            string? referenceId,
            string? recordedByUserId,
            CancellationToken cancellationToken,
            decimal? forcedAfter = null,
            int? reversesMovementId = null)
        {
            quantity = RoundQty(quantity);
            if (quantity <= 0)
                throw new InvalidOperationException("La cantidad del movimiento debe ser mayor que cero.");

            var before = item.QuantityOnHand;
            decimal after;
            if (forcedAfter.HasValue)
            {
                after = RoundQty(forcedAfter.Value);
            }
            else
            {
                after = type switch
                {
                    StockMovementType.In => before + quantity,
                    StockMovementType.Out => before - quantity,
                    StockMovementType.Reversal => before + quantity,
                    StockMovementType.Adjustment => before,
                    _ => before
                };
            }

            if (after < 0)
                throw new InvalidOperationException(
                    $"Stock insuficiente para '{item.Name}'. Disponible: {before} {item.Unit}.");

            item.QuantityOnHand = after;
            unitOfWork.InventoryItemsRepository.Update(item, cancellationToken);

            var movement = new StockMovement
            {
                InventoryItemId = item.Id,
                Type = type,
                Quantity = quantity,
                QuantityBefore = before,
                QuantityAfter = after,
                Reason = reason,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                ReversesMovementId = reversesMovementId,
                CreatedByUserId = recordedByUserId
            };

            await unitOfWork.StockMovementsRepository.AddAsync(movement, cancellationToken);
            return movement;
        }

        private static string NormalizeSku(string sku) => sku.Trim().ToLowerInvariant();

        private static string NormalizeUnit(string unit)
        {
            var value = string.IsNullOrWhiteSpace(unit) ? "u" : unit.Trim().ToLowerInvariant();
            return value switch
            {
                "unidad" or "unidades" or "piece" or "pcs" => "u",
                "milliliters" or "millilitre" or "ml." => "ml",
                "grams" or "gramo" or "gramos" => "g",
                _ => value
            };
        }

        private static decimal RoundQty(decimal value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }
}
