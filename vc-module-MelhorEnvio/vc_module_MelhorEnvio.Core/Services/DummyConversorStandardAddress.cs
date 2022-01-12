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
            return null;
        }
    }
}
