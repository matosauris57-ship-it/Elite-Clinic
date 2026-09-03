using Clinic_System.Core.Dashboard;

namespace Clinic_System.Application.Tests.Common;

public class DashboardLayoutEngineTests
{
    [Fact]
    public void DefaultLayout_IncludesCurrentOperationalWidgetsAndSystemStatus()
    {
        var layout = DashboardWidgetCatalog.CreateDefaultLayout();
        var visible = DashboardLayoutEngine.VisibleItems(layout).Select(i => i.WidgetKey).ToList();

        visible.Should().Contain(DashboardWidgetKeys.AppointmentsTodayKpi);
        visible.Should().Contain(DashboardWidgetKeys.TodayAppointments);
        visible.Should().Contain(DashboardWidgetKeys.SystemStatus);
        visible.Should().NotContain(DashboardWidgetKeys.PatientsRegistered);
    }

    [Fact]
    public void FilterByPermissions_HidesFinancialWidgetsWithoutBillingAccess()
    {
        var layout = DashboardWidgetCatalog.CreateDefaultLayout();
        var filtered = DashboardLayoutEngine.FilterByPermissions(layout, permission =>
            permission is not "facturacion.view");

        DashboardLayoutEngine.VisibleItems(filtered)
            .Select(i => i.WidgetKey)
            .Should()
            .NotContain(DashboardWidgetCatalog.FinancialWidgetKeys);
        DashboardLayoutEngine.VisibleItems(filtered)
            .Select(i => i.WidgetKey)
            .Should()
            .Contain(DashboardWidgetKeys.SystemStatus);
    }

    [Fact]
    public void ClinicAvailability_HidesDisabledOptionalWidgets()
    {
        var clinic = DashboardWidgetCatalog.CreateDefaultLayout();
        DashboardLayoutEngine.SetVisible(clinic, DashboardWidgetKeys.DailyRevenueKpi, false);
        clinic = DashboardLayoutEngine.SetVisible(clinic, DashboardWidgetKeys.DailyRevenueKpi, false);

        var user = DashboardWidgetCatalog.CreateDefaultLayout();
        var merged = DashboardLayoutEngine.ApplyClinicAvailability(userLayout: user, clinic);

        merged.Items.First(i => i.WidgetKey == DashboardWidgetKeys.DailyRevenueKpi).Visible.Should().BeFalse();
        merged.Items.First(i => i.WidgetKey == DashboardWidgetKeys.SystemStatus).Visible.Should().BeTrue();
    }

    [Fact]
    public void Normalize_ClampsSizeAndKeepsUnknownKeysOut()
    {
        var layout = new DashboardLayoutDocument
        {
            Items =
            [
                new DashboardWidgetPlacement
                {
                    WidgetKey = DashboardWidgetKeys.AppointmentsTodayKpi,
                    X = 99,
                    W = 99,
                    H = 99,
                    Visible = true
                },
                new DashboardWidgetPlacement
                {
                    WidgetKey = "inventario-bajo",
                    Visible = true
                }
            ]
        };

        var normalized = DashboardLayoutEngine.Normalize(layout);
        var kpi = normalized.Items.First(i => i.WidgetKey == DashboardWidgetKeys.AppointmentsTodayKpi);
        kpi.W.Should().BeLessThanOrEqualTo(6);
        kpi.X.Should().BeInRange(0, 11);
        normalized.Items.Should().NotContain(i => i.WidgetKey == "inventario-bajo");
    }

    [Fact]
    public void Restore_UsesCatalogDefaults()
    {
        var mutated = DashboardWidgetCatalog.CreateDefaultLayout();
        mutated.Items.ForEach(i =>
        {
            i.X = 0;
            i.Y = 0;
            i.Visible = false;
        });

        var restored = DashboardLayoutEngine.Normalize(DashboardWidgetCatalog.CreateDefaultLayout());
        DashboardLayoutEngine.VisibleItems(restored).Should().NotBeEmpty();
        restored.Items.First(i => i.WidgetKey == DashboardWidgetKeys.AppointmentsTodayKpi).X.Should().Be(0);
    }
}
