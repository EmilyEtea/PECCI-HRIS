using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    HolidayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HolidayType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.HolidayID);
                });

            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "HolidayID", "CreatedAt", "CreatedBy", "HolidayDate", "HolidayName", "HolidayType", "IsRecurring", "Remarks", "Year" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day", "Regular", true, null, 2026 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maundy Thursday", "Regular", false, null, 2026 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday", "Regular", false, null, 2026 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Araw ng Kagitingan (Day of Valor)", "Regular", true, null, 2026 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Labor Day", "Regular", true, null, 2026 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 6, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Independence Day", "Regular", true, null, 2026 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Heroes Day", "Regular", false, "Last Monday of August", 2026 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bonifacio Day", "Regular", true, null, 2026 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day", "Regular", true, null, 2026 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rizal Day", "Regular", true, null, 2026 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chinese New Year", "Special", false, null, 2026 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "EDSA People Power Revolution Anniversary", "Special", true, null, 2026 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Black Saturday", "Special", false, null, 2026 },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ninoy Aquino Day", "Special", true, null, 2026 },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "All Saints' Day", "Special", true, null, 2026 },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "All Souls' Day", "Special", false, null, 2026 },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Feast of the Immaculate Conception", "Special", true, null, 2026 },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Eve", "Special", false, null, 2026 },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Eve", "Special", false, null, 2026 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_HolidayDate_HolidayType",
                table: "Holidays",
                columns: new[] { "HolidayDate", "HolidayType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");
        }
    }
}
