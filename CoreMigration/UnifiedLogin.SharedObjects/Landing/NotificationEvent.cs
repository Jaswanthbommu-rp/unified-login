using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class NotificationEvent {
        public IList<string> Users { get; set; }
        public string Method { get; set; }
        public NotificationEventData Data { get; set; } 
        public string ProductCode { get; set; }

    }

    public class NotificationEventData {
        public long PersonaId { get; set; }
    }
}
