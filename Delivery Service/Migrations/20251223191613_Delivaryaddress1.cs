using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery_Service.Migrations
{
    /// <inheritdoc />
    public partial class Delivaryaddress1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FormattedAddress",
                table: "deliveryAddress",
                newName: "fullAddress");

            migrationBuilder.RenameColumn(
                name: "AddressText",
                table: "deliveryAddress",
                newName: "country");

            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "deliveryAddress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                table: "deliveryAddress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "deliveryAddress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "deliveryAddress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "phone",
                table: "deliveryAddress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "postalCode",
                table: "deliveryAddress",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Building",
                table: "deliveryAddress");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "deliveryAddress");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "deliveryAddress");

            migrationBuilder.DropColumn(
                name: "city",
                table: "deliveryAddress");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "deliveryAddress");

            migrationBuilder.DropColumn(
                name: "postalCode",
                table: "deliveryAddress");

            migrationBuilder.RenameColumn(
                name: "fullAddress",
                table: "deliveryAddress",
                newName: "FormattedAddress");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "deliveryAddress",
                newName: "AddressText");
        }
    }
}
