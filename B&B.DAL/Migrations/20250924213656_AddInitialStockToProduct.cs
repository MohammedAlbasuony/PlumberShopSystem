using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialStockToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialStock",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialStock",
                table: "Products");
        }
    }
}
