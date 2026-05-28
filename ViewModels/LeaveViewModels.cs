using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.ViewModels
{
    public class LeaveApplicationViewModel
    {
        public int LeaveApplicationID { get; set; }

        [Required, Display(Name = "Leave Type")]
        public int LeaveTypeID { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Half Day")]
        public bool IsHalfDay { get; set; } = false;

        /// <summary>AM = morning half, PM = afternoon half. Only relevant when IsHalfDay = true.</summary>
        [Display(Name = "Half Day Period")]
        public string HalfDayPeriod { get; set; } = "AM";

        [Display(Name = "Reason")]
        [MaxLength(500)]
        public string? Reason { get; set; }

        public int? EmployeeID { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<LeaveCredit> LeaveCredits { get; set; } = new List<LeaveCredit>();
    }

    public class LeaveApprovalViewModel
    {
        public int LeaveApplicationID { get; set; }
        public string Action { get; set; } = string.Empty; // Approve | Disapprove
        public string? Remarks { get; set; }
    }

    public class LeaveTypeViewModel
    {
        public int LeaveTypeID { get; set; }

        [Required, Display(Name = "Leave Type Name")]
        public string LeaveTypeName { get; set; } = string.Empty;

        [Display(Name = "Code")]
        [MaxLength(10)]
        public string? LeaveCode { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required, Display(Name = "Default Days Per Year")]
        public int DefaultDaysPerYear { get; set; }

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; } = true;

        [Display(Name = "Requires Approval")]
        public bool RequiresApproval { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
