import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { Subscription } from 'rxjs';

import { TareaService } from '../../services/tarea.service';
import { SprintService } from '../../services/sprint.service';
import { CatalogoService } from '../../services/catalogo.service';
import { SignalRService } from '../../services/signalr.service';
import { Tarea } from '../../models/models';

@Component({
  selector: 'app-backlog',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatBadgeModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatButtonToggleModule
  ],
  templateUrl: './backlog.component.html',
  styleUrls: ['./backlog.component.css']
})
export class BacklogComponent implements OnInit, OnDestroy {
  tareas: Tarea[] = [];
  tareasBacklog: Tarea[] = [];
  catalogos: any[] = [];
  loading = true;
  private subscription = new Subscription();
  
  // Propiedades para ordenamiento
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private tareaService: TareaService,
    private sprintService: SprintService,
    private catalogoService: CatalogoService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadBacklogTareas();
    this.loadCatalogos();
    this.setupSignalRConnection();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadBacklogTareas() {
    this.loading = true;
    this.subscription.add(
      this.tareaService.getBacklogTareas().subscribe({
        next: (tareas) => {
          this.tareasBacklog = tareas;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error cargando tareas del backlog:', error);
          this.loading = false;
        }
      })
    );
  }

  private loadCatalogos() {
    this.subscription.add(
      this.catalogoService.getCatalogos().subscribe({
        next: (catalogos) => {
          this.catalogos = catalogos;
        },
        error: (error) => {
          console.error('Error cargando catálogos:', error);
        }
      })
    );
  }

  // Getter para obtener tareas ordenadas
  get sortedTareas(): Tarea[] {
    if (!this.sortColumn || this.tareasBacklog.length === 0) {
      return this.tareasBacklog;
    }

    return [...this.tareasBacklog].sort((a, b) => {
      let aValue = this.getValueByColumn(a, this.sortColumn);
      let bValue = this.getValueByColumn(b, this.sortColumn);

      // Manejar valores nulos/undefined
      if (aValue === null || aValue === undefined) aValue = '';
      if (bValue === null || bValue === undefined) bValue = '';

      // Convertir a string para comparación
      const aStr = String(aValue).toLowerCase();
      const bStr = String(bValue).toLowerCase();

      let comparison = 0;
      if (aStr < bStr) {
        comparison = -1;
      } else if (aStr > bStr) {
        comparison = 1;
      }

      return this.sortDirection === 'desc' ? comparison * -1 : comparison;
    });
  }

  // Método para ordenar por columna
  sortBy(column: string) {
    if (this.sortColumn === column) {
      // Si ya está ordenado por esta columna, cambiar dirección
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      // Nueva columna, empezar con ascendente
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
  }

  // Método auxiliar para obtener el valor de una columna específica
  private getValueByColumn(tarea: Tarea, column: string): any {
    switch (column) {
      case 'id': return tarea.id;
      case 'responsable': return tarea.responsable;
      case 'descripcion': return tarea.descripcion;
      case 'estado': return tarea.estado;
      case 'prioridad': return tarea.prioridad;
      case 'categoriaId': return tarea.categoriaId;
      case 'progreso': return tarea.avance || 0;
      default: return '';
    }
  }

  // Método auxiliar para obtener las iniciales del usuario
  getInitials(name: string): string {
    if (!name || name.trim() === '' || name.trim().toLowerCase() === 'sin asignar') {
      return '?';
    }
    return name.trim()
      .split(' ')
      .map(word => word.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  // Método auxiliar para obtener el nombre completo formateado
  getFullName(name: string): string {
    if (!name || name.trim() === '' || name.trim().toLowerCase() === 'sin asignar') {
      return 'Sin Asignar';
    }
    return name.trim();
  }

  private setupSignalRConnection() {
    this.signalRService.startConnection();
    
    this.signalRService.onTareaUpdated().subscribe(() => {
      this.loadBacklogTareas();
    });
  }

  getPriorityColor(prioridad?: string): string {
    switch (prioridad?.toLowerCase()) {
      case 'alta': return '#f44336';
      case 'media': return '#ff9800';
      case 'baja': return '#4caf50';
      default: return '#757575';
    }
  }

  getStatusColor(estado?: string): string {
    switch (estado?.toLowerCase()) {
      case 'nuevo': return '#2196f3';
      case 'por hacer': return '#2196f3';
      case 'en progreso': return '#ff9800';
      case 'hecho': return '#4caf50';
      case 'terminado': return '#4caf50';
      case 'asignado': return '#9c27b0';
      default: return '#757575';
    }
  }

  getPriorityIcon(prioridad?: string): string {
    switch (prioridad?.toLowerCase()) {
      case 'crítica': return 'report';
      case 'alta': return 'priority_high';
      case 'media': return 'remove';
      case 'baja': return 'expand_more';
      default: return 'help';
    }
  }

  getStatusIcon(estado?: string): string {
    switch (estado?.toLowerCase()) {
      case 'nuevo': return 'fiber_new';
      case 'por hacer': return 'radio_button_unchecked';
      case 'en progreso': return 'hourglass_empty';
      case 'hecho': return 'check_circle';
      case 'terminado': return 'check_circle';
      case 'asignado': return 'assignment_ind';
      default: return 'help';
    }
  }

  getPriorityBadgeColor(prioridad?: string): string {
    switch (prioridad?.toLowerCase()) {
      case 'crítica': return '#d32f2f';
      case 'alta': return '#f57c00';
      case 'media': return '#388e3c';
      case 'baja': return '#1976d2';
      default: return '#757575';
    }
  }

  // Método para obtener el nombre de la categoría
  getCategoriaName(categoriaId?: number): string {
    if (!categoriaId) return 'Sin categoría';
    
    const categoria = this.catalogos.find((c: any) => c.id === categoriaId);
    return categoria ? categoria.descripcion : 'Sin categoría';
  }

  trackByTareaId(index: number, tarea: Tarea): number {
    return tarea.id;
  }

  onTareaClick(tarea: Tarea) {
    // TODO: Implementar modal para editar tarea
    console.log('Tarea seleccionada:', tarea);
  }

  onCreateTarea() {
    // TODO: Implementar modal para crear tarea
    console.log('Crear nueva tarea');
  }

  onImportExcel() {
    // TODO: Implementar importación desde Excel
    console.log('Importar desde Excel');
  }

  onExportExcel() {
    // TODO: Implementar exportación a Excel
    this.subscription.add(
      this.tareaService.exportToExcel().subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = 'backlog.xlsx';
          link.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error exportando Excel:', error);
        }
      })
    );
  }

  // Función auxiliar para obtener color de progreso
  getProgressColor(avance: number): string {
    if (avance >= 70) return '#22c55e'; // Verde
    if (avance >= 30) return '#eab308'; // Amarillo
    return '#ef4444'; // Rojo
  }
}
