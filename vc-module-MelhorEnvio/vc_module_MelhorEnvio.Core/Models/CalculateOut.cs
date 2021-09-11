using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CalculateOut
    {

        public class DeliveryRange
        {
            [JsonProperty("min")]
            public int Min { get; set; }

            [JsonProperty("max")]
            public int Max { get; set; }
        }

        public class CustomDeliveryRange
        {
            [JsonProperty("min")]
            public int Min { get; set; }

            [JsonProperty("max")]
            public int Max { get; set; }
        }

        public class Dimensions
        {
            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }
        }

        public class Product
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }

        public class Package
        {
            [JsonProperty("price")]
            public decimal Price { get; set; }

            [JsonProperty("discount")]
            public decimal Discount { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }

            [JsonProperty("dimensions")]
            public Dimensions Dimensions { get; set; }

            [JsonProperty("weight")]
            public decimal Weight { get; set; }

            [JsonProperty("insurance_value")]
            public decimal InsuranceValue { get; set; }

            [JsonProperty("products")]
            public List<Product> Products { get; set; }
        }

        public class AdditionalServices
        {
            [JsonProperty("receipt")]
            public bool Receipt { get; set; }

            [JsonProperty("own_hand")]
            public bool OwnHand { get; set; }

            [JsonProperty("collect")]
            public bool Collect { get; set; }
        }

        public class Company
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("picture")]
            public string Picture { get; set; }
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("custom_price")]
        public decimal CustomPrice { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("delivery_time")]
        public int DeliveryTime { get; set; }

        [JsonProperty("delivery_range")]
        public DeliveryRange deliveryRange { get; set; }

        [JsonProperty("custom_delivery_time")]
        public int CustomDeliveryTime { get; set; }

        [JsonProperty("custom_delivery_range")]
        public CustomDeliveryRange customDeliveryRange { get; set; }

        [JsonProperty("packages")]
        public List<Package> Packages { get; set; }

        [JsonProperty("additional_services")]
        public AdditionalServices additionalServices { get; set; }

        [JsonProperty("company")]
        public Company company { get; set; }
    }

}
