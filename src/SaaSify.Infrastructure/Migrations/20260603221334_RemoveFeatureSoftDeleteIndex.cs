using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFeatureSoftDeleteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "features",
                newName: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "features",
                newName: "deleted_at");
        }
    }
}
