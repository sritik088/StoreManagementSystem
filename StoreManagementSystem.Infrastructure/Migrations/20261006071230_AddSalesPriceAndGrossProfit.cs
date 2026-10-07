
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPriceAndGrossProfit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // 1. SAFELY CONVERT SalesOrders.Status
            // Old database: nvarchar(30)
            // New enum:
            // Draft     = 1
            // Confirmed = 2
            // Cancelled = 3
            // ============================================================

            migrationBuilder.AddColumn<int>(
                name: "StatusTemp",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET StatusTemp = 1
                WHERE Status = 'Pending';
            ");

            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET StatusTemp = 2
                WHERE Status = 'Confirmed';
            ");

            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET StatusTemp = 3
                WHERE Status = 'Cancelled';
            ");

            // Any NULL or unexpected old status becomes Draft.
            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET StatusTemp = 1
                WHERE Status IS NULL
                   OR Status NOT IN ('Pending', 'Confirmed', 'Cancelled');
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesOrders");

            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "SalesOrders",
                newName: "Status");


            // ============================================================
            // 2. Existing SalesOrder column changes
            // ============================================================

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "SalesOrders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);


            // ============================================================
            // 3. Add SalesOrder new columns
            // ============================================================

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "SalesOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(
                    1,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    DateTimeKind.Unspecified));

            // Populate existing records with their OrderDate.
            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET CreatedDate = OrderDate;
            ");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossProfit",
                table: "SalesOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);


            // ============================================================
            // 4. SalesOrderItems percentage precision
            // ============================================================

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxPercent",
                table: "SalesOrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "SalesOrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 5,
                oldScale: 2);


            // ============================================================
            // 5. Add SalesOrderItem SalePrice + GrossProfit
            // ============================================================

            migrationBuilder.AddColumn<decimal>(
                name: "GrossProfit",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // Remove new SalesOrderItem columns
            // ============================================================

            migrationBuilder.DropColumn(
                name: "GrossProfit",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "SalesOrderItems");


            // ============================================================
            // Remove new SalesOrder columns
            // ============================================================

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "GrossProfit",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SalesOrders");


            // ============================================================
            // Restore old string SalesOrders.Status
            //
            // New:
            // 1 = Draft
            // 2 = Confirmed
            // 3 = Cancelled
            //
            // Old:
            // Pending
            // Confirmed
            // Cancelled
            // ============================================================

            migrationBuilder.AddColumn<string>(
                name: "StatusTemp",
                table: "SalesOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql(@"
                UPDATE SalesOrders
                SET StatusTemp =
                    CASE Status
                        WHEN 1 THEN 'Pending'
                        WHEN 2 THEN 'Confirmed'
                        WHEN 3 THEN 'Cancelled'
                        ELSE 'Pending'
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesOrders");

            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "SalesOrders",
                newName: "Status");


            // ============================================================
            // Restore old SalesOrder column definitions
            // ============================================================

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "SalesOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "SalesOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "SalesOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "SalesOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);


            // ============================================================
            // Restore SalesOrderItem percentage precision
            // ============================================================

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxPercent",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);
        }
    }
}
