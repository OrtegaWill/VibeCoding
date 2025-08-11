import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatDividerModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'Sistema de Backlog y Gestión Ágil';
  sidenavOpened = false;

  navigationItems = [
    {
      label: 'Backlog',
      icon: 'inventory_2',
      route: '/backlog',
      description: 'Gestionar product backlog'
    },
    {
      label: 'Kanban',
      icon: 'view_kanban',
      route: '/kanban',
      description: 'Tablero visual de tareas'
    },
    {
      label: 'Sprint Activo',
      icon: 'rocket_launch',
      route: '/sprint-activo',
      description: 'Sprint en progreso'
    },
    {
      label: 'Todos los Sprints',
      icon: 'track_changes',
      route: '/todos-sprints',
      description: 'Gestionar sprints'
    }
  ];

  toggleSidenav() {
    this.sidenavOpened = !this.sidenavOpened;
  }
}
