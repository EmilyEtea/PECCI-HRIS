using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PECCI_HRIS.Configuration;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Sends transactional emails via SMTP using MailKit.
    ///
    /// When EmailSettings.Enabled = false (default for local dev), the email
    /// body is written to the application log instead of being sent, so you
    /// can verify the content without needing a real mail server.
    /// </summary>
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger   = logger;
        }

        // ── Public send methods ───────────────────────────────────────────────

        /// <summary>Sends a plain-text + HTML email to a single recipient.</summary>
        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation(
                    "[EmailService] Email disabled. Would have sent to {To} — Subject: {Subject}\n{Body}",
                    toEmail, subject, htmlBody);
                return;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("[EmailService] Skipped send — recipient email is empty. Subject: {Subject}", subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var secureOption = _settings.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable;

                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureOption);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("[EmailService] Sent '{Subject}' to {To}", subject, toEmail);
            }
            catch (Exception ex)
            {
                // Log but don't throw — a failed email must never break the main workflow
                _logger.LogError(ex, "[EmailService] Failed to send '{Subject}' to {To}", subject, toEmail);
            }
        }

        // ── Leave notification helpers ────────────────────────────────────────

        /// <summary>
        /// Notifies the employee that their leave application was submitted
        /// and is awaiting approval.
        /// </summary>
        public Task SendLeaveSubmittedAsync(
            string employeeEmail, string employeeName,
            string leaveType, DateTime startDate, DateTime endDate, decimal days)
        {
            string subject = $"[PECCI HRIS] Leave Application Submitted — {leaveType}";
            string body = LeaveSubmittedTemplate(employeeName, leaveType, startDate, endDate, days);
            return SendAsync(employeeEmail, employeeName, subject, body);
        }

        /// <summary>
        /// Notifies the employee that their leave was approved or rejected.
        /// </summary>
        public Task SendLeaveDecisionAsync(
            string employeeEmail, string employeeName,
            string leaveType, DateTime startDate, DateTime endDate, decimal days,
            string status, string approverName, string? remarks)
        {
            bool approved = status == "Approved";
            string subject = approved
                ? $"[PECCI HRIS] Leave Approved — {leaveType}"
                : $"[PECCI HRIS] Leave Rejected — {leaveType}";
            string body = LeaveDecisionTemplate(
                employeeName, leaveType, startDate, endDate, days,
                approved, approverName, remarks);
            return SendAsync(employeeEmail, employeeName, subject, body);
        }

        /// <summary>
        /// Notifies HR/Manager that a new leave application is pending their review.
        /// </summary>
        public Task SendLeavePendingReviewAsync(
            string reviewerEmail, string reviewerName,
            string employeeName, string leaveType,
            DateTime startDate, DateTime endDate, decimal days,
            int applicationId)
        {
            string subject = $"[PECCI HRIS] Leave Pending Review — {employeeName}";
            string body = LeavePendingReviewTemplate(
                reviewerName, employeeName, leaveType, startDate, endDate, days, applicationId);
            return SendAsync(reviewerEmail, reviewerName, subject, body);
        }

        // ── Email templates ───────────────────────────────────────────────────

        private static string LeaveSubmittedTemplate(
            string name, string leaveType,
            DateTime start, DateTime end, decimal days)
        {
            return $@"
{Header()}
<p>Hi <strong>{Encode(name)}</strong>,</p>
<p>Your leave application has been submitted and is now awaiting approval.</p>
{LeaveDetailsTable(leaveType, start, end, days, "Pending")}
<p>You will receive another email once your application has been reviewed.</p>
{Footer()}";
        }

        private static string LeaveDecisionTemplate(
            string name, string leaveType,
            DateTime start, DateTime end, decimal days,
            bool approved, string approverName, string? remarks)
        {
            string statusColor  = approved ? "#2d6a4f" : "#dc3545";
            string statusLabel  = approved ? "✅ Approved" : "❌ Rejected";
            string statusText   = approved
                ? "Your leave application has been <strong>approved</strong>."
                : "Your leave application has been <strong>rejected</strong>.";

            string remarksHtml = string.IsNullOrWhiteSpace(remarks)
                ? ""
                : $@"<p><strong>Remarks:</strong> {Encode(remarks)}</p>";

            return $@"
{Header()}
<p>Hi <strong>{Encode(name)}</strong>,</p>
<p>{statusText}</p>
{LeaveDetailsTable(leaveType, start, end, days, statusLabel, statusColor)}
<p><strong>Reviewed by:</strong> {Encode(approverName)}</p>
{remarksHtml}
{(approved
    ? "<p>Please ensure your work is properly handed over before your leave starts.</p>"
    : "<p>If you have questions about this decision, please contact HR.</p>")}
{Footer()}";
        }

        private static string LeavePendingReviewTemplate(
            string reviewerName, string employeeName,
            string leaveType, DateTime start, DateTime end, decimal days,
            int applicationId)
        {
            return $@"
{Header()}
<p>Hi <strong>{Encode(reviewerName)}</strong>,</p>
<p>A leave application is pending your review.</p>
{LeaveDetailsTable(leaveType, start, end, days, "Pending Review", "#fd7e14", employeeName)}
<p>
  <a href=""/Leave/Review/{applicationId}""
     style=""display:inline-block;padding:10px 20px;background:#2d6a4f;color:#fff;
             text-decoration:none;border-radius:4px;font-weight:600;"">
    Review Application
  </a>
</p>
<p style=""color:#6c757d;font-size:0.85em;"">
  If the button above doesn't work, log in to PECCI HRIS and navigate to
  Leave Applications → Review #{ applicationId}.
</p>
{Footer()}";
        }

        // ── Shared template parts ─────────────────────────────────────────────

        private static string Header() => @"
