using System;
using Serilog;
using Serilog.Events;

namespace MSAVA_INF.Models
{
    public class LocalEnvironmentValues
    {
        public required string JwtIssuerSigningKey { get; init; }
        public required string JwtIssuerName { get; init; }
        public required string JwtIssuerAudience { get; init; }
        public required string AdminUsername { get; init; }
        public required string AdminPassword { get; init; }
        public required string PostgresBaseDbUser { get; init; }
        public required string PostgresBaseDbPassword { get; init; }
        public required string PostgresBaseDbHost { get; init; }
        public required int PostgresBaseDbPort { get; init; }
        public required string PostgresBaseDbDbName { get; init; }
        public required string PostgresBaseDbSslMode { get; init; }
        public required LogEventLevel SerilogInformationLevel { get; init; }
        public required RollingInterval SerilogRollingInterval { get; init; }
        public required int? SerilogRetainedFileCountLimit { get; init; }
        public required long SerilogFileSizeLimitBytes { get; init; }
        public required bool SerilogRollOnFileSizeLimit { get; init; }
    }
}
