using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NexusFlow.Migrations
{
    /// <inheritdoc />
    public partial class SeedBusinessCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BusinessCategories",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("123e4567-e89b-12d3-a456-426614174000"), "Barbería", "barberia" },
                    { new Guid("6ec0bd7f-11c0-43da-975e-2a8ad9ebae0b"), "Peluquería", "peluqueria" },
                    { new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Salón de uñas", "salon-de-unas" },
                    { new Guid("f81d4fae-7dec-11d0-a765-00a0c91e6bf6"), "Estética", "estetica" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: new Guid("123e4567-e89b-12d3-a456-426614174000"));

            migrationBuilder.DeleteData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: new Guid("6ec0bd7f-11c0-43da-975e-2a8ad9ebae0b"));

            migrationBuilder.DeleteData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"));

            migrationBuilder.DeleteData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: new Guid("f81d4fae-7dec-11d0-a765-00a0c91e6bf6"));
        }
    }
}
