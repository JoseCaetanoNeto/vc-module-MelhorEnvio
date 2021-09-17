using Newtonsoft.Json;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class PrintOut: IErrorOut
    {
        [JsonProperty("url")]
        public bool Url { get; set; }
        public ErrorOut errorOut { get; set; }
    }
}
