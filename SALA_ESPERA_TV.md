# Sala de espera (TV)

Pantalla de llamados en tiempo real con **SignalR**, pensada para un televisor en la sala de espera.

## Qué hace

- La TV muestra `/sala-espera` a pantalla completa.
- Recepción pulsa **Llamar paciente** en Agenda.
- La API publica un aviso al grupo `WaitingRoomScreens`.
- La TV muestra el nombre del paciente y del médico (opcionalmente con voz).

## Requisitos

- API y Admin en ejecución
- TV/PC en la **misma red local**
- Usuario con permiso `sala-espera.view` (Admin o Recepcionista)

## Pasos en la UI

1. En el PC servidor, anota la IP LAN (ej. `192.168.1.50`).
2. En la TV abre `http://IP:5200/login` e inicia sesión.
3. Entra a **Sala de espera** (menú lateral) o `http://IP:5200/sala-espera`.
4. Confirma estado **En vivo**.
5. Pulsa **Pantalla completa**.
6. Desde Agenda → **Llamar paciente**.

Sin dominio ni costes extra en LAN.
