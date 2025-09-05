using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace xeepconcesionario.Migrations
{
    /// <inheritdoc />
    public partial class AgregoValores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobros_Cobradores_CobradorId",
                table: "Cobros");

            migrationBuilder.DropTable(
                name: "Cobradores");

            migrationBuilder.DropTable(
                name: "ValoresPlan");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_CobradorId",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "CobradorId",
                table: "Cobros");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Vehiculos",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Monto",
                table: "ActividadesVehiculo",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Monto",
                table: "ActividadesVehiculo");

            migrationBuilder.AddColumn<int>(
                name: "CobradorId",
                table: "Cobros",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cobradores",
                columns: table => new
                {
                    CobradorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCobrador = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobradores", x => x.CobradorId);
                });

            migrationBuilder.CreateTable(
                name: "ValoresPlan",
                columns: table => new
                {
                    ValorPlanId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    FechaValor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValoresPlan", x => x.ValorPlanId);
                    table.ForeignKey(
                        name: "FK_ValoresPlan_Planes_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Planes",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_CobradorId",
                table: "Cobros",
                column: "CobradorId");

            migrationBuilder.CreateIndex(
                name: "IX_ValoresPlan_PlanId",
                table: "ValoresPlan",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobros_Cobradores_CobradorId",
                table: "Cobros",
                column: "CobradorId",
                principalTable: "Cobradores",
                principalColumn: "CobradorId");
        }
    }
}
