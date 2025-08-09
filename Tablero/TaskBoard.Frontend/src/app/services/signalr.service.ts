import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  private taskUpdatedSubject = new Subject<any>();
  private sprintUpdatedSubject = new Subject<any>();
  private commentAddedSubject = new Subject<any>();

  public taskUpdated$: Observable<any> = this.taskUpdatedSubject.asObservable();
  public sprintUpdated$: Observable<any> = this.sprintUpdatedSubject.asObservable();
  public commentAdded$: Observable<any> = this.commentAddedSubject.asObservable();

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5015/taskboardhub')
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.log('Error while starting SignalR connection: ' + err));

    this.hubConnection.on('TaskUpdated', (task) => {
      this.taskUpdatedSubject.next(task);
    });

    this.hubConnection.on('SprintUpdated', (sprint) => {
      this.sprintUpdatedSubject.next(sprint);
    });

    this.hubConnection.on('CommentAdded', (comment) => {
      this.commentAddedSubject.next(comment);
    });
  }

  public joinProject = (projectId: string) => {
    this.hubConnection.invoke('JoinProject', projectId);
  }

  public leaveProject = (projectId: string) => {
    this.hubConnection.invoke('LeaveProject', projectId);
  }

  public broadcastTaskUpdate = (projectId: string, task: any) => {
    this.hubConnection.invoke('TaskUpdated', projectId, task);
  }

  public broadcastSprintUpdate = (projectId: string, sprint: any) => {
    this.hubConnection.invoke('SprintUpdated', projectId, sprint);
  }

  public broadcastCommentAdded = (projectId: string, comment: any) => {
    this.hubConnection.invoke('CommentAdded', projectId, comment);
  }
}
