# Diagrama de Arquitectura - Copilot Ticket Tracker

```
┌─────────────────────────────────────────────────────────────────────┐
│                           COPILOT TICKET TRACKER                   │
│                     (Sistema de Gestión de Tickets)                │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                               │
├─────────────────────────────────────────────────────────────────────┤
│  📊 Dashboard        🎫 Tickets        ⚙️ Settings        📧 Import  │
│  - Métricas          - Lista          - Filtros          - Excel    │
│  - Estadísticas      - Detalle        - Usuarios         - CSV      │
│  - Gráficos          - Comentarios    - Configuración    - Outlook  │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                              Blazor Server
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                             BACKEND                                 │
├─────────────────────────────────────────────────────────────────────┤
│  🔧 Services                    📡 Hubs                             │
│  - TicketManagerService         - TicketHub (SignalR)              │
│  - OutlookService              - Notificaciones en tiempo real      │
│  - EmailClassifierService                                           │
│  - FileImportService           🎯 Models                            │
│  - NotificationService         - Ticket                             │
│                                - TicketComment                       │
│                                - EmailFilter                         │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                            Entity Framework Core
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                             DATABASE                                │
├─────────────────────────────────────────────────────────────────────┤
│  🗄️ SQLite Database                                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐     │
│  │     Tickets     │  │ TicketComments  │  │  EmailFilters   │     │
│  │                 │  │                 │  │                 │     │
│  │ • Id            │  │ • Id            │  │ • Id            │     │
│  │ • EmailId       │  │ • TicketId      │  │ • Name          │     │
│  │ • Subject       │  │ • Content       │  │ • FromDomain    │     │
│  │ • Description   │  │ • Author        │  │ • Keywords      │     │
│  │ • Status        │  │ • CreatedDate   │  │ • AutoCategory  │     │
│  │ • Priority      │  │ • IsSystem      │  │ • AutoPriority  │     │
│  │ • AssignedTo    │  └─────────────────┘  │ • IsActive      │     │
│  │ • CreatedDate   │                       └─────────────────┘     │
│  │ • ... (30+ campos)                                               │
│  └─────────────────┘                                               │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                        INTEGRACIONES EXTERNAS                       │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  📧 Microsoft Outlook 365         📄 Archivos Excel/CSV            │
│  ┌─────────────────────────┐      ┌─────────────────────────┐       │
│  │  Microsoft Graph API    │      │     File Upload         │       │
│  │  - Autenticación OAuth2 │      │     - EPPlus (Excel)    │       │
│  │  - Lectura de emails    │      │     - CsvHelper (CSV)   │       │
│  │  - Filtros automáticos  │      │     - Mapeo automático  │       │
│  │  - Conversión a tickets │      │     - Validación datos  │       │
│  └─────────────────────────┘      └─────────────────────────┘       │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                            FLUJO DE DATOS                           │
└─────────────────────────────────────────────────────────────────────┘

📧 Email → 🔍 Filter → 🎫 Ticket → 👥 Assign → 🔧 Work → ✅ Resolve

1. EMAIL INGRESA
   - Usuario envía email a soporte
   - Outlook recibe y almacena

2. DETECCIÓN AUTOMÁTICA
   - Graph API consulta emails nuevos
   - EmailClassifierService analiza contenido
   - Aplica filtros y reglas automáticas

3. CREACIÓN DE TICKET
   - Convierte email en objeto Ticket
   - Asigna categoría, prioridad automáticamente
   - Guarda en base de datos SQLite

4. ASIGNACIÓN
   - Reglas automáticas asignan a grupos/usuarios
   - Notificaciones vía SignalR
   - Dashboard se actualiza en tiempo real

5. GESTIÓN Y SEGUIMIENTO
   - Usuario actualiza estado
   - Agrega comentarios y tiempo trabajado
   - Sistema registra historial completo

6. RESOLUCIÓN
   - Marca como resuelto
   - Documenta solución y causa raíz
   - Actualiza métricas y estadísticas

┌─────────────────────────────────────────────────────────────────────┐
│                          CARACTERÍSTICAS CLAVE                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ✅ AUTOMATIZACIÓN                                                  │
│     - Conversión automática email → ticket                          │
│     - Clasificación inteligente                                     │
│     - Asignación basada en reglas                                   │
│                                                                     │
│  ✅ TIEMPO REAL                                                     │
│     - Actualizaciones instantáneas (SignalR)                        │
│     - Notificaciones automáticas                                    │
│     - Dashboard dinámico                                             │
│                                                                     │
│  ✅ INTEGRACIÓN                                                     │
│     - Microsoft Graph API nativa                                    │
│     - Importación masiva de archivos                                │
│     - Compatibilidad con ecosistema Microsoft                       │
│                                                                     │
│  ✅ TRAZABILIDAD                                                    │
│     - Historial completo de cambios                                 │
│     - Métricas de tiempo y rendimiento                              │
│     - Análisis de causa raíz                                        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```