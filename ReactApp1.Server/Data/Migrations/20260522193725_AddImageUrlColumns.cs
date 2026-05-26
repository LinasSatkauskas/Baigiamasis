using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactApp1.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE `Plants` ADD COLUMN IF NOT EXISTS `ImageUrl` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Pests` ADD COLUMN IF NOT EXISTS `ImageUrl` longtext CHARACTER SET utf8mb4 NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty to avoid errors when rolling back
            // if the ImageUrl columns do not exist in the database.
        }
    }
}
