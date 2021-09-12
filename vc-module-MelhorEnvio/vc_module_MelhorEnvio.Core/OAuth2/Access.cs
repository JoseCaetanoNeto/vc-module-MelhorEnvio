//using System.Web.Script.Serialization;

namespace vc_module_MelhorEnvio.Core
{
    /// <summary>
    /// Model of what gets sent back from the OAuth2 servers
    /// </summary>
    public class Access
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
}