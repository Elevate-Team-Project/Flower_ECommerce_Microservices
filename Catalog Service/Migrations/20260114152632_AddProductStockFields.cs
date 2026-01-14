using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStockFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ProductReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaxStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ProductReviews");
        }
    }
}
