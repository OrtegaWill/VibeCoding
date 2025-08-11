import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/backlog',
    pathMatch: 'full'
  },
  {
    path: 'backlog',
    loadComponent: () => import('./components/backlog/backlog.component').then(m => m.BacklogComponent)
  },
  {
    path: 'kanban',
    loadComponent: () => import('./components/kanban/kanban.component').then(m => m.KanbanComponent)
  },
  {
    path: 'sprint-activo',
    loadComponent: () => import('./components/sprint-activo/sprint-activo.component').then(m => m.SprintActivoComponent)
  },
  {
    path: 'todos-sprints',
    loadComponent: () => import('./components/todos-sprints/todos-sprints.component').then(m => m.TodosSprintsComponent)
  },
  {
    path: '**',
    redirectTo: '/backlog'
  }
];
