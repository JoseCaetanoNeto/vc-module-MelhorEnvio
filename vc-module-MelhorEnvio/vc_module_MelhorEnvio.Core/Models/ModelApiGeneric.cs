using System;
using System.Collections.Generic;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class ErrorOut
    {
        public int status_code { get; set; }

        public string message { get; set; }

        public Dictionary<string,string[]> error { get; set; }
    }

}
