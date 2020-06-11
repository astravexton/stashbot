using Microsoft.EntityFrameworkCore.Migrations;

namespace StashBot.Migrations
{
    public partial class Modify_Author_001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramUsernameUpper",
                table: "Authors",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUsernameUpper",
                table: "Authors");
        }
    }
}
