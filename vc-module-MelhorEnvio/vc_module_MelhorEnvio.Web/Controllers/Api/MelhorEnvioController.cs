using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace vc_module_MelhorEnvio.Web.Controllers.Api
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/melhorenvio")]
    public class MelhorEnvioController : Controller
    {
        private readonly ICrudService<CustomerOrder> _orderService;

        public MelhorEnvioController(ICrudService<CustomerOrder> pCustomerOrderService)
        {
            _orderService = pCustomerOrderService;
        }

        [HttpPost]
        [Route("cart")]
        public async Task<ActionResult> InsertCart([FromForm] dataInsertCart data)
        {
            var order = await _orderService.GetByIdAsync(data.order_id);
            if (order != null)
            {
                var melhoEnvioSrv = order.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod)).FirstOrDefault()?.ShippingMethod as MelhorEnvioMethod;
                if (melhoEnvioSrv != null && melhoEnvioSrv.SendMECart(order))
                {
                    await _orderService.SaveChangesAsync(new[] { order });
                }
            }

            return Ok();
        }

        public class dataInsertCart
        {
            public string order_id { get; set; }
        }
    }
}