﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class GenerateIn
    {
        [JsonProperty("orders")]
        public List<string> Orders { get; set; }
    }


}