<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;color:#212529;max-width:600px;margin:0 auto;padding:20px;"">
<div style=""background:#2d6a4f;padding:16px 24px;border-radius:6px 6px 0 0;"">
  <h2 style=""color:#fff;margin:0;"">PECCI HRIS</h2>
  <p style=""color:#b7e4c7;margin:4px 0 0;"">Human Resource Information System</p>
</div>
<div style=""border:1px solid #dee2e6;border-top:none;padding:24px;border-radius:0 0 6px 6px;"">";

        private static string Footer() => @"
<hr style=""border:none;border-top:1px solid #dee2e6;margin:24px 0;""/>
<p style=""color:#6c757d;font-size:0.8em;margin:0;"">
  This is an automated message from PECCI HRIS. Please do not reply to this email.
</p>
</div>
</body>
</html>";

        private static string LeaveDetailsTable(
            string leaveType, DateTime start, DateTime end, decimal days,
            string status, string statusColor = "#2d6a4f", string? employeeName = null)
        {
            string employeeRow = employeeName != null
                ? $@"<tr><td style=""padding:6px 12px;color:#6c757d;"">Employee</td>
                         <td style=""padding:6px 12px;""><strong>{Encode(employeeName)}</strong></td></tr>"
                : "";

            return $@"
<table style=""width:100%;border-collapse:collapse;margin:16px 0;
               border:1px solid #dee2e6;border-radius:4px;"">
  <tbody>
    {employeeRow}
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">Leave Type</td>
      <td style=""padding:6px 12px;""><strong>{Encode(leaveType)}</strong></td>
    </tr>
    <tr>
      <td style=""padding:6px 12px;color:#6c757d;"">Start Date</td>
      <td style=""padding:6px 12px;"">{start:MMMM d, yyyy} ({start:dddd})</td>
    </tr>
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">End Date</td>
      <td style=""padding:6px 12px;"">{end:MMMM d, yyyy} ({end:dddd})</td>
    </tr>
    <tr>
      <td style=""padding:6px 12px;color:#6c757d;"">Duration</td>
      <td style=""padding:6px 12px;"">{days} day(s)</td>
    </tr>
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">Status</td>
      <td style=""padding:6px 12px;"">
        <span style=""background:{statusColor};color:#fff;padding:2px 10px;
                      border-radius:12px;font-size:0.85em;font-weight:600;"">
          {Encode(status)}
        </span>
      </td>
    </tr>
  </tbody>
</table>";
        }

        private static string Encode(string? s)
            => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}
