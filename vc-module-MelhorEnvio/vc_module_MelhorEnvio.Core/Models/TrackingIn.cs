using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class TrackingIn
    {
        [JsonProperty("orders")]
        public List<string> Orders { get; set; }
    }


}
