using FluentValidation;
using System.Linq;
using vc_module_MelhorEnvio.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace vc_module_MelhorEnvio.Web.Validation
{
    public class MelhorEnvioValidator : AbstractValidator<Shipment>
    {
        public MelhorEnvioValidator(ICrudService<CustomerOrder> pOrderService)
        {
            RuleFor(shipment => shipment).Custom((newShipmentRequest, context) =>
            {
                var order = pOrderService.GetByIdAsync(newShipmentRequest.CustomerOrderId).GetAwaiter().GetResult();
                if (order != null)
                {
                    var Shipment = order.Shipments.FirstOrDefault(s => s.Id == newShipmentRequest.Id && s.ShipmentMethodCode == nameof(MelhorEnvioMethod));
                    if (Shipment != null && Shipment.Status != newShipmentRequest.Status && newShipmentRequest.Status == ((MelhorEnvioMethod)Shipment.ShippingMethod).SendDataOnShippingStatus)
                    {
                        if (order.Total > order.InPayments.Where(p => p.PaymentStatus == PaymentStatus.Paid).Sum(p => p.Sum))
                        {
                            context.AddFailure("Para envio para transportadora � necess�rio que o Pedido esteja pago integralmente!");
                        }

                        if (MelhorEnvioMethod.DecodeOptionName(Shipment.ShipmentMethodOption).CompanyID != ModuleConstants.K_Company_CORREIOS)
                        {
                            var invoiceKey = order.DynamicProperties.FirstOrDefault(p => p.Name == ModuleConstants.K_InvoiceKey)?.Values.FirstOrDefault()?.Value;
                            if (invoiceKey == null)
                                context.AddFailure("Preencher o n�mero da Nota fiscal (Dynamic properties -> InvoiceKey)! � obritat�ria para encomendas com envio com transportadores que n�o seja Correios");
                        }
                    }
                }
            });
        }
    }
}
