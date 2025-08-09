import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { SignalRService } from '../../services/signalr.service';
import { TaskItem, Sprint, TaskStatus, SprintStatus, TaskPriority } from '../../models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit {
  // Datos para métricas
  totalTasks = 0;
  tasksInProgress = 0;
  completedTasks = 0;
  activeSprints = 0;

  // Datos principales
  tasks: TaskItem[] = [];
  sprints: Sprint[] = [];
  recentTasks: TaskItem[] = [];
  recentSprints: Sprint[] = [];
  currentSprint: Sprint | null = null;

  // Estados de modales
  showCreateTaskModal = false;
  showCreateSprintModal = false;

  // Objetos para formularios
  newTask: Partial<TaskItem> = {
    title: '',
    description: '',
    status: TaskStatus.Todo,
    priority: TaskPriority.Medium,
    sprintId: null
  };

  newSprint: Partial<Sprint> = {
    name: '',
    goal: '',
    status: SprintStatus.Planned,
    startDate: new Date(),
    endDate: new Date()
  };

  constructor(
    private apiService: ApiService,
    private signalrService: SignalRService
  ) {}

  ngOnInit() {
    this.loadDashboardData();
    this.setupSignalRListeners();
  }

  async loadDashboardData() {
    try {
      // Cargar tareas
      this.apiService.getTasks().subscribe(tasks => {
        this.tasks = tasks;
        this.recentTasks = this.tasks
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 5);
        this.calculateMetrics();
      });

      // Cargar sprints
      this.apiService.getSprints().subscribe(sprints => {
        this.sprints = sprints;
        this.recentSprints = this.sprints
          .sort((a, b) => {
            const dateA = a.startDate ? new Date(a.startDate).getTime() : 0;
            const dateB = b.startDate ? new Date(b.startDate).getTime() : 0;
            return dateB - dateA;
          })
          .slice(0, 3);
        
        // Encontrar sprint actual
        this.currentSprint = this.sprints.find(s => s.status === SprintStatus.Active) || null;
        this.calculateMetrics();
      });

    } catch (error) {
      console.error('Error loading dashboard data:', error);
    }
  }

  calculateMetrics() {
    this.totalTasks = this.tasks.length;
    this.tasksInProgress = this.tasks.filter(t => 
      t.status === TaskStatus.InProgress || t.status === TaskStatus.Todo
    ).length;
    this.completedTasks = this.tasks.filter(t => t.status === TaskStatus.Done).length;
    this.activeSprints = this.sprints.filter(s => s.status === SprintStatus.Active).length;
  }

  getSprintProgress(): number {
    if (!this.currentSprint) return 0;
    
    const sprintTasks = this.tasks.filter(t => t.sprintId === this.currentSprint!.id);
    if (sprintTasks.length === 0) return 0;
    
    const completedTasks = sprintTasks.filter(t => t.status === TaskStatus.Done).length;
    return Math.round((completedTasks / sprintTasks.length) * 100);
  }

  async createTask() {
    if (!this.newTask.title) return;

    try {
      const taskToCreate = {
        ...this.newTask,
        createdAt: new Date(),
        updatedAt: new Date(),
        comments: []
      } as TaskItem;

      this.apiService.createTask(taskToCreate).subscribe(createdTask => {
        this.tasks.unshift(createdTask);
        this.recentTasks = this.tasks.slice(0, 5);
        this.calculateMetrics();

        // Notificar a otros usuarios a través de SignalR
        this.signalrService.broadcastTaskUpdate('default', createdTask);
      });

      // Reset form
      this.newTask = {
        title: '',
        description: '',
        priority: TaskPriority.Medium,
        status: TaskStatus.Backlog,
        sprintId: undefined
      };
      
      this.showCreateTaskModal = false;
      
    } catch (error) {
      console.error('Error creating task:', error);
      alert('Error al crear la tarea. Por favor, intenta de nuevo.');
    }
  }

  async createSprint() {
    if (!this.newSprint.name) return;

    try {
      const sprintToCreate = {
        ...this.newSprint,
        createdAt: new Date(),
        updatedAt: new Date(),
        tasks: [],
        comments: []
      } as Sprint;

      this.apiService.createSprint(sprintToCreate).subscribe(createdSprint => {
        this.sprints.unshift(createdSprint);
        this.recentSprints = this.sprints.slice(0, 3);
        this.calculateMetrics();

        // Notificar a otros usuarios a través de SignalR
        this.signalrService.broadcastSprintUpdate('default', createdSprint);
      });

      // Reset form
      this.newSprint = {
        name: '',
        goal: '',
        status: SprintStatus.Planned,
        startDate: new Date(),
        endDate: new Date()
      };
      
      this.showCreateSprintModal = false;
      
    } catch (error) {
      console.error('Error creating sprint:', error);
      alert('Error al crear el sprint. Por favor, intenta de nuevo.');
    }
  }

  editTask(task: TaskItem) {
    // TODO: Implementar modal de edición o navegar a vista de edición
    console.log('Edit task:', task);
  }

  private setupSignalRListeners() {
    // Iniciar conexión SignalR
    this.signalrService.startConnection();
    this.signalrService.joinProject('default');

    // Escuchar eventos de SignalR para actualizar datos en tiempo real
    this.signalrService.taskUpdated$.subscribe((task: TaskItem) => {
      const index = this.tasks.findIndex(t => t.id === task.id);
      if (index !== -1) {
        this.tasks[index] = task;
        this.recentTasks = this.tasks.slice(0, 5);
        this.calculateMetrics();
      } else {
        // Nueva tarea creada por otro usuario
        this.tasks.unshift(task);
        this.recentTasks = this.tasks.slice(0, 5);
        this.calculateMetrics();
      }
    });

    this.signalrService.sprintUpdated$.subscribe((sprint: Sprint) => {
      const index = this.sprints.findIndex(s => s.id === sprint.id);
      if (index !== -1) {
        this.sprints[index] = sprint;
        this.recentSprints = this.sprints.slice(0, 3);
        
        if (this.currentSprint && this.currentSprint.id === sprint.id) {
          this.currentSprint = sprint;
        }
        
        this.calculateMetrics();
      } else {
        // Nuevo sprint creado por otro usuario
        this.sprints.unshift(sprint);
        this.recentSprints = this.sprints.slice(0, 3);
        this.calculateMetrics();
      }
    });
  }
}
