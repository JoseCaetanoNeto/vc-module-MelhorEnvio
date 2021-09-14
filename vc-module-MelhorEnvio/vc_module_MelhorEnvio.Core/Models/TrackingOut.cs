using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class TrackingOut : Dictionary<string, TrackingOut.TrackingItemOut>
    {
        public class TrackingItemOut
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("tracking")]
            public string Tracking { get; set; }

            [JsonProperty("melhorenvio_tracking")]
            public string MelhorenvioTracking { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("paid_at")]
            public DateTime? PaidAt { get; set; }

            [JsonProperty("generated_at")]
            public DateTime? GeneratedAt { get; set; }

            [JsonProperty("posted_at")]
            public DateTime? PostedAt { get; set; }

            [JsonProperty("delivered_at")]
            public DateTime? DeliveredAt { get; set; }

            [JsonProperty("canceled_at")]
            public DateTime? CanceledAt { get; set; }

            [JsonProperty("expired_at")]
            public DateTime? ExpiredAt { get; set; }

            public ErrorOut errorOut { get; set; }
        }
    }

}
