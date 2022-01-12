using Geo.Here.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.CoreModule.Core.Common;

namespace vc_module_MelhorEnvio.Core.Services
{
    public class HereConversorStandardAddress : IConversorStandardAddress
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IHereGeocoding _hereGeocoding;
        public HereConversorStandardAddress(IHereGeocoding pHereGeocoding, IPlatformMemoryCache pPlatformMemoryCache)
        {
            _hereGeocoding = pHereGeocoding;
            _platformMemoryCache = pPlatformMemoryCache;
        }

        public async Task<AddressStandardModel> GetStandardAsync(Address pAddress)
        {
            var query = string.Join(',', pAddress.Line1, pAddress.Line2, pAddress.City, pAddress.RegionId, pAddress.PostalCode, pAddress.CountryName);
            string key = CacheKey.With(GetType(), nameof(GetStandardAsync), query);
            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(key, async (cacheEntry) =>
            {
                var resultHere = (await _hereGeocoding.GeocodingAsync(new Geo.Here.Models.Parameters.GeocodeParameters() { Query = query }))?.Items.FirstOrDefault();
                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(15));

                var addressStd = new AddressStandardModel()
                {
                    Street = resultHere?.Address.Street,
                    Number = resultHere?.Address.HouseNumber,
                    Complement = pAddress.Line2,
                    Neighborhood = resultHere?.Address.County ?? resultHere?.Address.District,
                    City = resultHere?.Address.City,
                    State = resultHere?.Address.State,
                    Country = pAddress.CountryName,
                    ZipCode = resultHere?.Address.PostalCode,
                    HouseNumberFallback = resultHere?.HouseNumberFallback ?? false,
                };

                return addressStd;
            });
            return result;
        }
    }
}
