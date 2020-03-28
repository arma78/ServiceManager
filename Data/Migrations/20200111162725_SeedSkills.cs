using Microsoft.EntityFrameworkCore.Migrations;

namespace ServiceManager.Data.Migrations
{
    public partial class SeedSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Profession",
                columns: new[] { "Id", "Skill" },
                values: new object[,]
                {
                    { 1, "Weilder" },
                    { 2, "Brick Layer" },
                    { 3, "Electrician" },
                    { 4, "Hardwood Floor Installer" },
                    { 5, "Tile Installer" },
                    { 6, "Plumber" },
                    { 7, "Drywall Installer" },
                    { 8, "Insulation Installer" },
                    { 9, "Kitchen Cabinet Installer" },
                    { 10, "Framer" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
