using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Loggers
{
    public class ServiceLogger
    {
        private readonly ILogger<ServiceLogger> _logger;
        private readonly IIdentifiableRepository<ErrorLogDB> _errorLogRepository;
        private readonly IIdentifiableRepository<AccessLogDB> _accessLogRepository;
        private readonly IIdentifiableRepository<UserLogDB> _userLogRepository;
        private readonly IIdentifiableRepository<GroupLogDB> _groupLogRepository;
        private readonly IIdentifiableRepository<InviteLogDB> _inviteLogRepository;
        public ServiceLogger(
            ILogger<ServiceLogger> logger,
            IIdentifiableRepository<ErrorLogDB> errorLogRepository,
            IIdentifiableRepository<AccessLogDB> accessLogRepository,
            IIdentifiableRepository<UserLogDB> userLogRepository,
            IIdentifiableRepository<GroupLogDB> groupLogRepository,
            IIdentifiableRepository<InviteLogDB> inviteLogRepository)
        {
            _logger = logger;
            _errorLogRepository = errorLogRepository;
            _accessLogRepository = accessLogRepository;
            _userLogRepository = userLogRepository;
            _groupLogRepository = groupLogRepository;
            _inviteLogRepository = inviteLogRepository;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation("{Message}", message);
        }

        public string SanitizeString(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            string sanitizedMessage = message.Replace(Environment.NewLine, "");
            sanitizedMessage = message
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .Replace("&", "&amp;")
                .Replace(";", "&#59;")
                .Replace(":", "&#58;")
                .Replace("(", "&#40;")
                .Replace(")", "&#41;")
                .Replace("{", "&#123;")
                .Replace("}", "&#125;")
                .Replace("[", "&#91;")
                .Replace("]", "&#93;")
                .Replace("`", "&#96;")
                .Replace("\\", "&#92;")
                .Replace("/", "&#47;")
                .Replace("=", "&#61;")
                .Replace("+", "&#43;")
                .Replace("$", "&#36;")
                .Replace("%", "&#37;")
                .Replace("!", "&#33;")
                .Replace("@", "&#64;")
                .Replace("#", "&#35;")
                .Replace("^", "&#94;")
                .Replace("*", "&#42;")
                .Replace("|", "&#124;")
                .Replace("~", "&#126;")
                .Replace(",", "&#44;")
                .Replace(".", "&#46;")
                .Replace("?", "&#63;")
                .Replace("\t", "&#9;");
            sanitizedMessage = sanitizedMessage.Length > 200 ? sanitizedMessage.Substring(0, 200) : sanitizedMessage;
            return sanitizedMessage;
        }

        public void WriteLog(int statusCode, string message, Guid? userId)
        {
            var errorLog = new ErrorLogDB
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
            string sanitizedMessage = SanitizeString(message);
            _logger.LogError("Status Code: {StatusCode}, Message: {Message}, UserId: {UserId}", statusCode, sanitizedMessage, userId);
            _errorLogRepository.Insert(errorLog);
            _errorLogRepository.SaveChanges();
        }

        public void WriteLog(InviteLogActions action, string message, Guid userId, Guid codeId)
        {
            var inviteLog = new InviteLogDB
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                InviteCodeId = codeId,
                Timestamp = DateTime.UtcNow
            };

            string sanitizedMessage = SanitizeString(message);
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, InviteCodeId: {CodeId}", actionString, sanitizedMessage, userId, codeId);
            _inviteLogRepository.Insert(inviteLog);
            _inviteLogRepository.SaveChanges();
        }

        public void WriteLog(GroupLogActions action, string message, Guid userId, Guid groupId)
        {
            var groupLog = new GroupLogDB
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                GroupId = groupId,
                Timestamp = DateTime.UtcNow
            };
            string sanitizedMessage = SanitizeString(message);
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, GroupId: {GroupId}", actionString, sanitizedMessage, userId, groupId);
            _groupLogRepository.Insert(groupLog);
            _groupLogRepository.SaveChanges();
        }

        public void WriteLog(AccessLogActions action, string message, Guid userId, string fileNameWithExtension, Guid refId)
        {
            var accessLog = new AccessLogDB
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                FileRefId = refId,
                Timestamp = DateTime.UtcNow
            };
            string sanitizedMessage = SanitizeString(message);
            string sanitizedFileName = SanitizeString(fileNameWithExtension);
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, File: {fileNameWithExtension}, FileRefId: {refId}", actionString, sanitizedMessage, userId, sanitizedFileName, refId);
            _accessLogRepository.Insert(accessLog);
            _accessLogRepository.SaveChanges();
        }
        public void WriteLog(AccessLogActions action, string message, Guid userId, Guid refId)
        {
            var accessLog = new AccessLogDB
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                FileRefId = refId,
                Timestamp = DateTime.UtcNow
            };
            string sanitizedMessage = SanitizeString(message);
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, FileRefId: {refId}", actionString, sanitizedMessage, userId, refId);
            _accessLogRepository.Insert(accessLog);
            _accessLogRepository.SaveChanges();
        }

        public void WriteLog(UserLogAction action, string message, Guid userId, Guid? adminId)
        {
            var userLog = new UserLogDB
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AdminId = adminId,
                Action = action,
            };
            string sanitizedMessage = SanitizeString(message);
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, AdminId: {AdminId}", actionString, sanitizedMessage, userId, adminId);

            _userLogRepository.Insert(userLog);
            _userLogRepository.SaveChanges();
        }
    }
}
