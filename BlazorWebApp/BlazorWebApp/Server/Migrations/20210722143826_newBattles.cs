using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorWebApp.Server.Migrations
{
    public partial class newBattles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BattleData",
                table: "Battles",
                newName: "BattleDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BattleDate",
                table: "Battles",
                newName: "BattleData");
        }
    }
}
