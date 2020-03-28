using Microsoft.EntityFrameworkCore.Migrations;

namespace ServiceManager.Data.Migrations
{
    public partial class Proffesional_Skill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Professional_Skill",
                table: "AspNetUsers",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Professional_Skill",
                table: "AspNetUsers");
        }
    }
}
