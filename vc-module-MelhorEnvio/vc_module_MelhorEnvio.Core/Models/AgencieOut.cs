using Newtonsoft.Json;
using System;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class AgencieOut: IErrorOut
    {
        public class Country
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("country")]
            public string country { get; set; }
        }

        public class State
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("state")]
            public string state { get; set; }

            [JsonProperty("state_abbr")]
            public string StateAbbr { get; set; }

            [JsonProperty("country")]
            public Country Country { get; set; }
        }

        public class City
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("city")]
            public string city { get; set; }

            [JsonProperty("state")]
            public State State { get; set; }
        }

        public class Address
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("label")]
            public object Label { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("address")]
            public string address { get; set; }

            [JsonProperty("number")]
            public string Number { get; set; }

            [JsonProperty("complement")]
            public string Complement { get; set; }

            [JsonProperty("district")]
            public string District { get; set; }

            [JsonProperty("latitude")]
            public double Latitude { get; set; }

            [JsonProperty("longitude")]
            public double Longitude { get; set; }

            [JsonProperty("confirmed_at")]
            public DateTime? ConfirmedAt { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime? UpdatedAt { get; set; }

            [JsonProperty("city")]
            public City City { get; set; }
        }

        public class Phone
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("label")]
            public object Label { get; set; }

            [JsonProperty("phone")]
            public string phone { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("country_id")]
            public string CountryId { get; set; }

            [JsonProperty("confirmed_at")]
            public object ConfirmedAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("initials")]
        public string Initials { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("note")]
        public object Note { get; set; }

        [JsonProperty("company_id")]
        public int CompanyId { get; set; }

        [JsonProperty("address")]
        public Address address { get; set; }

        [JsonProperty("phone")]
        public Phone phone { get; set; }

        public ErrorOut errorOut { get; set; }
    }
}
