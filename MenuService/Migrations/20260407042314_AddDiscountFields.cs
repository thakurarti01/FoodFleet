using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuService.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEnd",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Items",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStart",
                table: "Items",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEnd",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DiscountStart",
                table: "Items");
        }
    }
}
