namespace UnifiedLogin.BatchProcessor.Models;

public enum BatchStatusType
{
    Waiting = 5,
    Running = 6,
    Error = 7,
    Success = 8,
    TimeoutError = 901,
    Paused = 101
}
