using Newtonsoft.Json;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class PrintOut
    {
        [JsonProperty("url")]
        public bool Url { get; set; }
        public ErrorOut errorOut { get; set; }
    }
}
