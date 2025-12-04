using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class IsOldAddedToReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOld",
                table: "Receipts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOld",
                table: "Receipts");
        }
    }
}
