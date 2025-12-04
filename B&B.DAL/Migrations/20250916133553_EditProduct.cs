using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityInStock",
                table: "Products",
                newName: "Sold");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "SellingPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Purchases",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Refunds",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Purchases",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Refunds",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Sold",
                table: "Products",
                newName: "QuantityInStock");

            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "Products",
                newName: "Price");
        }
    }
}
