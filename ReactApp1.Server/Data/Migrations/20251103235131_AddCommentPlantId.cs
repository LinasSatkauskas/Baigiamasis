using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactApp1.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentPlantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @sql := (SELECT IF(
    EXISTS(
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'Pests'
          AND COLUMN_NAME = 'Description'
    ),
    'ALTER TABLE `Pests` DROP COLUMN `Description`;'
    ,
    'SELECT 1'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.AddColumn<int>(
                name: "PlantId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PlantId",
                table: "Comments",
                column: "PlantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Plants_PlantId",
                table: "Comments",
                column: "PlantId",
                principalTable: "Plants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Plants_PlantId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PlantId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "PlantId",
                table: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
