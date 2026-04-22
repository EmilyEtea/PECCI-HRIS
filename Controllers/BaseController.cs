using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PECCI_HRIS.Controllers
{
    public class BaseController : Controller
    {
        protected int GetCurrentUserID()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        protected string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        }

        protected string GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        protected string GetClientIP()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        protected bool IsHRAdmin() => GetCurrentRole() == "HR Admin";
        protected bool IsHRStaff() => GetCurrentRole() == "HR Staff" || IsHRAdmin();
        protected bool IsManager() => GetCurrentRole() == "Manager" || IsHRAdmin();
    }
}
