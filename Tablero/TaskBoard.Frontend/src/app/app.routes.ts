import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';
import { KanbanBoardComponent } from './components/kanban-board/kanban-board';
import { BacklogComponent } from './components/backlog/backlog';
import { SprintViewComponent } from './components/sprint-view/sprint-view';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'kanban', component: KanbanBoardComponent },
  { path: 'backlog', component: BacklogComponent },
  { path: 'sprint/:id', component: SprintViewComponent }
];
