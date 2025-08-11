import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tarea, TareaCreate, TareaUpdate, KanbanColumn } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class TareaService {
  private apiUrl = 'http://localhost:5106/api/tareas';

  constructor(private http: HttpClient) { }

  getTareas(sprintId?: number, estadoId?: number): Observable<Tarea[]> {
    let params = new HttpParams();
    if (sprintId !== undefined) {
      params = params.set('sprintId', sprintId.toString());
    }
    if (estadoId !== undefined) {
      params = params.set('estadoId', estadoId.toString());
    }
    return this.http.get<Tarea[]>(this.apiUrl, { params });
  }

  getTarea(id: number): Observable<Tarea> {
    return this.http.get<Tarea>(`${this.apiUrl}/${id}`);
  }

  createTarea(tarea: TareaCreate): Observable<Tarea> {
    return this.http.post<Tarea>(this.apiUrl, tarea);
  }

  updateTarea(id: number, tarea: TareaUpdate): Observable<Tarea> {
    return this.http.put<Tarea>(`${this.apiUrl}/${id}`, tarea);
  }

  deleteTarea(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getBacklogTareas(): Observable<Tarea[]> {
    return this.http.get<Tarea[]>(`${this.apiUrl}/backlog`);
  }

  getKanban(sprintId?: number): Observable<KanbanColumn[]> {
    let params = new HttpParams();
    if (sprintId !== undefined) {
      params = params.set('sprintId', sprintId.toString());
    }
    return this.http.get<KanbanColumn[]>(`${this.apiUrl}/kanban`, { params });
  }

  exportToExcel(): Observable<Blob> {
    return this.http.get(`http://localhost:5000/api/excel/export`, { 
      responseType: 'blob',
      headers: new HttpHeaders({
        'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      })
    });
  }

  importFromExcel(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`http://localhost:5000/api/excel/import`, formData);
  }
}
