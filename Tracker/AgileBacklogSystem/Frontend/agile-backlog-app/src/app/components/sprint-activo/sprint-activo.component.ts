import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { Subscription } from 'rxjs';

import { SprintService } from '../../services/sprint.service';
import { TareaService } from '../../services/tarea.service';
import { SignalRService } from '../../services/signalr.service';
import { Sprint, Tarea } from '../../models/models';

@Component({
  selector: 'app-sprint-activo',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './sprint-activo.component.html',
  styleUrls: ['./sprint-activo.component.css']
})
export class SprintActivoComponent implements OnInit, OnDestroy {
  sprintActivo: Sprint | null = null;
  tareasSprintActivo: Tarea[] = [];
  loading = true;
  private subscription = new Subscription();

  // Estadísticas del sprint
  totalTareas = 0;
  tareasCompletadas = 0;
  tareasEnProgreso = 0;
  tareasPendientes = 0;
  horasEstimadasTotal = 0;
  horasCompletadas = 0;
  progreso = 0;

  constructor(
    private sprintService: SprintService,
    private tareaService: TareaService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadSprintActivo();
    this.setupSignalRConnection();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadSprintActivo() {
    this.loading = true;
    this.subscription.add(
      this.sprintService.getSprintActivo().subscribe({
        next: (sprint: Sprint | null) => {
          this.sprintActivo = sprint;
          if (sprint) {
            this.loadTareasSprintActivo(sprint.id);
          } else {
            this.loading = false;
          }
        },
        error: (error: any) => {
          console.error('Error cargando sprint activo:', error);
          this.loading = false;
        }
      })
    );
  }

  private loadTareasSprintActivo(sprintId: number) {
    this.subscription.add(
      this.tareaService.getTareas(sprintId).subscribe({
        next: (tareas: Tarea[]) => {
          this.tareasSprintActivo = tareas;
          this.calcularEstadisticas();
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error cargando tareas del sprint activo:', error);
          this.loading = false;
        }
      })
    );
  }

  private setupSignalRConnection() {
    this.signalRService.startConnection();
    
    this.signalRService.onTareaUpdated().subscribe(() => {
      if (this.sprintActivo) {
        this.loadTareasSprintActivo(this.sprintActivo.id);
      }
    });

    this.signalRService.onSprintUpdated().subscribe(() => {
      this.loadSprintActivo();
    });
  }

  private calcularEstadisticas() {
    this.totalTareas = this.tareasSprintActivo.length;
    this.tareasCompletadas = this.tareasSprintActivo.filter(t => t.estado === 'Terminado').length;
    this.tareasEnProgreso = this.tareasSprintActivo.filter(t => t.estado === 'En Progreso').length;
    this.tareasPendientes = this.tareasSprintActivo.filter(t => t.estado === 'Nuevo').length;
    
    this.horasEstimadasTotal = this.tareasSprintActivo.reduce((sum, tarea) => sum + (tarea.horasEstimadas || 0), 0);
    this.horasCompletadas = this.tareasSprintActivo
      .filter(t => t.estado === 'Terminado')
      .reduce((sum, tarea) => sum + (tarea.horasEstimadas || 0), 0);
    
    this.progreso = this.totalTareas > 0 ? (this.tareasCompletadas / this.totalTareas) * 100 : 0;
  }

  getPriorityColor(prioridad: string): string {
    switch (prioridad?.toLowerCase()) {
      case 'alta': return '#f44336';
      case 'media': return '#ff9800';
      case 'baja': return '#4caf50';
      default: return '#757575';
    }
  }

  getStatusColor(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'nuevo': return '#2196f3';
      case 'en progreso': return '#ff9800';
      case 'terminado': return '#4caf50';
      default: return '#757575';
    }
  }

  trackByTareaId(index: number, tarea: Tarea): number {
    return tarea.id;
  }

  onFinalizarSprint() {
    if (this.sprintActivo) {
      this.subscription.add(
        this.sprintService.finalizarSprint(this.sprintActivo.id).subscribe({
          next: () => {
            console.log('Sprint finalizado exitosamente');
            this.loadSprintActivo();
          },
          error: (error: any) => {
            console.error('Error finalizando sprint:', error);
          }
        })
      );
    }
  }

  onTareaClick(tarea: Tarea) {
    // TODO: Implementar modal para editar tarea
    console.log('Tarea seleccionada:', tarea);
  }

  getDaysRemaining(): number {
    if (!this.sprintActivo?.fechaFin) return 0;
    const today = new Date();
    const endDate = new Date(this.sprintActivo.fechaFin);
    const diffTime = endDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(0, diffDays);
  }

  isSprintOverdue(): boolean {
    if (!this.sprintActivo?.fechaFin) return false;
    return new Date() > new Date(this.sprintActivo.fechaFin);
  }
}
