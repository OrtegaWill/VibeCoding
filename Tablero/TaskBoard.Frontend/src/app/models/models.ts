export interface TaskItem {
  id?: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo?: string;
  sprintId?: number | null;
  sprint?: Sprint;
  createdAt: Date;
  updatedAt: Date;
  comments: Comment[];
}

export enum TaskStatus {
  Backlog = 'Backlog',
  Todo = 'Todo',
  InProgress = 'InProgress',
  Review = 'Review',
  Done = 'Done'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export interface Sprint {
  id?: number;
  name: string;
  goal?: string;
  status: SprintStatus;
  startDate?: Date;
  endDate?: Date;
  createdAt: Date;
  updatedAt: Date;
  tasks: TaskItem[];
  comments: Comment[];
}

export enum SprintStatus {
  Planned = 'Planned',
  Active = 'Active',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface Comment {
  id?: number;
  content: string;
  author: string;
  createdAt: Date;
  taskItemId?: number;
  taskItem?: TaskItem;
  sprintId?: number;
  sprint?: Sprint;
}
