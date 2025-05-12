using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnAir.Migrations
{
    /// <inheritdoc />
    public partial class EditBcAddEndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "PlannedEndTime",
                table: "Broadcasts",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedEndTime",
                table: "Broadcasts");
        }
    }
}
