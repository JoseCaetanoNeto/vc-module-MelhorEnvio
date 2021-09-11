using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class ErrorOut
    {
        public int status_code { get; set; }

        public string message { get; set; }

        public JObject error { get; set; }
    }

}
