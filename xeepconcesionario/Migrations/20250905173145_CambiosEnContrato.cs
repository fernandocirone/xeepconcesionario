using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xeepconcesionario.Migrations
{
    /// <inheritdoc />
    public partial class CambiosEnContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Contratos_ContratoId",
                table: "Solicitudes");

            migrationBuilder.DropIndex(
                name: "IX_Solicitudes_ContratoId",
                table: "Solicitudes");

            migrationBuilder.AlterColumn<string>(
                name: "NombreContrato",
                table: "Contratos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "CantidadCuotas",
                table: "Contratos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCuota",
                table: "Contratos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoPagadoAcumulado",
                table: "Contratos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SolicitudId",
                table: "Contratos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTransferencia",
                table: "Contratos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VehiculoId",
                table: "Contratos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_SolicitudId",
                table: "Contratos",
                column: "SolicitudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_VehiculoId",
                table: "Contratos",
                column: "VehiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Solicitudes_SolicitudId",
                table: "Contratos",
                column: "SolicitudId",
                principalTable: "Solicitudes",
                principalColumn: "SolicitudId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Vehiculos_VehiculoId",
                table: "Contratos",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Solicitudes_SolicitudId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Vehiculos_VehiculoId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_SolicitudId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_VehiculoId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "CantidadCuotas",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "MontoCuota",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "MontoPagadoAcumulado",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "SolicitudId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "ValorTransferencia",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "VehiculoId",
                table: "Contratos");

            migrationBuilder.AlterColumn<string>(
                name: "NombreContrato",
                table: "Contratos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_ContratoId",
                table: "Solicitudes",
                column: "ContratoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Contratos_ContratoId",
                table: "Solicitudes",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "ContratoId");
        }
    }
}
