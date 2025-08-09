import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Observable, Subscription, firstValueFrom } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { SignalRService } from '../../services/signalr.service';
import { TaskItem, Sprint, TaskStatus, TaskPriority, SprintStatus } from '../../models/models';

@Component({
  selector: 'app-backlog',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './backlog.html',
  styleUrl: './backlog.css'
})
export class BacklogComponent implements OnInit, OnDestroy {
  // Datos
  backlogTasks: TaskItem[] = [];
  sprints: Sprint[] = [];
  selectedTasks: TaskItem[] = [];
  filteredTasks: TaskItem[] = [];
  
  // Filtros
  currentFilter: string = 'all';
  currentSortBy: string = 'priority';
  searchTerm: string = '';
  selectedPriority: string = '';
  selectedAssignee: string = '';
  
  // Estado del componente
  showCreateModal: boolean = false;
  editingTask: TaskItem | null = null;
  
  // Formulario para nueva tarea
  newTask: Partial<TaskItem> = {
    title: '',
    description: '',
    priority: TaskPriority.Medium,
    status: TaskStatus.Backlog,
    assignedTo: ''
  };
  
  // Enums para el template
  TaskStatus = TaskStatus;
  TaskPriority = TaskPriority;
  SprintStatus = SprintStatus;
  
  // Suscripciones
  private subscription = new Subscription();

  constructor(
    private apiService: ApiService,
    private signalrService: SignalRService
  ) {}

