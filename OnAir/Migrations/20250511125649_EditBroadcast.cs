using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnAir.Migrations
{
    /// <inheritdoc />
    public partial class EditBroadcast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "BroadcastItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Part",
                table: "BroadcastItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rights",
                table: "BroadcastItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Series",
                table: "BroadcastItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customer",
                table: "BroadcastItems");

            migrationBuilder.DropColumn(
                name: "Part",
                table: "BroadcastItems");

            migrationBuilder.DropColumn(
                name: "Rights",
                table: "BroadcastItems");

            migrationBuilder.DropColumn(
                name: "Series",
                table: "BroadcastItems");
        }
    }
}
