using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace vc_module_MelhorEnvio.Core
{
    public static class ModuleConstants
    {
        public const string urlbaseSandbox = "https://sandbox.melhorenvio.com.br";
        public const string urlbase = "https://www.melhorenvio.com.br";
        public static string objectTypeRestrict => nameof(MelhorEnvioMethod) + "_restrict";

        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "vcmoduleMelhorEnvio:access";
                public const string Create = "vcmoduleMelhorEnvio:create";
                public const string Read = "vcmoduleMelhorEnvio:read";
                public const string Update = "vcmoduleMelhorEnvio:update";
                public const string Delete = "vcmoduleMelhorEnvio:delete";

                public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete };
            }
        }

        public static class Settings
        {
            public static class MelhorEnvio
            {

                public static readonly SettingDescriptor Sandbox = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.sandbox",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = "True"
                };

                public static readonly SettingDescriptor client_id = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.client_id",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = "client_id gerado no painel MelhorEnvio"
                };

                public static readonly SettingDescriptor client_secret = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.client_secret",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor SendDataOnShippingStatus = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.sendDataOnShippingStatus",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor SendDataOnOrderStatus = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.sendDataOnOrderStatus",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor Document = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.document",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor StateRegister = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.stateRegister",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor CompanyDocument = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.companyDocument",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor EconomicActivityCode = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.economicActivityCode",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };                

                public static readonly SettingDescriptor NonCommercial = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.nonCommercial",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };
                
                public static readonly SettingDescriptor Token = new SettingDescriptor
                {
                    Name = "vcmoduleMelhorEnvio.token",
                    GroupName = "vcmoduleMelhorEnvio|General",
                    ValueType = SettingValueType.LongText,
                    DefaultValue = ""
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
                            SendDataOnOrderStatus,
                            Sandbox,
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
                    list.Add(MelhorEnvio.Token);
                    return list;
                }
            }
        }
    }
}
