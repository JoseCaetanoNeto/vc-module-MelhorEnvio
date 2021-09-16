using Newtonsoft.Json;
using System.Collections.Generic;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CancelOut:Dictionary<string, CancelOut.Order>
    {
        public class Order
        {
            [JsonProperty("canceled")]
            public bool Canceled { get; set; }

            [JsonProperty("value")]
            public decimal? Value { get; set; }

            [JsonProperty("time")]
            public long? Time { get; set; }
        }
    }




}
