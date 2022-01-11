using System;
using System.Collections.Generic;
using System.Text;
using VirtoCommerce.Platform.Core.Settings;

namespace vc_module_MelhorEnvio.Core
{
    public static class HelperSettings
    {
        public static void SeveObjectSettings(this ISettingsManager pSettingsManager, string pName, string pObjectType, string pObjectId, string pValue)
        {
            var objectSetting = pSettingsManager.GetObjectSettingAsync(pName, pObjectType, pObjectId).GetAwaiter().GetResult();
            objectSetting.Value = pValue;
            pSettingsManager.SaveObjectSettingsAsync(new[] { objectSetting }).GetAwaiter().GetResult();
        }

        public static object GetObjectSettings(this ISettingsManager pSettingsManager, string pName, string pObjectType, string pObjectId)
        {
            var objectSetting = pSettingsManager.GetObjectSettingAsync(pName, pObjectType, pObjectId).GetAwaiter().GetResult();
            return objectSetting.Value;
        }
    }
}
