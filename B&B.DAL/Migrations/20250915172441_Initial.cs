using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B_B.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipt_Client_ClientId",
                table: "Receipt");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipt_Supplier_SupplierId",
                table: "Receipt");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetail_Product_ProductId",
                table: "ReceiptDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetail_Receipt_ReceiptId",
                table: "ReceiptDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransaction_Product_ProductId",
                table: "StockTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransaction_Receipt_ReceiptId",
                table: "StockTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockTransaction",
                table: "StockTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptDetail",
                table: "ReceiptDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipt",
                table: "Receipt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Client",
                table: "Client");

            migrationBuilder.RenameTable(
                name: "Supplier",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "StockTransaction",
                newName: "StockTransactions");

            migrationBuilder.RenameTable(
                name: "ReceiptDetail",
                newName: "ReceiptDetails");

            migrationBuilder.RenameTable(
                name: "Receipt",
                newName: "Receipts");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Client",
                newName: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_StockTransaction_ReceiptId",
                table: "StockTransactions",
                newName: "IX_StockTransactions_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_StockTransaction_ProductId",
                table: "StockTransactions",
                newName: "IX_StockTransactions_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptDetail_ReceiptId",
                table: "ReceiptDetails",
                newName: "IX_ReceiptDetails_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptDetail_ProductId",
                table: "ReceiptDetails",
                newName: "IX_ReceiptDetails_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipt_SupplierId",
                table: "Receipts",
                newName: "IX_Receipts_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipt_ClientId",
                table: "Receipts",
                newName: "IX_Receipts_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockTransactions",
                table: "StockTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptDetails",
                table: "ReceiptDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_Products_ProductId",
                table: "ReceiptDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_Receipts_ReceiptId",
                table: "ReceiptDetails",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Clients_ClientId",
                table: "Receipts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Receipts_ReceiptId",
                table: "StockTransactions",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_Products_ProductId",
                table: "ReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_Receipts_ReceiptId",
                table: "ReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Clients_ClientId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Receipts_ReceiptId",
                table: "StockTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockTransactions",
                table: "StockTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptDetails",
                table: "ReceiptDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Supplier");

            migrationBuilder.RenameTable(
                name: "StockTransactions",
                newName: "StockTransaction");

            migrationBuilder.RenameTable(
                name: "Receipts",
                newName: "Receipt");

            migrationBuilder.RenameTable(
                name: "ReceiptDetails",
                newName: "ReceiptDetail");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "Client");

            migrationBuilder.RenameIndex(
                name: "IX_StockTransactions_ReceiptId",
                table: "StockTransaction",
                newName: "IX_StockTransaction_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransaction",
                newName: "IX_StockTransaction_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_SupplierId",
                table: "Receipt",
                newName: "IX_Receipt_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_ClientId",
                table: "Receipt",
                newName: "IX_Receipt_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptDetails_ReceiptId",
                table: "ReceiptDetail",
                newName: "IX_ReceiptDetail_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptDetails_ProductId",
                table: "ReceiptDetail",
                newName: "IX_ReceiptDetail_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockTransaction",
                table: "StockTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipt",
                table: "Receipt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptDetail",
                table: "ReceiptDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Client",
                table: "Client",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipt_Client_ClientId",
                table: "Receipt",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipt_Supplier_SupplierId",
                table: "Receipt",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetail_Product_ProductId",
                table: "ReceiptDetail",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetail_Receipt_ReceiptId",
                table: "ReceiptDetail",
                column: "ReceiptId",
                principalTable: "Receipt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransaction_Product_ProductId",
                table: "StockTransaction",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransaction_Receipt_ReceiptId",
                table: "StockTransaction",
                column: "ReceiptId",
                principalTable: "Receipt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
