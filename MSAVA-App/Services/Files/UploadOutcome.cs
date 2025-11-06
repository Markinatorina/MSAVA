namespace MSAVA_App.Services.Files;

public sealed partial record UploadOutcome(bool Success, int StatusCode, string? Id, string? Error);
