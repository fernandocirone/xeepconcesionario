using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xeepconcesionario.Migrations
{
    /// <inheritdoc />
    public partial class preciocompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompra",
                table: "Vehiculos",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioCompra",
                table: "Vehiculos");
        }
    }
}
