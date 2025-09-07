using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xeepconcesionario.Migrations
{
    /// <inheritdoc />
    public partial class tipovehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Vehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Vehiculos");
        }
    }
}
