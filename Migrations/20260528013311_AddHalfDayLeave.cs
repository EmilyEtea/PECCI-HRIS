using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class AddHalfDayLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HalfDayPeriod",
                table: "LeaveApplications",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHalfDay",
                table: "LeaveApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HalfDayPeriod",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "IsHalfDay",
                table: "LeaveApplications");
        }
    }
}
