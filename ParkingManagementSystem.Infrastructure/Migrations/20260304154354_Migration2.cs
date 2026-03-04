using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastReservedDate",
                table: "ParkingSpots",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReservedDate",
                table: "ParkingSpots");
        }
    }
}
