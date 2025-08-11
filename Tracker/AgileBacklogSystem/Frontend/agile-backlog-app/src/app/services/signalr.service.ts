import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private connectionEstablished = new Subject<boolean>();
  
  // Subjects for different events
  private tareaUpdatedSubject = new Subject<any>();
  private sprintUpdatedSubject = new Subject<any>();

  constructor() { 
    this.initializeConnection();
  }

  private initializeConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5106/hubs/backlog')
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Reintentos automáticos
      .configureLogging(signalR.LogLevel.Information)
      .build();
    
    // Configurar timeout más largo
    this.hubConnection.serverTimeoutInMilliseconds = 60000; // 60 segundos
    this.hubConnection.keepAliveIntervalInMilliseconds = 15000; // 15 segundos
    
    this.setupEventListeners();
  }

  public startConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Disconnected) {
      this.hubConnection
        .start()
        .then(() => {
          console.log('SignalR connection started');
          this.connectionEstablished.next(true);
        })
        .catch(err => {
          console.error('Error while starting SignalR connection: ', err);
          this.connectionEstablished.next(false);
        });
    }
  }

  private setupEventListeners(): void {
    if (this.hubConnection) {
      // Eventos de datos
      this.hubConnection.on('TareaUpdated', (tarea: any) => {
        this.tareaUpdatedSubject.next(tarea);
      });

      this.hubConnection.on('SprintUpdated', (sprint: any) => {
        this.sprintUpdatedSubject.next(sprint);
      });

      // Eventos de conexión
      this.hubConnection.onreconnecting((error) => {
        console.log('SignalR reconnecting:', error);
        this.connectionEstablished.next(false);
      });

      this.hubConnection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        this.connectionEstablished.next(true);
      });

      this.hubConnection.onclose((error) => {
        console.log('SignalR connection closed:', error);
        this.connectionEstablished.next(false);
      });
    }
  }

  public onTareaUpdated(): Observable<any> {
    return this.tareaUpdatedSubject.asObservable();
  }

  public onSprintUpdated(): Observable<any> {
    return this.sprintUpdatedSubject.asObservable();
  }

  public onConnectionEstablished(): Observable<boolean> {
    return this.connectionEstablished.asObservable();
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
