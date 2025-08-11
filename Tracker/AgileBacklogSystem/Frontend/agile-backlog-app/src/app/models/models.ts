export interface Tarea {
  id: number;
  idIncidencia?: string;
  idPeticion?: string;
  detalleDescripcion?: string;
  grupoAsignadoId?: number;
  grupoAsignadoNombre?: string;
  prioridadId?: number;
  prioridadNombre?: string;
  prioridad?: string;
  estatusId?: number;
  estatusNombre?: string;
  fechaAsignacion?: Date;
  apellidos?: string;
  nombre?: string;
  descripcion?: string;
  tipoTarea?: string;
  responsable?: string;
  estado?: string;
  horasEstimadas?: number;
  fechaVencimiento?: Date;
  criticidadId?: number;
  criticidadNombre?: string;
  tipoQuejaId?: number;
  tipoQuejaNombre?: string;
  origenId?: number;
  origenNombre?: string;
  categoriaId?: number;
  categoriaNombre?: string;
  grupoResolutorId?: number;
  grupoResolutorNombre?: string;
  
  // Campos de seguimiento
  historial?: string;
  avance?: number;
  visorAplicativoAfectado?: string;
  problema?: string;
  detalleProblema?: string;
  quienAtiende?: string;
  tiempoResolucion?: number;
  fechaAckEquipoPrecargas?: Date;
  fechaSolucion?: Date;
  solucionRemedy?: string;
  precarga?: string;
  rfcSolicitudCambio?: string;
  causaRaiz?: string;
  
  // Campos ágiles
  sprintId?: number;
  sprintNombre?: string;
  estadoTareaId?: number;
  estadoTareaNombre?: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
  
  comentarios: ComentarioTarea[];
}

export interface TareaCreate {
  idIncidencia?: string;
  idPeticion?: string;
  detalleDescripcion?: string;
  grupoAsignadoId?: number;
  prioridadId?: number;
  estatusId?: number;
  fechaAsignacion?: Date;
  fechaSolucion?: Date;
  apellidos?: string;
  nombre?: string;
  criticidadId?: number;
  tipoQuejaId?: number;
  origenId?: number;
  categoriaId?: number;
  grupoResolutorId?: number;
  sprintId?: number;
  estadoTareaId?: number;
}

export interface TareaUpdate extends TareaCreate {
  historial?: string;
  avance?: number;
  visorAplicativoAfectado?: string;
  problema?: string;
  detalleProblema?: string;
  quienAtiende?: string;
  tiempoResolucion?: number;
  fechaAckEquipoPrecargas?: Date;
  solucionRemedy?: string;
  precarga?: string;
  rfcSolicitudCambio?: string;
  causaRaiz?: string;
}

export interface Sprint {
  id: number;
  nombre: string;
  descripcion?: string;
  objetivo?: string;
  fechaInicio: Date;
  fechaFin: Date;
  esActivo: boolean;
  esCompletado: boolean;
  estado: string;
  responsableId?: string;
  responsableNombre?: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
  tareas: Tarea[];
  totalTareas: number;
  tareasCompletadas: number;
  porcentajeCompletado: number;
}

export interface SprintCreate {
  nombre: string;
  descripcion?: string;
  objetivo?: string;
  fechaInicio: Date;
  fechaFin: Date;
  responsableNombre?: string;
}

export interface SprintUpdate extends SprintCreate {
  esActivo: boolean;
  esCompletado: boolean;
}

export interface Catalogo {
  id: number;
  tipo: string;
  valor: string;
  descripcion?: string;
  activo: boolean;
  orden: number;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export interface CatalogoCreate {
  tipo: string;
  valor: string;
  descripcion?: string;
  orden: number;
}

export interface ComentarioTarea {
  id: number;
  tareaId: number;
  contenido: string;
  autorNombre: string;
  autorId?: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export interface ComentarioTareaCreate {
  tareaId: number;
  contenido: string;
  autorNombre: string;
}

export interface KanbanColumn {
  estado: string;
  estadoId: number;
  tareas: Tarea[];
}
