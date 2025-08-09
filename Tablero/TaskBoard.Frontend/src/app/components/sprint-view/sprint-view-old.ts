import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskItem, TaskStatus, TaskPriority, Sprint, SprintStatus, Comment } from '../../models/models';
import { ApiService } from '../../services/api.service';
import { SignalRService } from '../../services/signalr.service';
import { Subscription, lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-sprint-view',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sprint-view.html',
  styleUrls: ['./sprint-view.css']
})
export class SprintViewComponent implements OnInit, OnDestroy {
  // Services
  private apiService = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private signalrService = inject(SignalRService);

  // Component state
  sprint: Sprint | null = null;
  sprints: Sprint[] = [];
  sprintTasks: TaskItem[] = [];
  selectedSprintId: number | null = null;
  
  // Task management
  selectedTask: TaskItem | null = null;
  editingTaskId: number | null = null;
  showTaskForm: boolean = false;
  showCreateTaskModal: boolean = false;
  showEditSprintModal: boolean = false;
  showCommentSection: boolean = false;
  
  // Kanban columns
  todoTasks: TaskItem[] = [];
  inProgressTasks: TaskItem[] = [];
  doneTasks: TaskItem[] = [];
  reviewTasks: TaskItem[] = [];
  
  // Sprint management
  isEditing: boolean = false;
  originalSprint: Sprint | null = null;
  
  // Comment system
  newComment: Partial<Comment> = { content: '', author: 'Usuario' };
  sprintComments: Comment[] = [];
  
  // Form objects
  newTask: Partial<TaskItem> = {
    title: '',
    description: '',
    priority: TaskPriority.Medium,
    status: TaskStatus.Todo,
    assignedTo: '',
    sprintId: null
  };
  
  // Metrics
  sprintProgress = 0;
  completedStoryPoints = 0;
  totalStoryPoints = 0;
  completedTasks = 0;
  totalTasks = 0;
  
  // Enums for templates
  TaskStatus = TaskStatus;
  TaskPriority = TaskPriority;
  SprintStatus = SprintStatus;
  
  private subscription = new Subscription();

