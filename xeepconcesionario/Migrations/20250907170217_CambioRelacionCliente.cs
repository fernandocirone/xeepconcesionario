using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xeepconcesionario.Migrations
{
    /// <inheritdoc />
    public partial class CambioRelacionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Clientes_ClienteId",
                table: "Solicitudes");

            migrationBuilder.InsertData(
                table: "TiposUsuario",
                columns: new[] { "TipousuarioId", "Nombretipousuario" },
                values: new object[] { 4, "Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Clientes_ClienteId",
                table: "Solicitudes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "ClienteId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Clientes_ClienteId",
                table: "Solicitudes");

            migrationBuilder.DeleteData(
                table: "TiposUsuario",
                keyColumn: "TipousuarioId",
                keyValue: 4);

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Clientes_ClienteId",
                table: "Solicitudes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "ClienteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
