using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DbsetPlumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Plumber_PlumberId",
                table: "Receipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Plumber",
                table: "Plumber");

            migrationBuilder.RenameTable(
                name: "Plumber",
                newName: "Plumbers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Plumbers",
                table: "Plumbers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Plumbers_PlumberId",
                table: "Receipts",
                column: "PlumberId",
                principalTable: "Plumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Plumbers_PlumberId",
                table: "Receipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Plumbers",
                table: "Plumbers");

            migrationBuilder.RenameTable(
                name: "Plumbers",
                newName: "Plumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Plumber",
                table: "Plumber",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Plumber_PlumberId",
                table: "Receipts",
                column: "PlumberId",
                principalTable: "Plumber",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
