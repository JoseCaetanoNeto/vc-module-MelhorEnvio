using Microsoft.AspNetCore.Mvc;
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
        private readonly IMelhorEnvioService _melhorEnvioService;
        private readonly ICrudService<CustomerOrder> _orderService;

        public MelhorEnvioController(ICrudService<CustomerOrder> pCustomerOrderService, IMelhorEnvioService pMelhorEnvioService)
        {
            _melhorEnvioService = pMelhorEnvioService;
            _orderService = pCustomerOrderService;
        }

        [HttpPost]
        [Route("cart")]
        public async Task<ActionResult> InsertCart([FromForm] dataInsertCart data)
        {
            var order = await _orderService.GetByIdAsync(data.order_id);
            if (order != null)
            {
                if (_melhorEnvioService.SendMECart(order))
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