import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sprint, SprintCreate, SprintUpdate } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class SprintService {
  private apiUrl = 'http://localhost:5106/api/sprints';

  constructor(private http: HttpClient) { }

  getSprints(): Observable<Sprint[]> {
    return this.http.get<Sprint[]>(this.apiUrl);
  }

  getSprint(id: number): Observable<Sprint> {
    return this.http.get<Sprint>(`${this.apiUrl}/${id}`);
  }

  getSprintActivo(): Observable<Sprint | null> {
    return this.http.get<Sprint | null>(`${this.apiUrl}/active`);
  }

  getActiveSprint(): Observable<Sprint> {
    return this.http.get<Sprint>(`${this.apiUrl}/active`);
  }

  createSprint(sprint: SprintCreate): Observable<Sprint> {
    return this.http.post<Sprint>(this.apiUrl, sprint);
  }

  updateSprint(id: number, sprint: SprintUpdate): Observable<Sprint> {
    return this.http.put<Sprint>(`${this.apiUrl}/${id}`, sprint);
  }

  startSprint(id: number): Observable<Sprint> {
    return this.http.post<Sprint>(`${this.apiUrl}/${id}/start`, {});
  }

  finalizarSprint(id: number, moverTareasA?: string, siguienteSprintId?: number): Observable<Sprint> {
    const body = {
      moverTareasA: moverTareasA || 'backlog',
      siguienteSprintId
    };
    return this.http.post<Sprint>(`${this.apiUrl}/${id}/complete`, body);
  }

  completeSprint(id: number, moverTareasA: string, siguienteSprintId?: number): Observable<Sprint> {
    const body = {
      moverTareasA,
      siguienteSprintId
    };
    return this.http.post<Sprint>(`${this.apiUrl}/${id}/complete`, body);
  }

  deleteSprint(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
