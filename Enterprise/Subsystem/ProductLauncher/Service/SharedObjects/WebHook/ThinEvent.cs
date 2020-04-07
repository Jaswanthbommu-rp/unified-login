using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook
{
    public class ThinEvent<T>
    {
        /// <summary>
        /// The id of the event
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// the topic of the event
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// The date/time the event was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public T Payload { get; set; }
    }
}
