using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class PrintOut
    {
        [JsonProperty("url")]
        public bool Url { get; set; }
        public ErrorOut errorOut { get; set; }
    }
}
