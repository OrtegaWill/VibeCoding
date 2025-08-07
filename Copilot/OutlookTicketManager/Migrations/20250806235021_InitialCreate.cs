using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutlookTicketManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FromEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FromDomain = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SubjectKeywords = table.Column<string>(type: "TEXT", nullable: true),
                    BodyKeywords = table.Column<string>(type: "TEXT", nullable: true),
                    AutoCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AutoPriority = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoAssignTo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailId = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    ConversationId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FromEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FromName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ActualHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsSystemComment = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EmailFilters",
                columns: new[] { "Id", "AutoAssignTo", "AutoCategory", "AutoPriority", "BodyKeywords", "CreatedDate", "FromDomain", "FromEmail", "IsActive", "Name", "SubjectKeywords" },
                values: new object[,]
                {
                    { 1, null, "Consulta Interna", 1, null, new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940), "@cognizant.com", null, true, "Filtro Cognizant", null },
                    { 2, null, "Bug", 2, null, new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940), null, null, true, "Bugs Críticos", "bug,error,critical,urgente" },
                    { 3, null, "Soporte", 1, null, new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940), null, null, true, "Solicitudes de Soporte", "soporte,ayuda,problema,issue" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "ActualHours", "AssignedTo", "Body", "Category", "ConversationId", "CreatedDate", "Description", "EmailId", "EstimatedHours", "FromEmail", "FromName", "LastUpdated", "Priority", "ResolvedDate", "Status", "Subject", "Tags", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 4.0m, "admin@company.com", null, "Bug", null, new DateTime(2025, 8, 3, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1040), "El sistema de facturación presenta errores al generar reportes mensuales. Los usuarios reportan que no pueden acceder a los documentos.", "sample-email-001", 8.5m, "juan.perez@cognizant.com", "Juan Pérez", new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1050), 2, null, 1, "Error en sistema de facturación", "facturación,bug,urgente", new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1050) },
                    { 2, null, null, null, "Solicitud", null, new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), "Se requiere crear una nueva cuenta de usuario para María González del departamento de ventas.", "sample-email-002", 2.0m, "hr@company.com", "Recursos Humanos", new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), 1, null, 0, "Solicitud de nuevo usuario", "usuario,acceso", new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060) },
                    { 3, 14.5m, "admin@company.com", null, "Mejora", null, new DateTime(2025, 7, 30, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), "Implementar nuevas políticas de seguridad según las directrices corporativas actualizadas.", "sample-email-003", 16.0m, "security@company.com", "Equipo de Seguridad", new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), 0, null, 3, "Actualización de políticas de seguridad", "seguridad,políticas,mejora", new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060) },
                    { 4, 2.0m, "techsupport@company.com", null, "Soporte", null, new DateTime(2025, 8, 6, 17, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070), "Varios usuarios reportan problemas intermitentes de conectividad a internet en el tercer piso.", "sample-email-004", 4.0m, "soporte@company.com", "Mesa de Ayuda", new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070), 2, null, 2, "Problema de conectividad en red", "red,conectividad,infraestructura", new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070) }
                });

            migrationBuilder.InsertData(
                table: "TicketComments",
                columns: new[] { "Id", "Author", "AuthorEmail", "AuthorName", "Comment", "Content", "CreatedDate", "IsSystemComment", "TicketId" },
                values: new object[,]
                {
                    { 1, "Sistema", "system@company.com", "Sistema", "Ticket asignado al equipo de desarrollo para investigación.", "Ticket asignado al equipo de desarrollo para investigación.", new DateTime(2025, 8, 3, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090), true, 1 },
                    { 2, "Admin Usuario", "admin@company.com", "Admin Usuario", "He identificado el problema. Está relacionado con la actualización reciente de la base de datos. Trabajando en la solución.", "He identificado el problema. Está relacionado con la actualización reciente de la base de datos. Trabajando en la solución.", new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090), false, 1 },
                    { 3, "Tech Support", "techsupport@company.com", "Tech Support", "El problema ha sido escalado al equipo de infraestructura de red.", "El problema ha sido escalado al equipo de infraestructura de red.", new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090), false, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailFilters_FromDomain",
                table: "EmailFilters",
                column: "FromDomain");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFilters_FromEmail",
                table: "EmailFilters",
                column: "FromEmail");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFilters_IsActive",
                table: "EmailFilters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CreatedDate",
                table: "TicketComments",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedDate",
                table: "Tickets",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EmailId",
                table: "Tickets",
                column: "EmailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FromEmail",
                table: "Tickets",
                column: "FromEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Priority",
                table: "Tickets",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailFilters");

            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
