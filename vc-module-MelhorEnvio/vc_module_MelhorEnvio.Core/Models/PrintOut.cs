using Newtonsoft.Json;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class PrintOut: IErrorOut
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        public ErrorOut errorOut { get; set; }
    }
}
