using Clinic_System.Core.Authorization;
using Clinic_System.Core.Entities;
using Clinic_System.Core.Enums;
using Clinic_System.Data.Context;
using Clinic_System.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Clinic_System.Data.Seed
{
    public static class DevDataSeeder
    {
        private static async Task SeedMedicalConditionsAsync(IServiceProvider services, ILogger logger)
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (await db.MedicalConditions.AnyAsync())
                return;

            var conditions = new[]
            {
                ("Diabetes", "Sistémica", 1),
                ("Hipertensión", "Sistémica", 2),
                ("Epilepsia", "Sistémica", 3),
                ("Asma", "Sistémica", 4),
                ("Alergia a medicamentos", "Alergia", 5),
                ("Alergia al látex", "Alergia", 6),
                ("Cardiopatía", "Sistémica", 7),
                ("Embarazo", "Otra", 8)
            };

            foreach (var (name, category, order) in conditions)
            {
                db.MedicalConditions.Add(new MedicalCondition
                {
                    Name = name,
                    Category = category,
                    SortOrder = order,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded medical conditions catalog");
        }

        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DevDataSeeder");

            foreach (var role in new[] { "Admin", "Doctor", "Patient" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role {Role}", role);
                }
            }

            const string adminEmail = "admin@clinic.com";
            const string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation("Seeded admin user {Email}", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to seed admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            await SeedDemoDoctorsAsync(services, userManager, logger);
            await SeedDemoPatientsAsync(services, userManager, logger);
            await SeedMedicalConditionsAsync(services, logger);
            await SeedStaffRolesAsync(roleManager, logger);
            await SeedDemoStaffUserAsync(userManager, logger);
        }

        private static async Task SeedDemoDoctorsAsync(
            IServiceProvider services,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (await db.Doctors.AnyAsync())
                return;

            var doctors = new[]
            {
                ("Dra. López", "Ortodoncia", "doctor.lopez@clinic.com"),
                ("Dr. Flores", "Cirugía", "doctor.flores@clinic.com"),
                ("Dr. Rodríguez", "Director", "doctor.rodriguez@clinic.com"),
                ("Dra. Martínez", "Endodoncia", "doctor.martinez@clinic.com"),
                ("Dr. García", "Implantología", "doctor.garcia@clinic.com")
            };

            foreach (var (name, spec, email) in doctors)
            {
                if (await userManager.FindByEmailAsync(email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var created = await userManager.CreateAsync(user, "Doctor@123");
                if (!created.Succeeded)
                {
                    logger.LogWarning("Failed to seed doctor {Email}: {Errors}", email,
                        string.Join(", ", created.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, "Doctor");
                db.Doctors.Add(new Doctor
                {
                    FullName = name,
                    Gender = name.Contains("Dra") ? Gender.Female : Gender.Male,
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Address = "Clínica DentalCare",
                    Phone = "+52 555 100 0001",
                    Specialization = spec,
                    ApplicationUserId = user.Id
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo doctors");
        }

        private static async Task SeedDemoPatientsAsync(
            IServiceProvider services,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (await db.Patients.AnyAsync())
                return;

            var patients = new[]
            {
                ("María García López", Gender.Female, "patient.maria@clinic.com", "+52 555 200 0001"),
                ("Carlos Mendoza Ruiz", Gender.Male, "patient.carlos@clinic.com", "+52 555 200 0002"),
                ("Ana Ramírez", Gender.Female, "patient.ana@clinic.com", "+52 555 200 0003"),
                ("José Morales", Gender.Male, "patient.jose@clinic.com", "+52 555 200 0004")
            };

            foreach (var (name, gender, email, phone) in patients)
            {
                if (await userManager.FindByEmailAsync(email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var created = await userManager.CreateAsync(user, "Patient@123");
                if (!created.Succeeded)
                {
                    logger.LogWarning("Failed to seed patient {Email}: {Errors}", email,
                        string.Join(", ", created.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, "Patient");
                db.Patients.Add(new Patient
                {
                    FullName = name,
                    Gender = gender,
                    DateOfBirth = new DateTime(1990, 3, 20),
                    Address = "Ciudad de México",
                    Phone = phone,
                    ApplicationUserId = user.Id
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo patients");
        }

        private static async Task SeedStaffRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            await EnsureRoleWithPermissionsAsync(roleManager, "Recepcionista",
            [
                "agendar-cita.view", "agendar-cita.create",
                "agenda.view", "agenda.edit",
                "sala-espera.view",
                "pacientes.view", "enfermedades.view",
                "campanas.view", "campanas.create", "campanas.edit"
            ], logger);

            await EnsureRoleWithPermissionsAsync(roleManager, "Contabilidad",
            [
                "dashboard.view",
                "facturacion.view", "facturacion.create", "facturacion.edit",
                "reportes.view"
            ], logger);
        }

        private static async Task EnsureRoleWithPermissionsAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            IEnumerable<string> permissions,
            ILogger logger)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Created staff role {Role}", roleName);
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
                return;

            var existing = await roleManager.GetClaimsAsync(role);
            foreach (var claim in existing.Where(c => c.Type == AdminPermissionCatalog.ClaimType))
                await roleManager.RemoveClaimAsync(role, claim);

            foreach (var permission in permissions.Where(AdminPermissionCatalog.IsValid))
                await roleManager.AddClaimAsync(role, new Claim(AdminPermissionCatalog.ClaimType, permission));
        }

        private static async Task SeedDemoStaffUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            const string email = "recepcion@clinic.com";
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(user, "Recepcion@123");
            if (!created.Succeeded)
            {
                logger.LogWarning("Failed to seed staff user {Email}: {Errors}", email,
                    string.Join(", ", created.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, "Recepcionista");
            logger.LogInformation("Seeded staff user {Email} with role Recepcionista", email);
        }
    }
}
