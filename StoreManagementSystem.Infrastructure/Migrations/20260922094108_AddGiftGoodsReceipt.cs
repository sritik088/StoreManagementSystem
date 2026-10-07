using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftGoodsReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Suppliers_SupplierId",
                table: "GoodsReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "GoodsReceipts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "GoodsReceipts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DonorName",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiftReason",
                table: "GoodsReceipts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptType",
                table: "GoodsReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderItemId",
                table: "GoodsReceiptItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Suppliers_SupplierId",
                table: "GoodsReceipts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Suppliers_SupplierId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "DonorName",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "GiftReason",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ReceiptType",
                table: "GoodsReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "GoodsReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "GoodsReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderItemId",
                table: "GoodsReceiptItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Suppliers_SupplierId",
                table: "GoodsReceipts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
