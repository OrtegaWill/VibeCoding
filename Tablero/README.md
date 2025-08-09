# TaskBoard - Sistema de Gestión de Sprints

Una aplicación web full-stack inspirada en monday.com para la gestión de tareas y sprints de equipos de desarrollo.

## Tecnologías Utilizadas

- **Frontend**: Angular 18, TypeScript, CSS
- **Backend**: ASP.NET Core 7, C#
- **Base de Datos**: SQLite
- **Comunicación en tiempo real**: SignalR
- **Arquitectura**: API REST + WebSockets

## Características Principales

### 🎯 Gestión de Tareas
- Crear, editar y eliminar tareas
- Asignar tareas a usuarios
- Establecer prioridades (Baja, Media, Alta, Crítica)
- Estados de tareas: Backlog, Por Hacer, En Progreso, En Revisión, Completado

### 🏃‍♂️ Gestión de Sprints
- Crear y planificar sprints
- Iniciar y completar sprints
- Agregar tareas a sprints
- Mover tareas incompletas al backlog al finalizar sprint

### 📊 Vistas del Dashboard
- **Backlog**: Lista de tareas pendientes por asignar
- **Kanban**: Vista de tablero con estados de tareas
- **Sprint Activo**: Tareas del sprint en curso
- **Todos los Sprints**: Historial y planificación

### 💬 Sistema de Comentarios
- Agregar comentarios a tareas y sprints
- Seguimiento de la actividad del proyecto

### ⚡ Actualizaciones en Tiempo Real
- Notificaciones automáticas de cambios
- Sincronización entre usuarios mediante SignalR

## Estructura del Proyecto

```
Tablero/
├── TaskBoard.API/              # Backend ASP.NET Core
│   ├── Controllers/           # Controladores de la API
│   ├── Models/               # Modelos de datos
│   ├── Data/                 # DbContext y configuración BD
│   ├── Hubs/                 # SignalR Hubs
│   └── Program.cs            # Configuración de la aplicación
├── TaskBoard.Frontend/        # Frontend Angular
│   ├── src/app/components/   # Componentes de Angular
│   ├── src/app/services/     # Servicios para API y SignalR
│   ├── src/app/models/       # Modelos TypeScript
│   └── src/styles.css        # Estilos globales
└── README.md
```

## Instalación y Configuración

### Prerrequisitos
- .NET 7 SDK
- Node.js (versión 18 o superior)
- Angular CLI

### Backend (API)

1. Navegar al directorio del API:
   ```bash
   cd TaskBoard.API/TaskBoard.API
   ```

2. Restaurar paquetes:
   ```bash
   dotnet restore
   ```

3. Ejecutar la aplicación:
   ```bash
   dotnet run
   ```
   
   La API estará disponible en: `http://localhost:5000`

### Frontend (Angular)

1. Navegar al directorio del frontend:
   ```bash
   cd TaskBoard.Frontend
   ```

2. Instalar dependencias:
   ```bash
   npm install
   ```

3. Ejecutar la aplicación:
   ```bash
   ng serve
   ```
   
   La aplicación estará disponible en: `http://localhost:4200`

## API Endpoints

### Tareas
- `GET /api/tasks` - Obtener todas las tareas
- `GET /api/tasks/{id}` - Obtener una tarea específica
- `GET /api/tasks/backlog` - Obtener tareas del backlog
- `GET /api/tasks/sprint/{sprintId}` - Obtener tareas de un sprint
- `POST /api/tasks` - Crear nueva tarea
- `PUT /api/tasks/{id}` - Actualizar tarea
- `DELETE /api/tasks/{id}` - Eliminar tarea

### Sprints
- `GET /api/sprints` - Obtener todos los sprints
- `GET /api/sprints/{id}` - Obtener sprint específico
- `GET /api/sprints/active` - Obtener sprint activo
- `POST /api/sprints` - Crear nuevo sprint
- `PUT /api/sprints/{id}/start` - Iniciar sprint
- `PUT /api/sprints/{id}/complete` - Completar sprint

### Comentarios
- `GET /api/comments/task/{taskId}` - Comentarios de una tarea
- `GET /api/comments/sprint/{sprintId}` - Comentarios de un sprint
- `POST /api/comments/task/{taskId}` - Agregar comentario a tarea
- `POST /api/comments/sprint/{sprintId}` - Agregar comentario a sprint

## Base de Datos

La aplicación utiliza SQLite como base de datos local. La base de datos se crea automáticamente al ejecutar la aplicación por primera vez.

### Modelos Principales:
- **TaskItem**: Representación de las tareas
- **Sprint**: Representación de los sprints
- **Comment**: Sistema de comentarios

## Desarrollo

### Próximas Funcionalidades
- [ ] Implementar autenticación de usuarios
- [ ] Drag & Drop en vista Kanban
- [ ] Filtros y búsqueda avanzada
- [ ] Reportes y métricas de sprints
- [ ] Notificaciones push
- [ ] Exportación de datos

### Contribución
1. Fork del proyecto
2. Crear branch para nueva funcionalidad
3. Realizar cambios
4. Crear Pull Request

## Licencia

Este proyecto está bajo la Licencia MIT.
