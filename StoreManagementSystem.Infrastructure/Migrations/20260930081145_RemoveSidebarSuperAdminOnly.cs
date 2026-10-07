using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSidebarSuperAdminOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuperAdminOnly",
                table: "SidebarMenuItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SuperAdminOnly",
                table: "SidebarMenuItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
