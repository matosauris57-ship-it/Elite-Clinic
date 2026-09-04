# Confirmación de asistencia por enlace público

El sistema puede generar un enlace para que el paciente confirme si asistirá a su cita.

## Flujo

1. En Agenda o al crear una cita, se genera un token seguro.
2. El Admin arma un enlace público:
   - `/confirmar-asistencia?token=...`
3. El enlace se envía por:
   - WhatsApp manual (`wa.me`)
   - Correo SMTP
4. El paciente abre el enlace y responde:
   - **Sí, asistiré**
   - **No podré asistir**

## Cloudflare Tunnel

Para que el paciente pueda abrir el enlace desde fuera de la clínica, expón el Admin con Cloudflare Tunnel:

```bash
cloudflared tunnel --url http://localhost:5200
```

Cloudflare mostrará una URL pública, por ejemplo:

```text
https://nombre-random.trycloudflare.com
```

Configura esa URL en:

```json
"ApiSettings": {
  "ApiBaseUrl": "http://localhost:5129",
  "PublicBaseUrl": "https://nombre-random.trycloudflare.com"
}
```

Si `PublicBaseUrl` está vacío, el sistema usa la URL actual del Admin (útil para LAN).

## Seguridad

- El enlace usa token protegido por servidor.
- El paciente no necesita iniciar sesión.
- El token expira en 14 días.
- El token valida cita, paciente y fecha.
