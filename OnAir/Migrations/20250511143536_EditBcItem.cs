using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnAir.Migrations
{
    /// <inheritdoc />
    public partial class EditBcItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndexInBroadcast",
                table: "BroadcastItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexInBroadcast",
                table: "BroadcastItems");
        }
    }
}
