using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlumberEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlumberId",
                table: "Receipts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Plumber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plumber", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PlumberId",
                table: "Receipts",
                column: "PlumberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Plumber_PlumberId",
                table: "Receipts",
                column: "PlumberId",
                principalTable: "Plumber",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Plumber_PlumberId",
                table: "Receipts");

            migrationBuilder.DropTable(
                name: "Plumber");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_PlumberId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PlumberId",
                table: "Receipts");
        }
    }
}
