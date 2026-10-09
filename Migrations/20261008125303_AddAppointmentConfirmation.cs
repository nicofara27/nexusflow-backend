using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationTokenExpiresAt",
                table: "Appointments",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationTokenHash",
                table: "Appointments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Appointments",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationTokenExpiresAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmationTokenHash",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Appointments");
        }
    }
}
