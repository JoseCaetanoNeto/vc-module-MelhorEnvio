using VirtoCommerce.OrdersModule.Core.Model;

namespace vc_module_MelhorEnvio.Core
{
    public interface IMelhorEnvioService
    {
        bool SendMECart(CustomerOrder pCustomerOrder);
    }
}