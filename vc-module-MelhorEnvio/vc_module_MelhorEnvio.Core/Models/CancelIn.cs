using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CancelIn
    {
        public class Order
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("reason_id")]
            public string ReasonId { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }


        [JsonProperty("order")]
        public Order order { get; set; }
    }




}
