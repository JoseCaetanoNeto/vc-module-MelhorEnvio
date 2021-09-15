using FluentValidation;
using System.Linq;
using vc_module_MelhorEnvio.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;

namespace vc_module_MelhorEnvio.Web.Validation
{
    public class MelhorEnvioValidator : AbstractValidator<Shipment>
    {
        public MelhorEnvioValidator(ICustomerOrderService pOrderService)
        {
            RuleFor(shipment => shipment).Custom((newShipmentRequest, context) =>
            {
                var order = pOrderService.GetByIdAsync(newShipmentRequest.CustomerOrderId).GetAwaiter().GetResult();
                if (order != null)
                {
                    var Shipment = order.Shipments.FirstOrDefault(s => s.Id == newShipmentRequest.Id && s.ShipmentMethodCode == nameof(MelhorEnvioMethod) && MelhorEnvioMethod.DecodeOptionName(s.ShipmentMethodOption).CompanyID != ModuleConstants.K_Company_CORREIOS);
                    if (Shipment != null && Shipment.Status != newShipmentRequest.Status && newShipmentRequest.Status == ((MelhorEnvioMethod)Shipment.ShippingMethod).SendDataOnShippingStatus)
                    {
                        var invoiceKey = order.DynamicProperties.FirstOrDefault(p => p.Name == ModuleConstants.K_InvoiceKey)?.Values.FirstOrDefault()?.Value;
                        if (invoiceKey == null)
                            context.AddFailure("Preencher o número da Nota fiscal (Dynamic properties -> InvoiceKey)! É obritatória para encomendas com envio com transportadores que não seja Correios");
                    }
                }
            });
        }
    }
}
