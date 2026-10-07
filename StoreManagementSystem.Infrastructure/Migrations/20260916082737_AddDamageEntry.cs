using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDamageEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DamageEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DamageNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DamageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DamageId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamageEntries_Damages_DamageId",
                        column: x => x.DamageId,
                        principalTable: "Damages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamageEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamageEntries_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamageEntries_DamageId",
                table: "DamageEntries",
                column: "DamageId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageEntries_DamageNumber",
                table: "DamageEntries",
                column: "DamageNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageEntries_ProductId",
                table: "DamageEntries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageEntries_WarehouseId",
                table: "DamageEntries",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamageEntries");
        }
    }
}
