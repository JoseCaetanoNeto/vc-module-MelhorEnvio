using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CalculateIn
    {
        public class From
        {
            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }
        }

        public class To
        {
            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }
        }

        public class Product
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("weight")]
            public double Weight { get; set; }

            [JsonProperty("insurance_value")]
            public double InsuranceValue { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }

        public class Options
        {
            [JsonProperty("receipt")]
            public bool Receipt { get; set; }

            [JsonProperty("own_hand")]
            public bool OwnHand { get; set; }
        }

        [JsonProperty("from")]
        public From from { get; set; }

        [JsonProperty("to")]
        public To to { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("options")]
        public Options options { get; set; }

        [JsonProperty("services")]
        public string Services { get; set; }
    }


}
