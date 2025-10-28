namespace UnifiedLogin.SharedObjects.Landing
{
    public class ActivityAttemptDetails
    {
        public int AttemptCount { get; set; }
        public int MaxActivitycount { get; set; }
        public int ActivityTokenExpirationMinutes { get; set; }
    }
}
