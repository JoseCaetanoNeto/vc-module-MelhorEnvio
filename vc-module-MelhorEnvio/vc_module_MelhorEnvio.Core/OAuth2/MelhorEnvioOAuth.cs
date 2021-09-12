using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace vc_module_MelhorEnvio.Core
{

    public class MelhorEnvioOAuth : OAuth2
    {
        public MelhorEnvioOAuth(Dictionary<string, string> p, HttpContext pHttpContext, string pUserAgent) : base(p, pHttpContext, pUserAgent)
        {
        }

        /// <summary>
        /// Gets the login URL based off cconfiguration settings.
        /// </summary>
        /// <returns>The login/Auth URL</returns>
        public string getLoginURL()
        {
            var p = new Dictionary<string, string>() {
                { "response_type", "code" },
                { "client_id", _variables["client_id"] },
                { "redirect_uri", _variables["redirect_uri"] },
                {"scope", _variables["scope"] },
                {"state", _variables["state"] }
            };

            return getURI(_variables["authorize_uri"], p);
        }
    }
}