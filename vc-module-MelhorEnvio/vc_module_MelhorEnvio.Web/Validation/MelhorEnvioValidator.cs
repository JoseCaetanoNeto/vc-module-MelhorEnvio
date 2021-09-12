using FluentValidation;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;

namespace vc_module_MelhorEnvio.Web.Validation
{
    public class MelhorEnvioValidator : AbstractValidator<Shipment>
    {
        /*public MelhorEnvioValidator(ICustomerOrderService pOrderService)
        {
            RuleFor(shipment => shipment).Custom((newShipmentRequest, context) =>
            {
                var order = pOrderService.GetByIdAsync(newShipmentRequest.CustomerOrderId).GetAwaiter().GetResult();
                if (order != null)
                {
                    var Shipment = order.Shipments.FirstOrDefault(s => s.Id == newShipmentRequest.Id);
                    if (Shipment != null)
                    {
                        
                    }
                }
            });
        }*/
    }
}
