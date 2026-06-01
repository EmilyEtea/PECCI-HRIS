using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    // ── Submit OT Request ─────────────────────────────────────────────────────

    public class OvertimeRequestViewModel
    {
        public int OvertimeRequestID { get; set; }

        public int? EmployeeID { get; set; }

        [Required(ErrorMessage = "Overtime date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Overtime Date")]
        public DateTime OvertimeDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start Time")]
        public string StartTimeString { get; set; } = "17:00";

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End Time")]
        public string EndTimeString { get; set; } = "19:00";

        // Parsed helpers used by the controller
        public TimeSpan StartTime => TimeSpan.TryParse(StartTimeString, out var t) ? t : TimeSpan.Zero;
        public TimeSpan EndTime   => TimeSpan.TryParse(EndTimeString,   out var t) ? t : TimeSpan.Zero;

        [Required(ErrorMessage = "Reason / purpose is required.")]
        [Display(Name = "Reason / Purpose")]
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    // ── Review (Approve / Disapprove) ─────────────────────────────────────────

    public class OvertimeReviewViewModel
    {
        public int OvertimeRequestID { get; set; }

        /// <summary>"Approve" or "Disapprove"</summary>
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [MaxLength(300)]
        public string? Remarks { get; set; }

        /// <summary>
        /// HR can adjust the approved minutes (e.g. cap at a maximum).
        /// Only used when Action = "Approve" at the HR stage.
        /// </summary>
        [Display(Name = "Approved Minutes")]
        [Range(1, 1440, ErrorMessage = "Approved minutes must be between 1 and 1440.")]
        public double? ApprovedMinutes { get; set; }
    }
}
