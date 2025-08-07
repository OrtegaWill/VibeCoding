# Sistema de Gestión de Tickets - Integración con Outlook 365

Una aplicación completa de gestión de tickets que se integra con Microsoft Outlook 365 para automatizar la gestión de tickets de soporte, transformando correos electrónicos en un sistema de seguimiento de trabajo estructurado.

## 🚀 Características Principales

### 1. Integración con Outlook 365
- **Conexión con Microsoft Graph API**: Autenticación OAuth 2.0 con Azure Active Directory
- **Procesamiento automático de correos**: Filtrado y clasificación inteligente de emails
- **Sincronización en tiempo real**: Actualización automática de tickets desde correos

### 2. Gestión de Tickets
- **Estados de workflow**: Backlog, En Progreso, En Revisión, Resuelto, Bloqueado
- **Clasificación inteligente**: Prioridad, categoría y asignación automática
- **Historial completo**: Seguimiento de todos los cambios y comunicaciones
- **Sistema de comentarios**: Comunicación interna y externa

### 3. Interfaz de Usuario
- **Dashboard interactivo**: Estadísticas y métricas en tiempo real
- **Vista de tickets**: Filtros avanzados y búsqueda
- **Gestión visual**: Kanban board para gestión de tickets
- **Notificaciones**: Alertas para tickets críticos

## 🛠️ Tecnologías Utilizadas

- **Backend**: ASP.NET Core 7.0, Entity Framework Core
- **Frontend**: Blazor Server
- **Base de Datos**: SQL Server
- **Integración**: Microsoft Graph API, Microsoft Identity Web
- **Comunicación en tiempo real**: SignalR

## 📋 Requisitos Previos

- .NET 7.0 SDK
- SQL Server (LocalDB, SQL Server Express, o Azure SQL)
- Cuenta de Microsoft 365 con acceso a Outlook
- Aplicación registrada en Azure Active Directory

## 🔧 Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone <repository-url>
cd TicketManagementApp
```

### 2. Configurar la Base de Datos

```bash
# Restaurar dependencias
dotnet restore

# Crear la base de datos
cd TicketManagement.API
dotnet ef database update
```

### 3. Configurar Azure Active Directory

1. **Registrar la aplicación en Azure AD**:
   - Ve a [Azure Portal](https://portal.azure.com)
   - Navega a Azure Active Directory > Registros de aplicaciones
   - Crea una nueva aplicación
   - Configura los permisos de Microsoft Graph API:
     - `Mail.Read`
     - `Mail.ReadWrite`
     - `Mail.Send`

2. **Configurar appsettings.json**:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "tu-dominio.onmicrosoft.com",
       "TenantId": "tu-tenant-id",
       "ClientId": "tu-client-id",
       "ClientSecret": "tu-client-secret"
     }
   }
   ```

### 4. Configurar la Cadena de Conexión

Edita `TicketManagement.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TicketManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 5. Ejecutar la Aplicación

```bash
# Ejecutar la API
cd TicketManagement.API
dotnet run

# En otra terminal, ejecutar la aplicación web
cd TicketManagement.Web
dotnet run
```

## 🎯 Uso de la Aplicación

### Dashboard Principal
- **Estadísticas**: Total de tickets, resueltos, en progreso, vencidos
- **Gráficos**: Distribución de tickets por estado
- **Tickets recientes**: Lista de los últimos tickets creados

### Gestión de Tickets
1. **Ver tickets**: Navega a `/tickets` para ver todos los tickets
2. **Filtrar**: Usa los filtros por estado, prioridad, categoría
3. **Buscar**: Utiliza la búsqueda para encontrar tickets específicos
4. **Crear ticket**: Haz clic en "New Ticket" para crear manualmente
5. **Editar**: Haz clic en el icono de editar para modificar un ticket

### Procesamiento de Emails
1. **Configurar filtros**: Define reglas para procesar emails automáticamente
2. **Procesar emails**: Ejecuta el procesamiento manual desde la API
3. **Crear tickets**: Los emails se convierten automáticamente en tickets

## 🔌 API Endpoints

### Tickets
- `GET /api/tickets` - Obtener todos los tickets
- `GET /api/tickets/{id}` - Obtener ticket por ID
- `GET /api/tickets/status/{status}` - Filtrar por estado
- `GET /api/tickets/priority/{priority}` - Filtrar por prioridad
- `POST /api/tickets` - Crear nuevo ticket
- `PUT /api/tickets/{id}` - Actualizar ticket
- `DELETE /api/tickets/{id}` - Eliminar ticket

### Emails
- `GET /api/email/unread` - Obtener emails no leídos
- `POST /api/email/process` - Procesar filtros de email
- `POST /api/email/create-ticket` - Crear ticket desde email

## 📊 Estados de Tickets

- **Backlog**: Tickets nuevos sin asignar
- **InProgress**: Tickets asignados en desarrollo
- **InReview**: Tickets completados pendientes de validación
- **Resolved**: Tickets cerrados exitosamente
- **Blocked**: Tickets con dependencias pendientes
- **Closed**: Tickets cerrados definitivamente

## 🎨 Personalización

### Filtros de Email
Puedes configurar filtros personalizados para procesar emails automáticamente:

```csharp
var filter = new EmailFilter
{
    Name = "Support Requests",
    SubjectContains = "support",
    DefaultPriority = TicketPriority.Medium,
    DefaultCategory = TicketCategory.Support,
    DefaultAssignee = "support@company.com"
};
```

### Categorías de Tickets
- **General**: Tickets generales
- **Technical**: Problemas técnicos
- **Billing**: Problemas de facturación
- **Feature**: Solicitudes de nuevas funcionalidades
- **Bug**: Reportes de errores
- **Support**: Solicitudes de soporte

## 🔒 Seguridad

- **Autenticación**: OAuth 2.0 con Azure AD
- **Autorización**: Control de acceso basado en roles
- **Encriptación**: Datos sensibles encriptados
- **Auditoría**: Historial completo de cambios

## 🚀 Despliegue

### Azure
1. Publicar la API en Azure App Service
2. Configurar Azure SQL Database
3. Configurar Azure AD para autenticación
4. Configurar variables de entorno

### Docker
```bash
# Construir imagen
docker build -t ticket-management .

# Ejecutar contenedor
docker run -p 8080:80 ticket-management
```

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📞 Soporte

Para soporte técnico o preguntas:
- Crear un issue en GitHub
- Contactar al equipo de desarrollo
- Revisar la documentación de la API

## 🔄 Actualizaciones

### v1.0.0
- Integración inicial con Outlook 365
- Sistema básico de gestión de tickets
- Dashboard con estadísticas
- API REST completa

### Próximas Funcionalidades
- Notificaciones push
- Reportes avanzados
- Integración con Slack/Teams
- Automatización con Power Automate 