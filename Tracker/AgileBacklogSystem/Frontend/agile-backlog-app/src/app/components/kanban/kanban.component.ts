import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';

import { TareaService } from '../../services/tarea.service';
import { SignalRService } from '../../services/signalr.service';
import { KanbanColumn, Tarea } from '../../models/models';

@Component({
  selector: 'app-kanban',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    DragDropModule
  ],
  templateUrl: './kanban.component.html',
  styleUrls: ['./kanban.component.css']
})
export class KanbanComponent implements OnInit, OnDestroy {
  kanbanColumns: KanbanColumn[] = [];
  loading = true;
  private subscription = new Subscription();

  constructor(
    private tareaService: TareaService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadKanbanData();
    this.setupSignalRConnection();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadKanbanData() {
    this.loading = true;
    this.subscription.add(
      this.tareaService.getKanban().subscribe({
        next: (columns: KanbanColumn[]) => {
          this.kanbanColumns = columns;
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error cargando kanban:', error);
          this.loading = false;
        }
      })
    );
  }

  private setupSignalRConnection() {
    this.signalRService.startConnection();
    
    this.signalRService.onTareaUpdated().subscribe(() => {
      this.loadKanbanData();
    });
  }

  drop(event: CdkDragDrop<Tarea[]>, targetColumn: KanbanColumn) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const tarea = event.previousContainer.data[event.previousIndex];
      
      // Actualizar el estado de la tarea según la columna
      const nuevoEstado = this.getEstadoFromColumn(targetColumn.estado);
      
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );

      // Actualizar en el backend
      this.updateTareaEstado(tarea, nuevoEstado);
    }
  }

  private getEstadoFromColumn(columnName: string): string {
    switch (columnName.toLowerCase()) {
      case 'nuevo':
      case 'por hacer':
        return 'Nuevo';
      case 'en progreso':
      case 'desarrollo':
        return 'En Progreso';
      case 'terminado':
      case 'completado':
        return 'Terminado';
      default:
        return 'Nuevo';
    }
  }

  private updateTareaEstado(tarea: Tarea, nuevoEstado: string) {
    const updateData = {
      ...tarea,
      estado: nuevoEstado
    };

    this.subscription.add(
      this.tareaService.updateTarea(tarea.id, updateData).subscribe({
        next: () => {
          console.log('Tarea actualizada exitosamente');
        },
        error: (error: any) => {
          console.error('Error actualizando tarea:', error);
          // Recargar datos en caso de error
          this.loadKanbanData();
        }
      })
    );
  }

  getPriorityColor(prioridad: string): string {
    switch (prioridad?.toLowerCase()) {
      case 'alta': return '#f44336';
      case 'media': return '#ff9800';
      case 'baja': return '#4caf50';
      default: return '#757575';
    }
  }

  trackByTareaId(index: number, tarea: Tarea): number {
    return tarea.id;
  }

  trackByColumnName(index: number, column: KanbanColumn): string {
    return column.estado;
  }
}
