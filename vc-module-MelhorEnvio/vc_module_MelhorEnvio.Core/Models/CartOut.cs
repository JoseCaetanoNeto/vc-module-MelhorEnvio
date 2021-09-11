using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CartOut
    {
        public class Product
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("unitary_value")]
            public int UnitaryValue { get; set; }

            [JsonProperty("weight")]
            public object Weight { get; set; }
        }

        public class Volume
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("height")]
            public string Height { get; set; }

            [JsonProperty("width")]
            public string Width { get; set; }

            [JsonProperty("length")]
            public string Length { get; set; }

            [JsonProperty("diameter")]
            public string Diameter { get; set; }

            [JsonProperty("weight")]
            public string Weight { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }
        }

        public class Tag
        {
            [JsonProperty("tag")]
            public string tag { get; set; }

            [JsonProperty("url")]
            public object Url { get; set; }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("service_id")]
        public int ServiceId { get; set; }

        [JsonProperty("agency_id")]
        public object AgencyId { get; set; }

        [JsonProperty("contract")]
        public string Contract { get; set; }

        [JsonProperty("service_code")]
        public object ServiceCode { get; set; }

        [JsonProperty("quote")]
        public double Quote { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("coupon")]
        public object Coupon { get; set; }

        [JsonProperty("discount")]
        public double Discount { get; set; }

        [JsonProperty("delivery_min")]
        public int DeliveryMin { get; set; }

        [JsonProperty("delivery_max")]
        public int DeliveryMax { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reminder")]
        public object Reminder { get; set; }

        [JsonProperty("insurance_value")]
        public int InsuranceValue { get; set; }

        [JsonProperty("weight")]
        public object Weight { get; set; }

        [JsonProperty("width")]
        public object Width { get; set; }

        [JsonProperty("height")]
        public object Height { get; set; }

        [JsonProperty("length")]
        public object Length { get; set; }

        [JsonProperty("diameter")]
        public object Diameter { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("billed_weight")]
        public int BilledWeight { get; set; }

        [JsonProperty("receipt")]
        public bool Receipt { get; set; }

        [JsonProperty("own_hand")]
        public bool OwnHand { get; set; }

        [JsonProperty("collect")]
        public bool Collect { get; set; }

        [JsonProperty("collect_scheduled_at")]
        public object CollectScheduledAt { get; set; }

        [JsonProperty("reverse")]
        public bool Reverse { get; set; }

        [JsonProperty("non_commercial")]
        public bool NonCommercial { get; set; }

        [JsonProperty("authorization_code")]
        public object AuthorizationCode { get; set; }

        [JsonProperty("tracking")]
        public object Tracking { get; set; }

        [JsonProperty("self_tracking")]
        public object SelfTracking { get; set; }

        [JsonProperty("delivery_receipt")]
        public object DeliveryReceipt { get; set; }

        [JsonProperty("additional_info")]
        public object AdditionalInfo { get; set; }

        [JsonProperty("cte_key")]
        public object CteKey { get; set; }

        [JsonProperty("paid_at")]
        public object PaidAt { get; set; }

        [JsonProperty("generated_at")]
        public object GeneratedAt { get; set; }

        [JsonProperty("posted_at")]
        public object PostedAt { get; set; }

        [JsonProperty("delivered_at")]
        public object DeliveredAt { get; set; }

        [JsonProperty("canceled_at")]
        public object CanceledAt { get; set; }

        [JsonProperty("suspended_at")]
        public object SuspendedAt { get; set; }

        [JsonProperty("expired_at")]
        public object ExpiredAt { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("parse_pi_at")]
        public object ParsePiAt { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("volumes")]
        public List<Volume> Volumes { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        public ErrorOut errorOut { get; set; }
    }
}
