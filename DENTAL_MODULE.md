# Módulo odontológico — Clínica Dental

Extensión del proyecto [Clinic-System](https://github.com/adhamdr1/Clinic-System) con funcionalidades para clínica odontológica.

## Requisitos

- .NET SDK 10
- SQL Server o LocalDB
- (Opcional) Redis para caché

## Configuración rápida

```powershell
cd "C:\Users\gaming\Clinic-System"
dotnet restore
cd "Clinic System.API"
dotnet ef database update
dotnet run
```

Swagger: `https://localhost:7179/swagger`

La cadena de conexión está en `Clinic System.API/appsettings.json` (`constr`), configurada para LocalDB por defecto.

## Nuevos endpoints API

| Módulo | Ruta base | Descripción |
|--------|-----------|-------------|
| Historial dental | `/api/dental/history` | Alergias, medicación, hábitos |
| Odontograma | `/api/dental/odontogram` | Estado por pieza (FDI 11-48) |
| Tratamientos | `/api/dental/treatments` | Procedimientos vinculados a citas |
| Presupuestos | `/api/dental/treatment-plans` | Planes de tratamiento con ítems |
| Facturación | `/api/dental/invoices` | Líneas de factura por pago |

## Entidades añadidas

- `DentalHistory` — historial clínico odontológico (1:1 con paciente)
- `ToothRecord` — odontograma por pieza dental
- `DentalTreatment` — tratamientos con costo y cita opcional
- `TreatmentPlan` + `PlanItem` — presupuestos odontológicos
- `InvoiceLine` — líneas de factura en pagos

## Fork en GitHub

Para guardar tus cambios en tu cuenta:

1. Abre https://github.com/adhamdr1/Clinic-System
2. Pulsa **Fork**
3. En tu máquina: `git remote set-url origin https://github.com/TU_USUARIO/Clinic-System.git`
4. Commit y push de los cambios
