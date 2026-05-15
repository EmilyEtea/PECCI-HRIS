using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class AddNightDifferentialMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "NightDifferentialMinutes",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NightDifferentialMinutes",
                table: "AttendanceRecords");
        }
    }
}
