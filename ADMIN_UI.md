# DentalCare Admin UI

Panel de administración Blazor Server para la clínica odontológica, basado en el diseño Figma "DentalCare Admin".

## Requisitos

- .NET 10 SDK
- SQL Server LocalDB (para la API)
- API en ejecución en `http://localhost:5129`

## Ejecución en paralelo

Abre **dos terminales**:

**Terminal 1 — API:**
```powershell
cd "C:\Users\gaming\Clinic-System\Clinic System.API"
dotnet run
```

**Terminal 2 — Admin UI:**
```powershell
cd "C:\Users\gaming\Clinic-System\Clinic System.Admin"
dotnet run
```

Abre el navegador en: **http://localhost:5200**

## Credenciales de prueba

| Campo | Valor |
|-------|-------|
| Email | `admin@clinic.com` |
| Contraseña | `Admin@123` |

## Visual Studio (recomendado)

El repositorio incluye el perfil compartido [`Elite Clinic.slnLaunch`](Elite Clinic.slnLaunch) para iniciar API y Admin juntos.

**Requisitos:** Visual Studio 2022 **17.11+** con *Herramientas → Opciones → Características en vista previa → Enable Multi-Project Launch Profiles* activado.

1. Abre `Elite Clinic.slnx`
2. En la barra de herramientas, selecciona el perfil **API + Admin**
3. Pulsa **F5** (la API arranca primero en `http://localhost:5129`, luego el Admin en `http://localhost:5200`)

**Alternativa manual:** clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio** → marca **Elite Clinic** y **DentalCare.Admin** como **Iniciar**.

## Configuración

La URL de la API se define en `Clinic System.Admin/appsettings.json`:

```json
"ApiSettings": {
  "ApiBaseUrl": "http://localhost:5129"
}
```

## Fase 1 (implementado)

- Login con JWT (`POST /api/authentication/login`)
- Layout oscuro: sidebar + topbar
- Dashboard con 8 tarjetas KPI
- Alertas inteligentes desde stats de citas
- Menú lateral con páginas placeholder "Próximamente"

## Endpoints consumidos

| Endpoint | Uso |
|----------|-----|
| `POST /api/authentication/login` | Autenticación |
| `GET /api/appointments/stats` | KPIs de citas |
| `GET /api/payment/daily-revenue` | Ingresos del día |
| `GET /api/patients` | Nuevos pacientes |
| `GET /api/payment/list` | Ingresos del mes (parcial) |
