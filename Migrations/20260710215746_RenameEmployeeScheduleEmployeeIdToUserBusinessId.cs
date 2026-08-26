using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusFlow.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeScheduleEmployeeIdToUserBusinessId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSchedule_UsersBusiness_EmployeeId",
                table: "EmployeeSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeSchedule",
                table: "EmployeeSchedule");

            migrationBuilder.RenameTable(
                name: "EmployeeSchedule",
                newName: "EmployeeSchedules");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeSchedules",
                newName: "UserBusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeSchedule_EmployeeId",
                table: "EmployeeSchedules",
                newName: "IX_EmployeeSchedules_UserBusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeSchedules",
                table: "EmployeeSchedules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSchedules_UsersBusiness_UserBusinessId",
                table: "EmployeeSchedules",
                column: "UserBusinessId",
                principalTable: "UsersBusiness",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSchedules_UsersBusiness_UserBusinessId",
                table: "EmployeeSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeSchedules",
                table: "EmployeeSchedules");

            migrationBuilder.RenameTable(
                name: "EmployeeSchedules",
                newName: "EmployeeSchedule");

            migrationBuilder.RenameColumn(
                name: "UserBusinessId",
                table: "EmployeeSchedule",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeSchedules_UserBusinessId",
                table: "EmployeeSchedule",
                newName: "IX_EmployeeSchedule_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeSchedule",
                table: "EmployeeSchedule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSchedule_UsersBusiness_EmployeeId",
                table: "EmployeeSchedule",
                column: "EmployeeId",
                principalTable: "UsersBusiness",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
