using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulPanel.Migrations
{
    /// <inheritdoc />
    public partial class init11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PageKey",
                table: "UserPermissions",
                newName: "PermissionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PermissionKey",
                table: "UserPermissions",
                newName: "PageKey");
        }
    }
}
