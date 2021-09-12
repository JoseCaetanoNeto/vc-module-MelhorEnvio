using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class TrackingOut: JObject
    {
        
    }

    public class TrackingItemOut
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("tracking")]
        public object Tracking { get; set; }

        [JsonProperty("melhorenvio_tracking")]
        public object MelhorenvioTracking { get; set; }

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
