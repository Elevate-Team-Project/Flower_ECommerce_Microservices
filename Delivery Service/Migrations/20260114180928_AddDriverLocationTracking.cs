using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverLocationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "UserAddresses",
                newName: "Governorate");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Apartment",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "UserAddresses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "UserAddresses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DriverLatitude",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DriverLongitude",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhone",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhotoUrl",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdate",
                table: "Shipments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apartment",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Building",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Landmark",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "DriverLatitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverLongitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverPhone",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverPhotoUrl",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdate",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "Governorate",
                table: "UserAddresses",
                newName: "State");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
