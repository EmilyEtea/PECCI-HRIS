using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PECCI_HRIS.Controllers
{
    /// <summary>
    /// Base controller providing shared helper methods for all PECCI HRIS controllers.
    /// All controllers inherit from this instead of Controller directly.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>Returns the UserID of the currently authenticated user (0 if anonymous).</summary>
        protected int GetCurrentUserID()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        /// <summary>Returns the username of the currently authenticated user.</summary>
        protected string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        }

        /// <summary>Returns the role name of the currently authenticated user.</summary>
        protected string GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>Returns the remote IP address of the current HTTP request.</summary>
        protected string GetClientIP()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
