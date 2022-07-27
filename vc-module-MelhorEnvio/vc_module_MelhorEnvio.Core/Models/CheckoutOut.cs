using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace vc_module_MelhorEnvio.Core.Models
{
    public class CheckoutOut: IErrorOut
    {
        public class Reason
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        public class Transaction
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("authorized_at")]
            public string AuthorizedAt { get; set; }

            [JsonProperty("unauthorized_at")]
            public object UnauthorizedAt { get; set; }

            [JsonProperty("reserved_at")]
            public object ReservedAt { get; set; }

            [JsonProperty("canceled_at")]
            public object CanceledAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("description_internal")]
            public object DescriptionInternal { get; set; }

            [JsonProperty("reason")]
            public Reason Reason { get; set; }
        }

        public class From
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("document")]
            public object Document { get; set; }

            [JsonProperty("company_document")]
            public string CompanyDocument { get; set; }

            [JsonProperty("state_register")]
            public object StateRegister { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("location_number")]
            public object LocationNumber { get; set; }

            [JsonProperty("complement")]
            public object Complement { get; set; }

            [JsonProperty("district")]
            public object District { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state_abbr")]
            public string StateAbbr { get; set; }

            [JsonProperty("country_id")]
            public string CountryId { get; set; }

            [JsonProperty("latitude")]
            public object Latitude { get; set; }

            [JsonProperty("longitude")]
            public object Longitude { get; set; }

            [JsonProperty("note")]
            public object Note { get; set; }
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
            public object CompanyDocument { get; set; }

            [JsonProperty("state_register")]
            public object StateRegister { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("location_number")]
            public object LocationNumber { get; set; }

            [JsonProperty("complement")]
            public string Complement { get; set; }

            [JsonProperty("district")]
            public object District { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state_abbr")]
            public string StateAbbr { get; set; }

            [JsonProperty("country_id")]
            public string CountryId { get; set; }

            [JsonProperty("latitude")]
            public object Latitude { get; set; }

            [JsonProperty("longitude")]
            public object Longitude { get; set; }

            [JsonProperty("note")]
            public object Note { get; set; }
        }

        public class Company
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("picture")]
            public string Picture { get; set; }

            [JsonProperty("use_own_contract")]
            public bool UseOwnContract { get; set; }
        }

        public class Service
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("range")]
            public string Range { get; set; }

            [JsonProperty("restrictions")]
            public string Restrictions { get; set; }

            [JsonProperty("requirements")]
            public string Requirements { get; set; }

            [JsonProperty("optionals")]
            public string Optionals { get; set; }

            [JsonProperty("company")]
            public Company Company { get; set; }
        }

        public class Tag
        {
            [JsonProperty("tag")]
            public string tag { get; set; }

            [JsonProperty("url")]
            public object Url { get; set; }
        }

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

        public class Order
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("service_id")]
            public int ServiceId { get; set; }

            [JsonProperty("agency_id")]
            public int? AgencyId { get; set; }

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
            public double InsuranceValue { get; set; }

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
            public decimal BilledWeight { get; set; }

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
            public string PaidAt { get; set; }

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

            [JsonProperty("from")]
            public From From { get; set; }

            [JsonProperty("to")]
            public To To { get; set; }

            [JsonProperty("service")]
            public Service Service { get; set; }

            [JsonProperty("agency")]
            public object Agency { get; set; }

            [JsonProperty("invoice")]
            public object Invoice { get; set; }

            [JsonProperty("tags")]
            public List<Tag> Tags { get; set; }

            [JsonProperty("products")]
            public List<Product> Products { get; set; }

            [JsonProperty("generated_key")]
            public object GeneratedKey { get; set; }
        }

        public class Purchase
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("total")]
            public double Total { get; set; }

            [JsonProperty("discount")]
            public double Discount { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("paid_at")]
            public string PaidAt { get; set; }

            [JsonProperty("canceled_at")]
            public object CanceledAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }

            [JsonProperty("payment")]
            public object Payment { get; set; }

            [JsonProperty("transactions")]
            public List<Transaction> Transactions { get; set; }

            [JsonProperty("orders")]
            public List<Order> Orders { get; set; }

            [JsonProperty("paypal_discounts")]
            public List<object> PaypalDiscounts { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public int Protocol { get; set; }

            [JsonProperty("firstname")]
            public string Firstname { get; set; }

            [JsonProperty("lastname")]
            public string Lastname { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("picture")]
            public object Picture { get; set; }

            [JsonProperty("thumbnail")]
            public object Thumbnail { get; set; }

            [JsonProperty("document")]
            public string Document { get; set; }

            [JsonProperty("birthdate")]
            public string Birthdate { get; set; }

            [JsonProperty("email_confirmed_at")]
            public string EmailConfirmedAt { get; set; }

            [JsonProperty("email_alternative")]
            public object EmailAlternative { get; set; }

            [JsonProperty("access_at")]
            public string AccessAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }
        }

        public class Group
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("paid_at")]
            public string PaidAt { get; set; }

            [JsonProperty("canceled_at")]
            public object CanceledAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }
        }

        public class Conciliation
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("service_code")]
            public object ServiceCode { get; set; }

            [JsonProperty("from_postal_code")]
            public string FromPostalCode { get; set; }

            [JsonProperty("from_city")]
            public string FromCity { get; set; }

            [JsonProperty("from_state_abbr")]
            public string FromStateAbbr { get; set; }

            [JsonProperty("to_postal_code")]
            public string ToPostalCode { get; set; }

            [JsonProperty("to_city")]
            public string ToCity { get; set; }

            [JsonProperty("to_state_abbr")]
            public string ToStateAbbr { get; set; }

            [JsonProperty("authorization_code")]
            public string AuthorizationCode { get; set; }

            [JsonProperty("tracking")]
            public string Tracking { get; set; }

            [JsonProperty("quote")]
            public double Quote { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("discount")]
            public double Discount { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

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
            public decimal BilledWeight { get; set; }

            [JsonProperty("receipt")]
            public bool Receipt { get; set; }

            [JsonProperty("own_hand")]
            public bool OwnHand { get; set; }

            [JsonProperty("collect")]
            public bool Collect { get; set; }

            [JsonProperty("distinct_metrics")]
            public bool DistinctMetrics { get; set; }

            [JsonProperty("paid_at")]
            public string PaidAt { get; set; }

            [JsonProperty("canceled_at")]
            public object CanceledAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }

            [JsonProperty("rate")]
            public object Rate { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("group")]
            public Group Group { get; set; }

            [JsonProperty("agency")]
            public object Agency { get; set; }
        }

        public class ConciliationGroup
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("protocol")]
            public string Protocol { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("paid_at")]
            public string PaidAt { get; set; }

            [JsonProperty("canceled_at")]
            public object CanceledAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }

            [JsonProperty("conciliations")]
            public List<Conciliation> Conciliations { get; set; }

            [JsonProperty("transactions")]
            public List<Transaction> Transactions { get; set; }

            [JsonProperty("payment")]
            public object Payment { get; set; }
        }

        [JsonProperty("purchase")]
        public Purchase purchase { get; set; }

        [JsonProperty("conciliation_group")]
        public ConciliationGroup conciliationGroup { get; set; }

        [JsonProperty("digitable")]
        public object Digitable { get; set; }

        [JsonProperty("redirect")]
        public object Redirect { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        [JsonProperty("token")]
        public object Token { get; set; }

        [JsonProperty("payment_id")]
        public object PaymentId { get; set; }

        public ErrorOut errorOut { get; set; }
    }




}