  ngOnInit() {
    this.loadSprints();
    this.setupSignalRListeners();
    
    // Watch for route parameter changes
    this.subscription.add(
      this.route.params.subscribe(params => {
        const sprintId = params['id'];
        if (sprintId) {
          this.selectedSprintId = parseInt(sprintId);
          this.loadSprintData(this.selectedSprintId);
        }
      })
    );
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  async loadSprints() {
    try {
      this.sprints = await lastValueFrom(this.apiService.getSprints());
    } catch (error) {
      console.error('Error loading sprints:', error);
    }
  }

  async loadSprintData(sprintId: number) {
    try {
      this.sprint = await lastValueFrom(this.apiService.getSprint(sprintId));
      this.sprintTasks = await lastValueFrom(this.apiService.getTasksBySprint(sprintId));
      this.sprintComments = await lastValueFrom(this.apiService.getSprintComments(sprintId));
      this.organizeTasks();
      this.calculateMetrics();
    } catch (error) {
      console.error('Error loading sprint data:', error);
    }
  }

  onSprintChange() {
    if (this.selectedSprintId) {
      this.router.navigate(['/sprint', this.selectedSprintId]);
    }
  }

  organizeTasks() {
    this.todoTasks = this.sprintTasks.filter(task => task.status === TaskStatus.Todo);
    this.inProgressTasks = this.sprintTasks.filter(task => task.status === TaskStatus.InProgress);
    this.reviewTasks = this.sprintTasks.filter(task => task.status === TaskStatus.Review);
    this.doneTasks = this.sprintTasks.filter(task => task.status === TaskStatus.Done);
  }

  calculateMetrics() {
    this.totalTasks = this.sprintTasks.length;
    this.completedTasks = this.doneTasks.length;
    this.sprintProgress = this.totalTasks > 0 ? (this.completedTasks / this.totalTasks) * 100 : 0;
    
    // Story points calculation (assuming each task has 1 story point for now)
    this.totalStoryPoints = this.totalTasks;
    this.completedStoryPoints = this.completedTasks;
  }

  async updateTaskStatus(task: TaskItem, newStatus: TaskStatus) {
    try {
      const updatedTask = { ...task, status: newStatus, updatedAt: new Date() };
      await lastValueFrom(this.apiService.updateTask(updatedTask));
      
      // Update local state
      const index = this.sprintTasks.findIndex(t => t.id === task.id);
      if (index !== -1) {
        this.sprintTasks[index] = updatedTask;
        this.organizeTasks();
        this.calculateMetrics();
      }
      
      // Broadcast update via SignalR
      this.signalrService.broadcastTaskUpdate('default', updatedTask);
    } catch (error) {
      console.error('Error updating task status:', error);
      alert('Error al actualizar el estado de la tarea');
    }
  }

  async startSprint() {
    if (!this.sprint || this.sprint.status === SprintStatus.Active) return;
    
    try {
      const updatedSprint = {
        ...this.sprint,
        status: SprintStatus.Active,
        startDate: new Date(),
        updatedAt: new Date()
      };
      
      await lastValueFrom(this.apiService.updateSprint(updatedSprint));
      this.sprint = updatedSprint;
      
      this.signalrService.broadcastSprintUpdate('default', updatedSprint);
      alert('Sprint iniciado exitosamente');
    } catch (error) {
      console.error('Error starting sprint:', error);
      alert('Error al iniciar el sprint');
    }
  }

  async createTask() {
    if (!this.newTask.title?.trim() || !this.sprint) return;
    
    try {
      const taskToCreate = {
        ...this.newTask,
        sprintId: this.sprint.id,
        createdAt: new Date(),
        updatedAt: new Date()
      };
      
      const createdTask = await lastValueFrom(this.apiService.createTask(taskToCreate as TaskItem));
      this.sprintTasks.push(createdTask);
      this.organizeTasks();
      this.calculateMetrics();
      
      // Reset form
      this.newTask = {
        title: '',
        description: '',
        priority: TaskPriority.Medium,
        status: TaskStatus.Todo,
        assignedTo: '',
        sprintId: this.sprint.id
      };
      
      this.showCreateTaskModal = false;
      this.signalrService.broadcastTaskUpdate('default', createdTask);
      
    } catch (error) {
      console.error('Error creating task:', error);
      alert('Error al crear la tarea');
    }
  }

  async updateSprint() {
    if (!this.sprint) return;
    
    try {
      const updatedSprint = {
        ...this.sprint,
        updatedAt: new Date()
      };
      
      await lastValueFrom(this.apiService.updateSprint(updatedSprint));
      this.signalrService.broadcastSprintUpdate('default', updatedSprint);
      this.showEditSprintModal = false;
      
      alert('Sprint actualizado exitosamente');
    } catch (error) {
      console.error('Error updating sprint:', error);
      alert('Error al actualizar el sprint');
    }
  }

  async addComment() {
    if (!this.newComment.content?.trim() || !this.sprint) return;
    
    try {
      const commentToCreate = {
        ...this.newComment,
        sprintId: this.sprint.id,
        createdAt: new Date()
      };
      
      const createdComment = await lastValueFrom(this.apiService.createComment(commentToCreate as Comment));
      this.sprintComments.push(createdComment);
      this.newComment = { content: '', author: 'Usuario' };
      
      this.signalrService.broadcastCommentAdded('default', createdComment);
    } catch (error) {
      console.error('Error adding comment:', error);
      alert('Error al agregar comentario');
    }
  }

  private setupSignalRListeners() {
    this.subscription.add(
      this.signalrService.taskUpdated$.subscribe(updatedTask => {
        if (updatedTask.sprintId === this.sprint?.id) {
          const index = this.sprintTasks.findIndex(t => t.id === updatedTask.id);
          if (index !== -1) {
            this.sprintTasks[index] = updatedTask;
            this.organizeTasks();
            this.calculateMetrics();
          }
        }
      })
    );

    this.subscription.add(
      this.signalrService.sprintUpdated$.subscribe(updatedSprint => {
        if (updatedSprint.id === this.sprint?.id) {
          this.sprint = updatedSprint;
        }
      })
    );

    this.subscription.add(
      this.signalrService.commentAdded$.subscribe(newComment => {
        if (newComment.sprintId === this.sprint?.id) {
          this.sprintComments.push(newComment);
        }
      })
    );
  }

  getStatusColor(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Todo: return '#6c757d';
      case TaskStatus.InProgress: return '#007bff';
      case TaskStatus.Review: return '#ffc107';
      case TaskStatus.Done: return '#28a745';
      default: return '#6c757d';
    }
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Critical: return '🔴';
      case TaskPriority.High: return '🟠';
      case TaskPriority.Medium: return '🟡';
      case TaskPriority.Low: return '🟢';
      default: return '⚪';
    }
  }

  get completionPercentage(): number {
    return this.sprintProgress;
  }

  // Additional helper methods
  getTasksByStatus(status: TaskStatus): TaskItem[] {
    return this.sprintTasks.filter(task => task.status === status);
  }

  getSprintStatusClass(status: SprintStatus): string {
    switch (status) {
      case SprintStatus.Planned: return 'status-planning';
      case SprintStatus.Active: return 'status-active';
      case SprintStatus.Completed: return 'status-completed';
      case SprintStatus.Cancelled: return 'status-cancelled';
      default: return 'status-default';
    }
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Critical: return 'priority-critical';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.Low: return 'priority-low';
      default: return 'priority-default';
    }
  }

  async deleteTask(task: TaskItem) {
    if (!confirm('¿Estás seguro de que quieres eliminar esta tarea?') || !task.id) return;
    
    try {
      await lastValueFrom(this.apiService.deleteTask(task.id));
      this.sprintTasks = this.sprintTasks.filter(t => t.id !== task.id);
      this.organizeTasks();
      this.calculateMetrics();
      
      alert('Tarea eliminada exitosamente');
    } catch (error) {
      console.error('Error deleting task:', error);
      alert('Error al eliminar la tarea');
    }
  }

  async completeSprint() {
    if (!this.sprint || this.sprint.status === SprintStatus.Completed) return;
    
    try {
      const updatedSprint = {
        ...this.sprint,
        status: SprintStatus.Completed,
        endDate: new Date(),
        updatedAt: new Date()
      };
      
      await lastValueFrom(this.apiService.updateSprint(updatedSprint));
      this.sprint = updatedSprint;
      
      this.signalrService.broadcastSprintUpdate('default', updatedSprint);
      alert('Sprint completado exitosamente');
    } catch (error) {
      console.error('Error completing sprint:', error);
      alert('Error al completar el sprint');
    }
  }
}
