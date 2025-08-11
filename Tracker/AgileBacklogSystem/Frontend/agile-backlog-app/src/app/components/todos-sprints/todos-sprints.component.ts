import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { Subscription } from 'rxjs';

import { SprintService } from '../../services/sprint.service';
import { TareaService } from '../../services/tarea.service';
import { SignalRService } from '../../services/signalr.service';
import { Sprint, Tarea } from '../../models/models';

@Component({
  selector: 'app-todos-sprints',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTabsModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './todos-sprints.component.html',
  styleUrls: ['./todos-sprints.component.css']
})
export class TodosSprintsComponent implements OnInit, OnDestroy {
  sprints: Sprint[] = [];
  sprintsActivos: Sprint[] = [];
  sprintsCompletados: Sprint[] = [];
  sprintsPlanificados: Sprint[] = [];
  loading = true;
  private subscription = new Subscription();

  constructor(
    private sprintService: SprintService,
    private tareaService: TareaService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadSprints();
    this.setupSignalRConnection();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadSprints() {
    this.loading = true;
    this.subscription.add(
      this.sprintService.getSprints().subscribe({
        next: (sprints: Sprint[]) => {
          this.sprints = sprints;
          this.categorizeSprints();
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error cargando sprints:', error);
          this.loading = false;
        }
      })
    );
  }

  private setupSignalRConnection() {
    this.signalRService.startConnection();
    
    this.signalRService.onSprintUpdated().subscribe(() => {
      this.loadSprints();
    });
  }

  private categorizeSprints() {
    this.sprintsActivos = this.sprints.filter(s => s.estado === 'Activo');
    this.sprintsCompletados = this.sprints.filter(s => s.estado === 'Completado');
    this.sprintsPlanificados = this.sprints.filter(s => s.estado === 'Planificado' || s.estado === 'Pendiente');
  }

  getSprintProgress(sprint: Sprint): number {
    if (!sprint.totalTareas || sprint.totalTareas === 0) return 0;
    return (sprint.tareasCompletadas || 0) / sprint.totalTareas * 100;
  }

  getSprintStatusColor(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'activo': return '#4caf50';
      case 'completado': return '#2196f3';
      case 'planificado':
      case 'pendiente': return '#ff9800';
      default: return '#757575';
    }
  }

  getDaysUntilStart(sprint: Sprint): number {
    if (!sprint.fechaInicio) return 0;
    const today = new Date();
    const startDate = new Date(sprint.fechaInicio);
    const diffTime = startDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(0, diffDays);
  }

  getDaysRemaining(sprint: Sprint): number {
    if (!sprint.fechaFin) return 0;
    const today = new Date();
    const endDate = new Date(sprint.fechaFin);
    const diffTime = endDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(0, diffDays);
  }

  isSprintOverdue(sprint: Sprint): boolean {
    if (!sprint.fechaFin) return false;
    return new Date() > new Date(sprint.fechaFin) && sprint.estado === 'Activo';
  }

  trackBySprintId(index: number, sprint: Sprint): number {
    return sprint.id;
  }

  onCreateSprint() {
    // TODO: Implementar modal para crear sprint
    console.log('Crear nuevo sprint');
  }

  onEditSprint(sprint: Sprint) {
    // TODO: Implementar modal para editar sprint
    console.log('Editar sprint:', sprint);
  }

  onStartSprint(sprint: Sprint) {
    this.subscription.add(
      this.sprintService.startSprint(sprint.id).subscribe({
        next: () => {
          console.log('Sprint iniciado exitosamente');
          this.loadSprints();
        },
        error: (error: any) => {
          console.error('Error iniciando sprint:', error);
        }
      })
    );
  }

  onCompleteSprint(sprint: Sprint) {
    this.subscription.add(
      this.sprintService.completeSprint(sprint.id, 'backlog').subscribe({
        next: () => {
          console.log('Sprint completado exitosamente');
          this.loadSprints();
        },
        error: (error: any) => {
          console.error('Error completando sprint:', error);
        }
      })
    );
  }

  onDeleteSprint(sprint: Sprint) {
    if (confirm(`¿Estás seguro de que deseas eliminar el sprint "${sprint.nombre}"?`)) {
      this.subscription.add(
        this.sprintService.deleteSprint(sprint.id).subscribe({
          next: () => {
            console.log('Sprint eliminado exitosamente');
            this.loadSprints();
          },
          error: (error: any) => {
            console.error('Error eliminando sprint:', error);
          }
        })
      );
    }
  }

  onViewSprintDetails(sprint: Sprint) {
    // TODO: Implementar vista detallada del sprint
    console.log('Ver detalles del sprint:', sprint);
  }
}
