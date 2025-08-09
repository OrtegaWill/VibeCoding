import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskItem, Sprint, Comment, TaskStatus } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5015/api';

  constructor(private http: HttpClient) { }

  // Tasks
  getTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.baseUrl}/tasks`);
  }

  getTask(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.baseUrl}/tasks/${id}`);
  }

  getBacklogTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.baseUrl}/tasks/backlog`);
  }

  getTasksBySprint(sprintId: number): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.baseUrl}/tasks/sprint/${sprintId}`);
  }

  createTask(task: TaskItem): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.baseUrl}/tasks`, task);
  }

  updateTask(task: TaskItem): Observable<any> {
    return this.http.put(`${this.baseUrl}/tasks/${task.id}`, task);
  }

  deleteTask(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/tasks/${id}`);
  }

  updateTaskStatus(id: number, status: TaskStatus): Observable<any> {
    return this.http.put(`${this.baseUrl}/tasks/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  assignTask(id: number, assignedTo: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/tasks/${id}/assign`, JSON.stringify(assignedTo), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // Sprints
  getSprints(): Observable<Sprint[]> {
    return this.http.get<Sprint[]>(`${this.baseUrl}/sprints`);
  }

  getSprint(id: number): Observable<Sprint> {
    return this.http.get<Sprint>(`${this.baseUrl}/sprints/${id}`);
  }

  getActiveSprint(): Observable<Sprint> {
    return this.http.get<Sprint>(`${this.baseUrl}/sprints/active`);
  }

  createSprint(sprint: Sprint): Observable<Sprint> {
    return this.http.post<Sprint>(`${this.baseUrl}/sprints`, sprint);
  }

  updateSprint(sprint: Sprint): Observable<any> {
    return this.http.put(`${this.baseUrl}/sprints/${sprint.id}`, sprint);
  }

  deleteSprint(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/sprints/${id}`);
  }

  startSprint(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/sprints/${id}/start`, {});
  }

  completeSprint(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/sprints/${id}/complete`, {});
  }

  addTaskToSprint(sprintId: number, taskId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/sprints/${sprintId}/add-task/${taskId}`, {});
  }

  // Comments
  getTaskComments(taskId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/comments/task/${taskId}`);
  }

  getSprintComments(sprintId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/comments/sprint/${sprintId}`);
  }

  addTaskComment(taskId: number, comment: Comment): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/comments/task/${taskId}`, comment);
  }

  addSprintComment(sprintId: number, comment: Comment): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/comments/sprint/${sprintId}`, comment);
  }

  createComment(comment: Comment): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/comments`, comment);
  }

  deleteComment(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/comments/${id}`);
  }
}
