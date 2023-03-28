using System.Threading.Tasks;
using System.Linq;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.CoreModule.Core.Common;
using vc_module_zipcode_position.Core.Services;
using Geo.Here.Abstractions;

namespace vc_module_MelhorEnvio.Core.Services
{
    public class BuscaCEPStandardAddress : IConversorStandardAddress
    {
        ISearchZipCode _searchZipCode;
        IHereGeocoding _hereGeocoding;

        public BuscaCEPStandardAddress(ISearchZipCode searchZipCode, IHereGeocoding hereGeocoding)
        {
            _searchZipCode = searchZipCode;
            _hereGeocoding = hereGeocoding;
        }
        
        public Task<AddressStandardModel> GetStandardAsync(Address pAddress)
        {
            var result = _searchZipCode.GetGeoDataAsync(pAddress.PostalCode, string.Empty).GetAwaiter().GetResult();
            var query = string.Join(',', pAddress.Line1, pAddress.Line2, pAddress.City, pAddress.RegionId, pAddress.PostalCode, pAddress.CountryName);
            var resultHere = _hereGeocoding.GeocodingAsync(new Geo.Here.Models.Parameters.GeocodeParameters() { Query = query }).GetAwaiter().GetResult()?.Items.FirstOrDefault();

            return Task.FromResult(new AddressStandardModel()
            {
                Street = result?.Street ?? resultHere?.Address.Street,
                Number = resultHere?.Address.HouseNumber,
                Complement = pAddress.Line2,
                Neighborhood = result?.District ?? resultHere?.Address.County ?? resultHere?.Address.District,
                City = result?.City ?? resultHere?.Address.City,
                State = result?.State ?? resultHere?.Address.State,
                Country = result?.CountryCode ?? resultHere?.Address.CountryCode,
                ZipCode = pAddress.PostalCode,
                HouseNumberFallback = false,
            });
        }
    }
}
