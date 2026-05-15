using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class FixHolidaySeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix: Chinese New Year 2026 was seeded as Jan 28 (incorrect).
            // The correct date is Feb 17, 2026.
            migrationBuilder.Sql(@"
                UPDATE Holidays
                SET HolidayDate = '2026-02-17'
                WHERE HolidayDate = '2026-01-28'
                  AND HolidayName = 'Chinese New Year'
                  AND Year = 2026;
            ");

            // Add missing Eid'l Fitr 2026 (Mar 20) if not already present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM Holidays
                    WHERE HolidayDate = '2026-03-20' AND HolidayType = 'Regular'
                )
                INSERT INTO Holidays (HolidayDate, HolidayName, HolidayType, Year, IsRecurring, Remarks, CreatedAt, CreatedBy)
                VALUES ('2026-03-20', 'Eid''l Fitr (Feast of Ramadan)', 'Regular', 2026, 0,
                        'Approximate — confirm via official Proclamation', GETDATE(), NULL);
            ");

            // Add missing Eid'l Adha 2026 (May 27) if not already present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM Holidays
                    WHERE HolidayDate = '2026-05-27' AND HolidayType = 'Regular'
                )
                INSERT INTO Holidays (HolidayDate, HolidayName, HolidayType, Year, IsRecurring, Remarks, CreatedAt, CreatedBy)
                VALUES ('2026-05-27', 'Eid''l Adha (Feast of Sacrifice)', 'Regular', 2026, 0,
                        'Approximate — confirm via official Proclamation', GETDATE(), NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Chinese New Year date back to original (incorrect) seed
            migrationBuilder.Sql(@"
                UPDATE Holidays
                SET HolidayDate = '2026-01-28'
                WHERE HolidayDate = '2026-02-17'
                  AND HolidayName = 'Chinese New Year'
                  AND Year = 2026;
            ");

            // Remove the Eid entries added by this migration
            migrationBuilder.Sql(@"
                DELETE FROM Holidays
                WHERE Year = 2026
                  AND HolidayName IN ('Eid''l Fitr (Feast of Ramadan)', 'Eid''l Adha (Feast of Sacrifice)');
            ");
        }
    }
}
