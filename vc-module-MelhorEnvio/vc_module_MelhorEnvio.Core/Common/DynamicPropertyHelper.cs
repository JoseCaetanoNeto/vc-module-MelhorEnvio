using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace vc_module_MelhorEnvio.Core
{
    public static class DynamicPropertyHelper
    {
        public static async Task SetDynamicProp(this IList<DynamicProperty> resultSearch, IDynamicPropertySearchService _dynamicPropertySearchService, IHasDynamicProperties pProp, string pName, object pValue)
        {
            if (resultSearch.Count == 0)
            {
                resultSearch = (await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria() { ObjectType = pProp.ObjectType })).Results;
            }

            var property = pProp.DynamicProperties.FirstOrDefault(o => o.Name == pName);
            if (property == null)
            {
                if (pProp.DynamicProperties.IsReadOnly)
                    pProp.DynamicProperties = new List<DynamicObjectProperty>();

                property = new DynamicObjectProperty { Name = pName };
                pProp.DynamicProperties.Add(property);
            }
            var prop = resultSearch.FirstOrDefault(o => o.Name == pName);
            property.ValueType = GetValueType(pValue);
            property.Values = new List<DynamicPropertyObjectValue>(new[] { new DynamicPropertyObjectValue { Value = pValue, PropertyId = prop.Id } });
        }


        private static DynamicPropertyValueType GetValueType(object pValue)
        {
            return (pValue is int ? DynamicPropertyValueType.Integer : pValue is decimal ? DynamicPropertyValueType.Decimal : pValue is DateTime ? DynamicPropertyValueType.DateTime : pValue is bool ? DynamicPropertyValueType.Boolean : DynamicPropertyValueType.ShortText);
        }
    }
}