  ngOnInit() {
    this.loadData();
    this.setupSignalRListeners();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  async loadData() {
    await Promise.all([
      this.loadBacklogTasks(),
      this.loadSprints()
    ]);
  }

  async loadBacklogTasks() {
    try {
      const allTasks = await firstValueFrom(this.apiService.getTasks());
      this.backlogTasks = allTasks.filter(task => 
        task.status === TaskStatus.Backlog || !task.sprintId
      );
      this.applyFilters();
    } catch (error) {
      console.error('Error loading backlog tasks:', error);
    }
  }

  async loadSprints() {
    try {
      this.sprints = await firstValueFrom(this.apiService.getSprints());
    } catch (error) {
      console.error('Error loading sprints:', error);
    }
  }

  applyFilters() {
    let tasks = [...this.backlogTasks];
    
    // Filtro por término de búsqueda
    if (this.searchTerm) {
      tasks = tasks.filter(task => 
        task.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        task.description?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        task.assignedTo?.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }
    
    // Filtro por prioridad
    if (this.selectedPriority) {
      const priority = parseInt(this.selectedPriority) as unknown as TaskPriority;
      tasks = tasks.filter(task => task.priority === priority);
    }
    
    // Filtro por asignado
    if (this.selectedAssignee) {
      tasks = tasks.filter(task => task.assignedTo === this.selectedAssignee);
    }
    
    // Filtro general
    switch (this.currentFilter) {
      case 'high-priority':
        tasks = tasks.filter(task => 
          task.priority === TaskPriority.Critical || 
          task.priority === TaskPriority.High
        );
        break;
      case 'unassigned':
        tasks = tasks.filter(task => !task.assignedTo || task.assignedTo.trim() === '');
        break;
      case 'assigned':
        tasks = tasks.filter(task => task.assignedTo && task.assignedTo.trim() !== '');
        break;
    }
    
    // Ordenamiento
    this.sortTasks(tasks);
    
    this.filteredTasks = tasks;
  }

  sortTasks(tasks: TaskItem[]) {
    switch (this.currentSortBy) {
      case 'priority':
        tasks.sort((a, b) => Number(a.priority) - Number(b.priority));
        break;
      case 'title':
        tasks.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case 'created':
        tasks.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
      case 'updated':
        tasks.sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
        break;
    }
  }

  onFilterChange() {
    this.applyFilters();
  }

  onSortChange() {
    this.applyFilters();
  }

  onSearchChange() {
    this.applyFilters();
  }

  clearFilters() {
    this.currentFilter = 'all';
    this.searchTerm = '';
    this.selectedPriority = '';
    this.selectedAssignee = '';
    this.applyFilters();
  }

  async createTask() {
    if (!this.newTask.title?.trim()) {
      alert('El título es requerido');
      return;
    }

    try {
      const taskToCreate: TaskItem = {
        title: this.newTask.title,
        description: this.newTask.description || '',
        priority: this.newTask.priority || TaskPriority.Medium,
        status: TaskStatus.Backlog,
        assignedTo: this.newTask.assignedTo || '',
        createdAt: new Date(),
        updatedAt: new Date(),
        sprintId: null,
        comments: []
      };

      const createdTask = await firstValueFrom(this.apiService.createTask(taskToCreate));
      this.backlogTasks.push(createdTask);
      this.applyFilters();
      
      // Reset form
      this.newTask = {
        title: '',
        description: '',
        priority: TaskPriority.Medium,
        status: TaskStatus.Backlog,
        assignedTo: ''
      };
      
      this.showCreateModal = false;
      
      // Broadcast via SignalR
      this.signalrService.broadcastTaskUpdate('default', createdTask);
      
    } catch (error) {
      console.error('Error creating task:', error);
      alert('Error al crear la tarea');
    }
  }

  async updateTask(task: TaskItem) {
    try {
      const updatedTask = { ...task, updatedAt: new Date() };
      await firstValueFrom(this.apiService.updateTask(updatedTask));
      
      // Update local state
      const index = this.backlogTasks.findIndex(t => t.id === task.id);
      if (index !== -1) {
        this.backlogTasks[index] = updatedTask;
        this.applyFilters();
      }
      
      this.signalrService.broadcastTaskUpdate('default', updatedTask);
      this.editingTask = null;
      
    } catch (error) {
      console.error('Error updating task:', error);
      alert('Error al actualizar la tarea');
    }
  }

  async deleteTask(task: TaskItem) {
    if (!confirm('¿Estás seguro de que quieres eliminar esta tarea?') || !task.id) return;
    
    try {
      await firstValueFrom(this.apiService.deleteTask(task.id));
      this.backlogTasks = this.backlogTasks.filter(t => t.id !== task.id);
      this.applyFilters();
      
      alert('Tarea eliminada exitosamente');
    } catch (error) {
      console.error('Error deleting task:', error);
      alert('Error al eliminar la tarea');
    }
  }

  // Selección de tareas
  toggleTaskSelection(task: TaskItem) {
    const index = this.selectedTasks.findIndex(t => t.id === task.id);
    if (index === -1) {
      this.selectedTasks.push(task);
    } else {
      this.selectedTasks.splice(index, 1);
    }
  }

  isTaskSelected(task: TaskItem): boolean {
    return this.selectedTasks.some(t => t.id === task.id);
  }

  selectAllTasks() {
    this.selectedTasks = [...this.filteredTasks];
  }

  deselectAllTasks() {
    this.selectedTasks = [];
  }

  // Acciones masivas
  async bulkAssignToSprint(sprintId: number) {
    if (this.selectedTasks.length === 0 || !sprintId) {
      alert('Por favor, selecciona tareas y un sprint.');
      return;
    }

    try {
      const updates = this.selectedTasks.map(task => {
        const updatedTask = { ...task, sprintId, status: TaskStatus.Todo, updatedAt: new Date() };
        return firstValueFrom(this.apiService.updateTask(updatedTask));
      });

      await Promise.all(updates);
      
      // Remove tasks from backlog locally
      this.selectedTasks.forEach(selectedTask => {
        const index = this.backlogTasks.findIndex(t => t.id === selectedTask.id);
        if (index !== -1) {
          this.backlogTasks.splice(index, 1);
        }
      });
      
      this.selectedTasks = [];
      this.applyFilters();
      
      alert('Tareas asignadas al sprint exitosamente');
      
    } catch (error) {
      console.error('Error assigning tasks to sprint:', error);
      alert('Error al asignar tareas al sprint');
    }
  }

  async bulkChangePriority(priorityValue: number) {
    if (this.selectedTasks.length === 0) {
      alert('Por favor, selecciona al menos una tarea.');
      return;
    }

    if (!priorityValue || !Object.values(TaskPriority).includes(priorityValue as unknown as TaskPriority)) {
      alert('Por favor, selecciona una prioridad válida.');
      return;
    }

    const priority = priorityValue as unknown as TaskPriority;

    try {
      const updates = this.selectedTasks.map(task => {
        const updatedTask = { ...task, priority, updatedAt: new Date() };
        return firstValueFrom(this.apiService.updateTask(updatedTask));
      });

      await Promise.all(updates);
      
      // Actualizar tareas localmente
      this.selectedTasks.forEach(selectedTask => {
        const index = this.backlogTasks.findIndex(t => t.id === selectedTask.id);
        if (index !== -1) {
          this.backlogTasks[index].priority = priority;
        }
      });
      
      this.selectedTasks = [];
      this.applyFilters();
      
      alert('Prioridad actualizada exitosamente');
      
    } catch (error) {
      console.error('Error updating priority:', error);
      alert('Error al actualizar la prioridad');
    }
  }

  async bulkDelete() {
    if (this.selectedTasks.length === 0) {
      alert('Por favor, selecciona al menos una tarea.');
      return;
    }

    if (!confirm(`¿Estás seguro de que quieres eliminar ${this.selectedTasks.length} tarea(s)?`)) {
      return;
    }

    try {
      const deletions = this.selectedTasks.map(task => {
        if (task.id) {
          return firstValueFrom(this.apiService.deleteTask(task.id));
        }
        return Promise.resolve();
      });

      await Promise.all(deletions);
      
      // Remove tasks locally
      this.selectedTasks.forEach(selectedTask => {
        const index = this.backlogTasks.findIndex(t => t.id === selectedTask.id);
        if (index !== -1) {
          this.backlogTasks.splice(index, 1);
        }
      });
      
      this.selectedTasks = [];
      this.applyFilters();
      
      alert('Tareas eliminadas exitosamente');
      
    } catch (error) {
      console.error('Error deleting tasks:', error);
      alert('Error al eliminar las tareas');
    }
  }

  // Métodos de utilidad
  getPriorityLabel(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Critical: return 'Crítica';
      case TaskPriority.High: return 'Alta';
      case TaskPriority.Medium: return 'Media';
      case TaskPriority.Low: return 'Baja';
      default: return 'No definida';
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

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Critical: return '🔴';
      case TaskPriority.High: return '🟠';
      case TaskPriority.Medium: return '🟡';
      case TaskPriority.Low: return '🟢';
      default: return '⚪';
    }
  }

  getUniqueAssignees(): string[] {
    const assignees = new Set<string>();
    this.backlogTasks.forEach(task => {
      if (task.assignedTo && task.assignedTo.trim() !== '') {
        assignees.add(task.assignedTo);
      }
    });
    return Array.from(assignees).sort();
  }

  startEditingTask(task: TaskItem) {
    this.editingTask = { ...task };
  }

  cancelEditing() {
    this.editingTask = null;
  }

  saveTaskEdit() {
    if (this.editingTask) {
      this.updateTask(this.editingTask);
    }
  }

  trackByTaskId(index: number, task: TaskItem): number | undefined {
    return task.id;
  }

  private setupSignalRListeners() {
    this.subscription.add(
      this.signalrService.taskUpdated$.subscribe(updatedTask => {
        // If task is moved to backlog, add it
        if (updatedTask.status === TaskStatus.Backlog || !updatedTask.sprintId) {
          const existingIndex = this.backlogTasks.findIndex(t => t.id === updatedTask.id);
          if (existingIndex === -1) {
            this.backlogTasks.push(updatedTask);
          } else {
            this.backlogTasks[existingIndex] = updatedTask;
          }
        } else {
          // Task moved out of backlog, remove it
          this.backlogTasks = this.backlogTasks.filter(t => t.id !== updatedTask.id);
        }
        this.applyFilters();
      })
    );
  }
}
