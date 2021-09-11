using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class CartIn
    {
        public class From
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("document")]
            public string Document { get; set; }

            [JsonProperty("company_document")]
            public string CompanyDocument { get; set; }

            [JsonProperty("state_register")]
            public string StateRegister { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("complement")]
            public string Complement { get; set; }

            [JsonProperty("number")]
            public string Number { get; set; }

            [JsonProperty("district")]
            public string District { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("country_id")]
            public string CountryId { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("note")]
            public string Note { get; set; }

            
            [JsonProperty("economic_activity_code")]
            public string EconomicActivityCode { get; set; }
        }

        public class To
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("document")]
            public string Document { get; set; }

            [JsonProperty("company_document")]
            public string CompanyDocument { get; set; }

            [JsonProperty("state_register")]
            public string StateRegister { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("complement")]
            public string Complement { get; set; }

            [JsonProperty("number")]
            public string Number { get; set; }

            [JsonProperty("district")]
            public string District { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state_abbr")]
            public string StateAbbr { get; set; }

            [JsonProperty("country_id")]
            public string CountryId { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("note")]
            public string Note { get; set; }

            [JsonProperty("economic_activity_code")]
            public string EconomicActivityCode { get; set; }
        }

        public class Product
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("unitary_value")]
            public decimal UnitaryValue { get; set; }
        }

        public class Volume
        {
            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("weight")]
            public double Weight { get; set; }
        }

        public class Invoice
        {
            [JsonProperty("key")]
            public string Key { get; set; }
        }

        public class Tag
        {
            [JsonProperty("tag")]
            public string tag { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class Options
        {
            [JsonProperty("insurance_value")]
            public decimal InsuranceValue { get; set; }

            [JsonProperty("receipt")]
            public bool Receipt { get; set; }

            [JsonProperty("own_hand")]
            public bool OwnHand { get; set; }

            [JsonProperty("reverse")]
            public bool Reverse { get; set; }

            [JsonProperty("non_commercial")]
            public bool NonCommercial { get; set; }

            [JsonProperty("invoice")]
            public Invoice Invoice { get; set; }

            [JsonProperty("platform")]
            public string Platform { get; set; }

            [JsonProperty("tags")]
            public List<Tag> Tags { get; set; }
        }


        [JsonProperty("service")]
        public int Service { get; set; }

        [JsonProperty("agency")]
        public int? Agency { get; set; }

        [JsonProperty("from")]
        public From from { get; set; }

        [JsonProperty("to")]
        public To to { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("volumes")]
        public List<Volume> Volumes { get; set; }

        [JsonProperty("options")]
        public Options options { get; set; }
    }
}
