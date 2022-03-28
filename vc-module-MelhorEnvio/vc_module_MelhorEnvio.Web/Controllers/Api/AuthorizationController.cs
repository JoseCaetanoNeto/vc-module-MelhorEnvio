using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vc_module_MelhorEnvio.Core;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using static vc_module_MelhorEnvio.Core.ModuleConstants;

namespace vc_module_MelhorEnvio.Web.Controllers.Api
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/melhorenvio/oauth")]
    public class AuthorizationController : Controller
    {
        private const string _state = "vc30";
        private const string _scope = "purchases-read orders-read products-read cart-read cart-write shipping-calculate shipping-cancel shipping-checkout shipping-companies shipping-generate shipping-preview shipping-print shipping-share shipping-tracking ecommerce-shipping transactions-read";

        private readonly ISettingsManager _settingsManager;
        private readonly ISearchService<ShippingMethodsSearchCriteria, ShippingMethodsSearchResult, ShippingMethod> _ShippingMethodsSearchService;
        private readonly ICrudService<Store> _storeService;

        public AuthorizationController(ISettingsManager settingsManager, IShippingMethodsSearchService pShippingMethodsService, ICrudService<Store> pStoreService)
        {
            _settingsManager = settingsManager;
            _ShippingMethodsSearchService = (ISearchService<ShippingMethodsSearchCriteria, ShippingMethodsSearchResult, ShippingMethod>)pShippingMethodsService;
            _storeService = pStoreService;
        }

        [HttpGet]
        [Route("authorize")]
        public ActionResult<string> Authorize([FromQuery(Name = "store")] string store)
        {
            if (string.IsNullOrEmpty(store))
                return BadRequest();

            var _ShippingMethods = _ShippingMethodsSearchService.SearchAsync(new ShippingMethodsSearchCriteria() { StoreId = store, Keyword = nameof(MelhorEnvioMethod) }).GetAwaiter().GetResult().Results.FirstOrDefault();
            if (_ShippingMethods == null || string.IsNullOrEmpty(_ShippingMethods.Id))
                return BadRequest();

            var retVal = string.Empty;
            if (Request != null)
            {
                Dictionary<string, string> configuration = buildConfigurations(_ShippingMethods.Settings, _state + "|" + store);

                var meo = new MelhorEnvioOAuth(configuration, HttpContext, string.Empty);
                retVal = meo.getLoginURL();
            }
            return Ok(new[] { retVal });
        }

        [HttpGet]
        [Route("complete")]
        [AllowAnonymous]
        public ActionResult<string> Complete([FromQuery(Name = "state")] string state, [FromQuery(Name = "code")] string code, [FromQuery(Name = "error")] string error)
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(state))
            {
                return Ok();
            }

            var splitArray = state.Split("|");

            if (splitArray[0].Equals(_state))
            {
                string store = splitArray[1];
                string objectType = nameof(MelhorEnvioMethod);

                var _ShippingMethods = _ShippingMethodsSearchService.SearchAsync(new ShippingMethodsSearchCriteria() { StoreId = store, Codes = new[] { objectType }, IsActive = true, Keyword = objectType }).GetAwaiter().GetResult().Results.FirstOrDefault();
                if (_ShippingMethods == null || string.IsNullOrEmpty(_ShippingMethods.Id))
                    return BadRequest();

                var storeR = _storeService.GetByIdAsync(_ShippingMethods.StoreId).GetAwaiter().GetResult();

                Dictionary<string, string> configuration = buildConfigurations(_ShippingMethods.Settings, state);
                configuration.Add("code", code);
                configuration["scope"] = "read";

                var meo = new MelhorEnvioOAuth(configuration, HttpContext, buildAgent(storeR.Name, storeR.AdminEmail));
                var json = meo.getAccessTokenFromAuthorizationCode(code);

                SeveObjectSettings(Settings.MelhorEnvio.Token.Name, objectTypeRestrict, _ShippingMethods.Id, json);

                return Ok("Registrado...");
            }
            return new BadRequestResult();
        }

        private void SeveObjectSettings(string name, string objectType, string objectId, string value)
        {
            var objectSetting = _settingsManager.GetObjectSettingAsync(name, objectType, objectId).GetAwaiter().GetResult();
            objectSetting.Value = value;
            _settingsManager.SaveObjectSettingsAsync(new[] { objectSetting }).GetAwaiter().GetResult();
        }

        private string buildAgent(string pApplicationName, string pAdminEmail)
        {
            return pApplicationName + " (" + pAdminEmail + ")";
        }

        private Dictionary<string, string> buildConfigurations(ICollection<ObjectSettingEntry> pSettings, string pState)
        {
            bool sandbox = pSettings.GetSettingValue(Settings.MelhorEnvio.Sandbox.Name, false);

            return new Dictionary<string, string>()
                {
                    { "client_id", pSettings.GetSettingValue(Settings.MelhorEnvio.client_id.Name,string.Empty) },
                    { "client_secret", pSettings.GetSettingValue(Settings.MelhorEnvio.client_secret.Name,string.Empty) },
                    { "redirect_uri", GetDisplayUrl(Request) /*pSettings.GetSettingValue(Settings.MelhorEnvio.VCmanagerURL.Name,string.Empty)*/ + "/api/melhorenvio/oauth/complete" },
                    { "authorize_uri", (sandbox ? urlbaseSandbox : urlbase) + "/oauth/authorize" },
                    {"scope", _scope },
                    {"state", pState},
                    { "access_token_uri", (sandbox ? urlbaseSandbox : urlbase) + "/oauth/token" }
                };
        }

        const string SchemeDelimiter = "://";
        public static string GetDisplayUrl(HttpRequest request)
        {
            var scheme = request.Scheme ?? string.Empty;
            var host = request.Host.Value ?? string.Empty;
            var pathBase = request.PathBase.Value ?? string.Empty;

            // PERF: Calculate string length to allocate correct buffer size for StringBuilder.
            var length = scheme.Length + SchemeDelimiter.Length + host.Length + pathBase.Length;

            return new StringBuilder(length).Append(scheme).Append(SchemeDelimiter).Append(host).Append(pathBase).ToString();
        }
    }
}