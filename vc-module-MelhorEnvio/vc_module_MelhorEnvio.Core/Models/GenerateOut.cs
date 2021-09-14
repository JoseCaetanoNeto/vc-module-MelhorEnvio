using Newtonsoft.Json;
using System.Collections.Generic;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class GenerateOut : Dictionary<string, GenerateOut.ItemGenerate>
    {
        public class ItemGenerate
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
