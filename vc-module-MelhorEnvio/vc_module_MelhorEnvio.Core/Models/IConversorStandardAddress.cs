using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Common;

namespace vc_module_MelhorEnvio.Core.Models
{
    public interface IConversorStandardAddress
    {
        Task<AddressStandardModel> GetStandardAsync(Address address);

    }

    public class AddressStandardModel
    {
        // numero da rua não achou, é aproximado
        public bool HouseNumberFallback { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string ZipCode { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Complement { get; set; }
    }
}
