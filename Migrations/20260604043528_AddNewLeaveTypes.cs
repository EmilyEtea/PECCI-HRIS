using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class AddNewLeaveTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LeaveTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenderRestriction",
                table: "LeaveTypes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDateSpecific",
                table: "LeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 1,
                columns: new[] { "GenderRestriction", "IsDateSpecific" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 2,
                columns: new[] { "GenderRestriction", "IsDateSpecific" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 3,
                columns: new[] { "GenderRestriction", "IsDateSpecific" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 4,
                columns: new[] { "GenderRestriction", "IsDateSpecific" },
                values: new object[] { "Female", false });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 5,
                columns: new[] { "GenderRestriction", "IsDateSpecific", "LeaveCode" },
                values: new object[] { "Male", false, "PTL" });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "LeaveTypeID", "CreatedAt", "DefaultDaysPerYear", "Description", "GenderRestriction", "IsActive", "IsDateSpecific", "IsPaid", "LeaveCode", "LeaveTypeName", "RequiresApproval" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "5 consecutive working days for death of an immediate family member. Requires registered death certificate. Immediate family: legitimate parents, siblings, spouse, children, legally adopted children, newborn child (35–36 weeks gestation). Source: PECCI HR Policy Manual Part III §8.", null, true, false, true, "BRL", "Bereavement Leave", true },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 60, "Special Leave Benefit for Women — 2 months (up to 60 working days) for surgery due to gynecological disorders. Female employees only. Requires medical certificate and surgeon's certification. Source: RA 9710 (Magna Carta of Women), RA 9262; PECCI HR Policy Manual Part III §3 (SL).", "Female", true, false, true, "MCL", "Magna Carta Leave", true },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, "Used to convert an unplanned absence into a formal leave entry. Unpaid — the corresponding salary deduction for the absent day is retained. Credits are not pre-allocated; HR grants individual credits when converting an absence on record. Source: PECCI HR Policy Manual Part II §5 (Working Conditions — AWOL).", null, true, false, false, "LLA", "Leave in Lieu of Absence", true },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "1 day paid leave on the employee's birthday. If the birthday falls on a holiday, the following workday off with regular pay is granted. Source: PECCI HR Policy Manual Part III §6.", null, true, true, true, "BDL", "Birthday Leave", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 9);

            migrationBuilder.DropColumn(
                name: "GenderRestriction",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "IsDateSpecific",
                table: "LeaveTypes");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeID",
                keyValue: 5,
                column: "LeaveCode",
                value: "PL");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LeaveTypes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
