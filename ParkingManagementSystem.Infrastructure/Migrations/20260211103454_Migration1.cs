using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedParkingSpotId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "ParkingSpots",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedParkingSpotId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "ParkingSpots");
        }
    }
}
