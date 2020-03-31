using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServiceManager.Data.Migrations
{
    public partial class MetaData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    ModifiedStatusDate = table.Column<DateTime>(nullable: true),
                    StatusModifiedBy = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true),
                    WorkServiceID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaData_WorkOrder_WorkServiceID",
                        column: x => x.WorkServiceID,
                        principalTable: "WorkOrder",
                        principalColumn: "WorkServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 1,
                column: "Skill",
                value: "Welder");

            migrationBuilder.CreateIndex(
                name: "IX_MetaData_WorkServiceID",
                table: "MetaData",
                column: "WorkServiceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaData");

            migrationBuilder.UpdateData(
                table: "Profession",
                keyColumn: "Id",
                keyValue: 1,
                column: "Skill",
                value: "Weilder");
        }
    }
}
