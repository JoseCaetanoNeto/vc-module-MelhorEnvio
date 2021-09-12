using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class PrintIn
    {
        [JsonProperty("mode")]
        public string Modes { get; set; }

        [JsonProperty("orders")]
        public List<string> Orders { get; set; }
    }
}
