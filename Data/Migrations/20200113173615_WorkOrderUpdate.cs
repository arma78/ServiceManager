using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServiceManager.Data.Migrations
{
    public partial class WorkOrderUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkOrderWorkServiceID",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrder",
                columns: table => new
                {
                    WorkServiceID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Property_Address = table.Column<string>(nullable: false),
                    Floor = table.Column<int>(nullable: false),
                    Unit = table.Column<string>(nullable: true),
                    WorkServiceName = table.Column<string>(nullable: true),
                    WorkService_Description = table.Column<string>(nullable: true),
                    RequestedBy = table.Column<string>(nullable: false),
                    Requested_Date = table.Column<DateTime>(nullable: false),
                    Contractor_Assigned = table.Column<string>(nullable: false),
                    Contractor_Comments = table.Column<string>(nullable: true),
                    Contractor_Start_Date = table.Column<DateTime>(nullable: true),
                    Contractor_Completion_Date = table.Column<DateTime>(nullable: true),
                    Service_Status = table.Column<string>(nullable: false),
                    FolderUrl = table.Column<string>(nullable: false),
                    Inspected_By = table.Column<string>(nullable: true),
                    Date_Inspected = table.Column<DateTime>(nullable: true),
                    Inspection_Comments = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.WorkServiceID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkOrderWorkServiceID",
                table: "AspNetUsers",
                column: "WorkOrderWorkServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_WorkOrder_WorkOrderWorkServiceID",
                table: "AspNetUsers",
                column: "WorkOrderWorkServiceID",
                principalTable: "WorkOrder",
                principalColumn: "WorkServiceID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_WorkOrder_WorkOrderWorkServiceID",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkOrderWorkServiceID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkOrderWorkServiceID",
                table: "AspNetUsers");
        }
    }
}
