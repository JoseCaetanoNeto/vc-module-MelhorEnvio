using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vc_module_MelhorEnvio.Core;


namespace vc_module_MelhorEnvio.Web.Controllers.Api
{
    [Route("api/melhorenvio")]
    public class vcmoduleMelhorEnvioController : Controller
    {
        // GET: api/vcmoduleMelhorEnvio
        /// <summary>
        /// Get message
        /// </summary>
        /// <remarks>Return "Hello world!" message</remarks>
        [HttpGet]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public ActionResult<string> Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
