using System;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.CoreModule.Core.Common;

namespace vc_module_MelhorEnvio.Core.Services
{
    public class DummyConversorStandardAddress : IConversorStandardAddress
    {
        public Task<AddressStandardModel> GetStandardAsync(Address address)
        {
            return Task.FromResult(new AddressStandardModel()
            {
                Street = address.Street,
                Number = address.Number,
                Complement = address.Line2,
                Neighborhood = address.District,
                City = address.City,
                State = address.RegionId,
                Country = address.CountryCode,
                ZipCode = address.PostalCode,
                HouseNumberFallback = false,
            });
        }
    }
}
