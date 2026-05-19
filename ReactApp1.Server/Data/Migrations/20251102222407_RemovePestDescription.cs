using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactApp1.Server.Data.Migrations
{
    public partial class RemovePestDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop column only if it exists (MySQL/Pomelo)
            migrationBuilder.Sql(@"
SET @sql := (SELECT IF(
    EXISTS(
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'Pests'
          AND COLUMN_NAME = 'Description'
    ),
    'ALTER TABLE `Pests` DROP COLUMN `Description`;',
    'SELECT 1'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pests",
                type: "longtext",
                nullable: true);
        }
    }
}
