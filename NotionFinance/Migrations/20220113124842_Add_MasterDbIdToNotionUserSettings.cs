using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionFinance.Migrations
{
    public partial class Add_MasterDbIdToNotionUserSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasterDatabaseId",
                table: "NotionUserSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterDatabaseId",
                table: "NotionUserSettings");
        }
    }
}
