using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace vc_module_MelhorEnvio.Core
{
    public static class ModuleConstants
    {
        public const string urlbaseSandbox = "https://sandbox.melhorenvio.com.br";
        public const string urlbase = "https://www.melhorenvio.com.br";
        public static string objectTypeRestrict => nameof(MelhorEnvioMethod) + "_restrict";

        public const string K_DefaultCancelReason = "Cliente desistiu da compra";

        public const int K_Company_CORREIOS = 1;
        public const int K_Company_JADLOG = 2;
        public const int K_Company_AZULEXPRES = 9;

        public const string K_InvoiceKey = "InvoiceKey";
        public const string K_linkEtiqueta = "linkEtiqueta";

        
        public static class Security
        {
            public static class Permissions
            {
                public static string[] AllPermissions { get; } = { };
            }
        }

        public static class Settings
        {
            public static class MelhorEnvio
            {

                public static readonly SettingDescriptor Sandbox = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.sandbox",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = "True"
                };

                public static readonly SettingDescriptor client_id = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.client_id",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = "client_id gerado no painel MelhorEnvio"
                };

                public static readonly SettingDescriptor client_secret = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.client_secret",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor SendDataOnShippingStatus = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.sendDataOnShippingStatus",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor Document = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.document",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor StateRegister = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.stateRegister",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor CompanyDocument = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.companyDocument",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor EconomicActivityCode = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.economicActivityCode",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor NonCommercial = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.nonCommercial",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static readonly SettingDescriptor Token = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.token",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.LongText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor EnableSyncJob = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.EnableSyncJob",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };

                public static readonly SettingDescriptor CronSyncJob = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.CronSyncJob",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "0/15 * * * *"
                };

                public static readonly SettingDescriptor AgencyAzul = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.AgencyAzul",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = null // "id" : 5627, "name" : "CPVF2", "address.label" : "Agncia Azul Cargo"
                    // curl -k --location -g --request GET 'https://www.melhorenvio.com.br/api/v2/me/shipment/agencies?company=9&country=BR&state=PB&city=Campina Grande' | json_pp | more
                };

                public static readonly SettingDescriptor AgencyJadLog = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.AgencyJadLog",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = null // "id" : 13313, "name" : "LJ CAMPINA GRANDE 01", "address.label" : "Unidade Jadlog",
                    // curl -k --location -g --request GET 'https://www.melhorenvio.com.br/api/v2/me/shipment/agencies?company=2&country=BR&state=PB&city=Campina Grande' | json_pp | more
                };

                public static readonly SettingDescriptor Checkout = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.Checkout",
                    GroupName = "vcmoduleMelhorEnvio|MelhorEnvio by store",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static IEnumerable<SettingDescriptor> Settings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            client_id,
                            client_secret,
                            CompanyDocument,
                            StateRegister,
                            EconomicActivityCode,
                            NonCommercial,
                            Document,
                            SendDataOnShippingStatus,
                            Sandbox,
                            AgencyJadLog,
                            AgencyAzul,
                            Checkout
                        };
                    }
                }

                public static IEnumerable<SettingDescriptor> GlobalSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            EnableSyncJob,
                            CronSyncJob
                        };
                    }
                }

                public static IEnumerable<SettingDescriptor> RestrictSettings
                {
                    get
                    {
                        yield return Token;
                    }
                }
            }


            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    var list = new List<SettingDescriptor>(MelhorEnvio.Settings);
                    list.AddRange(MelhorEnvio.GlobalSettings);
                    list.AddRange(MelhorEnvio.RestrictSettings);
                    return list;
                }
            }
        }
    }
}