import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { SignalRService } from '../../services/signalr.service';
import { TaskItem, Sprint, TaskStatus, TaskPriority, SprintStatus } from '../../models/models';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './kanban-board.html',
  styleUrl: './kanban-board.css'
})
export class KanbanBoardComponent implements OnInit {
  tasks: TaskItem[] = [];
  sprints: Sprint[] = [];
  selectedSprint: Sprint | null = null;
  
  // Columnas del tablero Kanban
  kanbanColumns = [
    { status: TaskStatus.Backlog, title: 'Backlog', color: '#6c757d' },
    { status: TaskStatus.Todo, title: 'Por Hacer', color: '#007bff' },
    { status: TaskStatus.InProgress, title: 'En Progreso', color: '#ffc107' },
    { status: TaskStatus.Review, title: 'En Revisión', color: '#fd7e14' },
    { status: TaskStatus.Done, title: 'Completado', color: '#28a745' }
  ];

  // Control de drag and drop
  draggedTask: TaskItem | null = null;

  // Modal para crear tarea
  showCreateTaskModal = false;
  newTask: Partial<TaskItem> = {
    title: '',
    description: '',
    priority: TaskPriority.Medium,
    status: TaskStatus.Todo,
    sprintId: undefined
  };

  constructor(
    private apiService: ApiService,
    private signalrService: SignalRService
  ) {}

  ngOnInit() {
    this.loadKanbanData();
    this.setupSignalRListeners();
  }

  async loadKanbanData() {
    try {
      // Cargar sprints
      this.apiService.getSprints().subscribe(sprints => {
        this.sprints = sprints.filter(s => s.status === SprintStatus.Active || s.status === SprintStatus.Planned);
        // Seleccionar el primer sprint activo o el primero disponible
        this.selectedSprint = this.sprints.find(s => s.status === SprintStatus.Active) || this.sprints[0] || null;
        this.loadTasksForSprint();
      });
    } catch (error) {
      console.error('Error loading kanban data:', error);
    }
  }

  loadTasksForSprint() {
    this.apiService.getTasks().subscribe(allTasks => {
      if (this.selectedSprint) {
        this.tasks = allTasks.filter(task => task.sprintId === this.selectedSprint?.id);
      } else {
        // Si no hay sprint seleccionado, mostrar tareas sin asignar
        this.tasks = allTasks.filter(task => !task.sprintId);
      }
    });
  }

  getTasksForStatus(status: TaskStatus): TaskItem[] {
    return this.tasks.filter(task => task.status === status);
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Critical: return 'priority-critical';
      default: return 'priority-medium';
    }
  }

  onSprintChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    const sprintId = target.value ? parseInt(target.value) : null;
    this.selectedSprint = this.sprints.find(s => s.id === sprintId) || null;
    this.loadTasksForSprint();
  }

  // Funcionalidad Drag and Drop
  onDragStart(event: DragEvent, task: TaskItem) {
    this.draggedTask = task;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/html', '');
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, newStatus: TaskStatus) {
    event.preventDefault();
    
    if (this.draggedTask && this.draggedTask.status !== newStatus) {
      const updatedTask = { ...this.draggedTask, status: newStatus, updatedAt: new Date() };
      
      // Actualizar tarea en el servidor
      this.apiService.updateTask(updatedTask).subscribe(
        (updated) => {
          // Actualizar tarea localmente
          const taskIndex = this.tasks.findIndex(t => t.id === updated.id);
          if (taskIndex !== -1) {
            this.tasks[taskIndex] = updated;
          }
          
          // Notificar cambio a través de SignalR
          this.signalrService.broadcastTaskUpdate('default', updated);
        },
        (error) => {
          console.error('Error updating task:', error);
          alert('Error al actualizar la tarea. Por favor, intenta de nuevo.');
        }
      );
    }
    
    this.draggedTask = null;
  }

  // Gestión de tareas
  async createTask() {
    if (!this.newTask.title) return;

    try {
      const taskToCreate = {
        ...this.newTask,
        sprintId: this.selectedSprint?.id || undefined,
        createdAt: new Date(),
        updatedAt: new Date(),
        comments: []
      } as TaskItem;

      this.apiService.createTask(taskToCreate).subscribe(createdTask => {
        this.tasks.unshift(createdTask);
        this.signalrService.broadcastTaskUpdate('default', createdTask);
      });

      // Reset form
      this.newTask = {
        title: '',
        description: '',
        priority: TaskPriority.Medium,
        status: TaskStatus.Todo,
        sprintId: this.selectedSprint?.id || undefined
      };
      
      this.showCreateTaskModal = false;
      
    } catch (error) {
      console.error('Error creating task:', error);
      alert('Error al crear la tarea. Por favor, intenta de nuevo.');
    }
  }

  editTask(task: TaskItem) {
    // TODO: Implementar modal de edición
    console.log('Edit task:', task);
  }

  deleteTask(task: TaskItem) {
    if (confirm('¿Estás seguro de que quieres eliminar esta tarea?')) {
      this.apiService.deleteTask(task.id!).subscribe(
        () => {
          this.tasks = this.tasks.filter(t => t.id !== task.id);
          // TODO: Notificar eliminación a través de SignalR
        },
        (error) => {
          console.error('Error deleting task:', error);
          alert('Error al eliminar la tarea. Por favor, intenta de nuevo.');
        }
      );
    }
  }

  private setupSignalRListeners() {
    // Iniciar conexión SignalR si no está ya iniciada
    this.signalrService.startConnection();
    this.signalrService.joinProject('default');

    // Escuchar actualizaciones de tareas
    this.signalrService.taskUpdated$.subscribe((task: TaskItem) => {
      const index = this.tasks.findIndex(t => t.id === task.id);
      if (index !== -1) {
        this.tasks[index] = task;
      } else {
        // Nueva tarea creada por otro usuario
        if (!this.selectedSprint || task.sprintId === this.selectedSprint.id) {
          this.tasks.unshift(task);
        }
      }
    });

    // Escuchar actualizaciones de sprints
    this.signalrService.sprintUpdated$.subscribe((sprint: Sprint) => {
      const index = this.sprints.findIndex(s => s.id === sprint.id);
      if (index !== -1) {
        this.sprints[index] = sprint;
        if (this.selectedSprint && this.selectedSprint.id === sprint.id) {
          this.selectedSprint = sprint;
        }
      } else {
        this.sprints.push(sprint);
      }
    });
  }
}
