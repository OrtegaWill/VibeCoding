// SignalR Connection para actualizaciones en tiempo real
class TicketSignalRConnection {
    constructor() {
        this.connection = null;
        this.dotNetObjectRef = null;
        this.isConnected = false;
    }

    // Inicializar la conexión SignalR
    async start(dotNetObjectRef) {
        try {
            this.dotNetObjectRef = dotNetObjectRef;
            
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/ticketHub")
                .withAutomaticReconnect()
                .build();

            // Configurar eventos de reconexión
            this.connection.onreconnecting(() => {
                console.log("SignalR: Reconectando...");
                this.isConnected = false;
            });

            this.connection.onreconnected(() => {
                console.log("SignalR: Reconectado");
                this.isConnected = true;
                this.joinGroups();
            });

            this.connection.onclose(() => {
                console.log("SignalR: Desconectado");
                this.isConnected = false;
            });

            // Configurar manejadores de eventos
            this.setupEventHandlers();

            // Iniciar la conexión
            await this.connection.start();
            this.isConnected = true;
            console.log("SignalR: Conectado exitosamente");
            
            // Unirse a grupos por defecto
            await this.joinGroups();
            
        } catch (err) {
            console.error("Error al iniciar SignalR:", err);
        }
    }

    // Configurar los manejadores de eventos del hub
    setupEventHandlers() {
        if (!this.connection) return;

        // Evento: Ticket actualizado
        this.connection.on("TicketUpdated", (ticket) => {
            console.log("SignalR: Ticket actualizado", ticket);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnTicketUpdated", ticket);
            }
        });

        // Evento: Nuevo ticket creado
        this.connection.on("TicketCreated", (ticket) => {
            console.log("SignalR: Nuevo ticket creado", ticket);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnTicketCreated", ticket);
            }
        });

        // Evento: Ticket eliminado
        this.connection.on("TicketDeleted", (ticketId) => {
            console.log("SignalR: Ticket eliminado", ticketId);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnTicketDeleted", ticketId);
            }
        });

        // Evento: Nuevo comentario agregado
        this.connection.on("CommentAdded", (ticketId, comment) => {
            console.log("SignalR: Comentario agregado al ticket", ticketId);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnCommentAdded", ticketId, comment);
            }
        });

        // Evento: Estadísticas del dashboard actualizadas
        this.connection.on("DashboardStatsUpdated", (stats) => {
            console.log("SignalR: Estadísticas actualizadas", stats);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnDashboardStatsUpdated", stats);
            }
        });

        // Evento: Notificación general
        this.connection.on("NotificationReceived", (notification) => {
            console.log("SignalR: Notificación recibida", notification);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnNotificationReceived", notification);
            }
            
            // Mostrar notificación del navegador si está permitido
            this.showBrowserNotification(notification);
        });

        // Evento: Importación de emails completada
        this.connection.on("EmailImportCompleted", (result) => {
            console.log("SignalR: Importación completada", result);
            if (this.dotNetObjectRef) {
                this.dotNetObjectRef.invokeMethodAsync("OnEmailImportCompleted", result);
            }
        });
    }

    // Unirse a grupos de SignalR
    async joinGroups() {
        if (!this.connection || !this.isConnected) return;

        try {
            // Unirse al grupo general de tickets
            await this.connection.invoke("JoinGroup", "AllTickets");
            
            // Unirse al grupo de dashboard
            await this.connection.invoke("JoinGroup", "Dashboard");
            
            console.log("SignalR: Unido a grupos");
        } catch (err) {
            console.error("Error al unirse a grupos:", err);
        }
    }

    // Abandonar grupos de SignalR
    async leaveGroups() {
        if (!this.connection || !this.isConnected) return;

        try {
            await this.connection.invoke("LeaveGroup", "AllTickets");
            await this.connection.invoke("LeaveGroup", "Dashboard");
            console.log("SignalR: Abandonado grupos");
        } catch (err) {
            console.error("Error al abandonar grupos:", err);
        }
    }

    // Enviar notificación de que un ticket está siendo visualizado
    async notifyTicketViewing(ticketId) {
        if (!this.connection || !this.isConnected) return;

        try {
            await this.connection.invoke("NotifyTicketViewing", ticketId);
        } catch (err) {
            console.error("Error al notificar visualización:", err);
        }
    }

    // Mostrar notificación del navegador
    showBrowserNotification(notification) {
        // Verificar si las notificaciones están permitidas
        if ("Notification" in window && Notification.permission === "granted") {
            const options = {
                body: notification.message,
                icon: '/favicon.png',
                badge: '/favicon.png',
                tag: `ticket-${notification.ticketId || 'general'}`,
                requireInteraction: notification.priority === 'Critical'
            };

            const browserNotification = new Notification(notification.title, options);
            
            // Auto-cerrar después de 5 segundos (excepto para críticas)
            if (notification.priority !== 'Critical') {
                setTimeout(() => browserNotification.close(), 5000);
            }
            
            // Click handler
            browserNotification.onclick = () => {
                window.focus();
                browserNotification.close();
                
                // Navegar al ticket si está disponible
                if (notification.ticketId && this.dotNetObjectRef) {
                    this.dotNetObjectRef.invokeMethodAsync("NavigateToTicket", notification.ticketId);
                }
            };
        }
    }

    // Solicitar permisos de notificación
    async requestNotificationPermission() {
        if ("Notification" in window && Notification.permission === "default") {
            const permission = await Notification.requestPermission();
            return permission === "granted";
        }
        return Notification.permission === "granted";
    }

    // Detener la conexión
    async stop() {
        if (this.connection) {
            try {
                await this.leaveGroups();
                await this.connection.stop();
                console.log("SignalR: Desconectado");
            } catch (err) {
                console.error("Error al desconectar SignalR:", err);
            }
        }
    }

    // Verificar estado de conexión
    getConnectionState() {
        if (!this.connection) return "Disconnected";
        
        switch (this.connection.state) {
            case signalR.HubConnectionState.Connecting:
                return "Connecting";
            case signalR.HubConnectionState.Connected:
                return "Connected";
            case signalR.HubConnectionState.Reconnecting:
                return "Reconnecting";
            case signalR.HubConnectionState.Disconnected:
                return "Disconnected";
            default:
                return "Unknown";
        }
    }
}

// Instancia global
window.ticketSignalR = new TicketSignalRConnection();

// Funciones expuestas para Blazor
window.signalRFunctions = {
    start: async (dotNetObjectRef) => {
        return await window.ticketSignalR.start(dotNetObjectRef);
    },
    
    stop: async () => {
        return await window.ticketSignalR.stop();
    },
    
    notifyTicketViewing: async (ticketId) => {
        return await window.ticketSignalR.notifyTicketViewing(ticketId);
    },
    
    requestNotificationPermission: async () => {
        return await window.ticketSignalR.requestNotificationPermission();
    },
    
    getConnectionState: () => {
        return window.ticketSignalR.getConnectionState();
    }
};

// Auto-inicializar cuando la página esté lista
document.addEventListener('DOMContentLoaded', function() {
    console.log("SignalR: JavaScript cargado y listo");
});
