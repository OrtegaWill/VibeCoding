using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutlookTicketManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Avance",
                table: "Tickets",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CausaRaiz",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Criticidad",
                table: "Tickets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetalleProblema",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAckPrecargas",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrupoAsignado",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrupoResolutor",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Historial",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdPeticion",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origen",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Precarga",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Problema",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuienAtiende",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfcSolicitudCambio",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolucionRemedy",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoResolucionHoras",
                table: "Tickets",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoQueja",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisorAplicativoAfectado",
                table: "Tickets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 11, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2310));

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 11, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2310));

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 11, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2320));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 8, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 10, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 11, 20, 33, 28, 295, DateTimeKind.Utc).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Apellidos", "Avance", "CausaRaiz", "CreatedDate", "Criticidad", "DetalleProblema", "FechaAckPrecargas", "FechaAsignacion", "GrupoAsignado", "GrupoResolutor", "Historial", "IdPeticion", "LastUpdated", "Nombre", "Origen", "Precarga", "Problema", "QuienAtiende", "RfcSolicitudCambio", "SolucionRemedy", "TiempoResolucionHoras", "TipoQueja", "UpdatedDate", "VisorAplicativoAfectado" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 8, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2420), null, null, null, null, null, null, null, null, new DateTime(2025, 8, 10, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2420), null, null, null, null, null, null, null, null, null, new DateTime(2025, 8, 10, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2420), null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Apellidos", "Avance", "CausaRaiz", "CreatedDate", "Criticidad", "DetalleProblema", "FechaAckPrecargas", "FechaAsignacion", "GrupoAsignado", "GrupoResolutor", "Historial", "IdPeticion", "LastUpdated", "Nombre", "Origen", "Precarga", "Problema", "QuienAtiende", "RfcSolicitudCambio", "SolucionRemedy", "TiempoResolucionHoras", "TipoQueja", "UpdatedDate", "VisorAplicativoAfectado" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 9, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2430), null, null, null, null, null, null, null, null, new DateTime(2025, 8, 9, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2430), null, null, null, null, null, null, null, null, null, new DateTime(2025, 8, 9, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2430), null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Apellidos", "Avance", "CausaRaiz", "CreatedDate", "Criticidad", "DetalleProblema", "FechaAckPrecargas", "FechaAsignacion", "GrupoAsignado", "GrupoResolutor", "Historial", "IdPeticion", "LastUpdated", "Nombre", "Origen", "Precarga", "Problema", "QuienAtiende", "RfcSolicitudCambio", "SolucionRemedy", "TiempoResolucionHoras", "TipoQueja", "UpdatedDate", "VisorAplicativoAfectado" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 4, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2440), null, null, null, null, null, null, null, null, new DateTime(2025, 8, 10, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2440), null, null, null, null, null, null, null, null, null, new DateTime(2025, 8, 10, 22, 33, 28, 295, DateTimeKind.Utc).AddTicks(2440), null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Apellidos", "Avance", "CausaRaiz", "CreatedDate", "Criticidad", "DetalleProblema", "FechaAckPrecargas", "FechaAsignacion", "GrupoAsignado", "GrupoResolutor", "Historial", "IdPeticion", "LastUpdated", "Nombre", "Origen", "Precarga", "Problema", "QuienAtiende", "RfcSolicitudCambio", "SolucionRemedy", "TiempoResolucionHoras", "TipoQueja", "UpdatedDate", "VisorAplicativoAfectado" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 11, 16, 33, 28, 295, DateTimeKind.Utc).AddTicks(2450), null, null, null, null, null, null, null, null, new DateTime(2025, 8, 11, 20, 33, 28, 295, DateTimeKind.Utc).AddTicks(2450), null, null, null, null, null, null, null, null, null, new DateTime(2025, 8, 11, 20, 33, 28, 295, DateTimeKind.Utc).AddTicks(2450), null });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Criticidad",
                table: "Tickets",
                column: "Criticidad");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_GrupoAsignado",
                table: "Tickets",
                column: "GrupoAsignado");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_IdPeticion",
                table: "Tickets",
                column: "IdPeticion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_Criticidad",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_GrupoAsignado",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_IdPeticion",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Avance",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CausaRaiz",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Criticidad",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DetalleProblema",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FechaAckPrecargas",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "GrupoAsignado",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "GrupoResolutor",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Historial",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IdPeticion",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Precarga",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Problema",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QuienAtiende",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RfcSolicitudCambio",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SolucionRemedy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TiempoResolucionHoras",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TipoQueja",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VisorAplicativoAfectado",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "EmailFilters",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 6, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 3, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090));

            migrationBuilder.UpdateData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1090));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "LastUpdated", "UpdatedDate" },
                values: new object[] { new DateTime(2025, 8, 3, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1040), new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1050), new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1050) });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "LastUpdated", "UpdatedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), new DateTime(2025, 8, 4, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060) });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "LastUpdated", "UpdatedDate" },
                values: new object[] { new DateTime(2025, 7, 30, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060), new DateTime(2025, 8, 5, 23, 50, 21, 651, DateTimeKind.Utc).AddTicks(1060) });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "LastUpdated", "UpdatedDate" },
                values: new object[] { new DateTime(2025, 8, 6, 17, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070), new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070), new DateTime(2025, 8, 6, 21, 50, 21, 651, DateTimeKind.Utc).AddTicks(1070) });
        }
    }
}
