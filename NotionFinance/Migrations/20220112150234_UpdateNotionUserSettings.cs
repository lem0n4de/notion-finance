using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionFinance.Migrations
{
    public partial class UpdateNotionUserSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MasterDatabaseCreationTime",
                table: "NotionUserSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MasterDatabaseFetchingFailedCount",
                table: "NotionUserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterDatabaseCreationTime",
                table: "NotionUserSettings");

            migrationBuilder.DropColumn(
                name: "MasterDatabaseFetchingFailedCount",
                table: "NotionUserSettings");
        }
    }
}
