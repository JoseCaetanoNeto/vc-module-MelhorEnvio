using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vc_module_MelhorEnvio.Core
{
    public class MelhorEnvioService
    {
        const string C_OauthToken = "{urlbase}/oauth/token";
        const string C_Calculate = "{urlbase}/api/v2/me/shipment/calculate";
        const string C_Inserir = "{urlbase}/api/v2/me/cart";
        const string C_checkout = "{urlbase}/api/v2/me/shipment/checkout";
        const string C_generate = "{urlbase}/api/v2/me/shipment/generate";
        const string C_print = "{urlbase}/api/v2/me/shipment/print";
        const string C_Tracking = "{urlbase}/api/v2/me/shipment/tracking";
        const string C_agencies = "{urlbase}/api/v2/me/shipment/agencies";
        const string C_Cancel = "{urlbase}/api/v2/me/shipment/cancel";

        readonly bool m_SandBox;
        readonly string m_applycation;
        readonly string m_AdminEmail;
        readonly string m_client_id;
        readonly string m_client_secret;
        string m_Access;
        public Action<string> onSaveNewToken;

        public MelhorEnvioService(string pClient_id, string pClient_secret, bool pSandBox, string pApplycation_id, string pAdminEmail, string pAccess)
        {
            m_client_id = pClient_id;
            m_client_secret = pClient_secret;
            m_SandBox = pSandBox;
            m_applycation = pApplycation_id;
            m_AdminEmail = pAdminEmail;
            m_Access = pAccess;
        }

        public List<Models.CalculateOut> Calculate(Models.CalculateIn pCalc)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(List<Models.CalculateOut>);

            var result = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, List<Models.CalculateOut>>(BuildUrl(C_Calculate), GetAut(), "POST", buildAgent(), pCalc);
            if (isInvalidToken(result.Item1))
                result = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, List<Models.CalculateOut>>(BuildUrl(C_Calculate), GetAut(), "POST", buildAgent(), pCalc);
            return result.Item2;
        }

        public Models.CartOut InserirCart(Models.CartIn pCart)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.CartOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CartOut>(BuildUrl(C_Inserir), GetAut(), "POST", buildAgent(), pCart);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CartOut>(BuildUrl(C_Inserir), GetAut(), "POST", buildAgent(), pCart);
            return transation.Item2;
        }

        public Models.ErrorOut RemoveCart(string pOrdem)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.ErrorOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.ErrorOut>(BuildUrl(C_Inserir) + "/" + pOrdem, GetAut(), "DELETE", buildAgent());
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.ErrorOut>(BuildUrl(C_Inserir) + "/" + pOrdem, GetAut(), "DELETE", buildAgent());
            return transation.Item2;
        }

        public Models.CheckoutOut Checkout(Models.CheckoutIn pOrders)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.CheckoutOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CheckoutOut>(BuildUrl(C_checkout), GetAut(), "POST", buildAgent(), pOrders);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CheckoutOut>(BuildUrl(C_checkout), GetAut(), "POST", buildAgent(), pOrders);
            return transation.Item2;
        }

        public Models.CancelOut.Order Cancel(string pOrdem, string pDescription)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.CancelOut.Order);

            Models.CancelIn cancel = new Models.CancelIn()
            {
                order = new Models.CancelIn.Order()
                {
                    Description = pDescription,
                    Id = pOrdem,
                    ReasonId = "2"
                }
            };

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CancelOut>(BuildUrl(C_Cancel), GetAut(), "POST", buildAgent(), cancel);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.CancelOut>(BuildUrl(C_Cancel), GetAut(), "POST", buildAgent(), cancel);

            return transation.Item2[pOrdem];
        }

        public Models.TrackingOut Tracking(Models.TrackingIn pOrders)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.TrackingOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.TrackingOut>(BuildUrl(C_Tracking), GetAut(), "POST", buildAgent(), pOrders);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.TrackingOut>(BuildUrl(C_Tracking), GetAut(), "POST", buildAgent(), pOrders);
            return transation.Item2;
        }

        public Models.GenerateOut Generate(List<string> pOrders)
        {
            var orders = new Models.GenerateIn() { Orders = pOrders };

            if (string.IsNullOrEmpty(m_Access))
                return default(Models.GenerateOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.GenerateOut>(BuildUrl(C_generate), GetAut(), "POST", buildAgent(), orders);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.GenerateOut>(BuildUrl(C_generate), GetAut(), "POST", buildAgent(), orders);
            return transation.Item2;
        }

        public Models.PrintOut Print(Models.PrintIn pOrders)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.PrintOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.PrintOut>(BuildUrl(C_print), GetAut(), "POST", buildAgent(), pOrders);
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.PrintOut>(BuildUrl(C_print), GetAut(), "POST", buildAgent(), pOrders);
            return transation.Item2;
        }

        public Models.AgencieOut GetAgencyInfo(int pAgencia)
        {
            if (string.IsNullOrEmpty(m_Access))
                return default(Models.AgencieOut);

            var transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.AgencieOut>(BuildUrl(C_agencies) + "/" + pAgencia, GetAut(), "GET", buildAgent());
            if (isInvalidToken(transation.Item1))
                transation = ConexoesApi.EfetuarChamadaApi<Models.ErrorOut, Models.AgencieOut>(BuildUrl(C_agencies) + "/" + pAgencia, GetAut(), "GET", buildAgent());
            return transation.Item2;
        }

        private bool isInvalidToken(Models.ErrorOut transation)
        {
            if (transation != null && transation.message != null && transation.status_code == 401)
            {
                string newToken = requestNewToken();
                if (onSaveNewToken != null)
                    onSaveNewToken(newToken);

                if (!string.IsNullOrEmpty(newToken))
                {
                    m_Access = newToken;
                    return true;
                }
            }

            return false;
        }

        private string requestNewToken()
        {
            if (!string.IsNullOrEmpty(m_Access))
            {
                var configuration = new Dictionary<string, string>()
                {
                    { "client_id", m_client_id },
                    { "client_secret", m_client_secret },
                    { "access_token_uri", BuildUrl(C_OauthToken) }
                };
                MelhorEnvioOAuth meo = new MelhorEnvioOAuth(configuration, null, buildAgent());

                Access currentJsonAccessToken = JsonConvert.DeserializeObject<Access>(m_Access);
                string newAccessToken = meo.getAccessTokenFromRefreshToken(currentJsonAccessToken.refresh_token);

                return newAccessToken;
            }
            return string.Empty;
        }

        string GetAut()
        {
            string access_token = JsonConvert.DeserializeObject<Access>(m_Access).access_token;
            return string.Format("Bearer {0}", access_token);
        }

        private string buildAgent()
        {
            return m_applycation + " (" + m_AdminEmail + ")";
        }

        string BuildUrl(string url)
        {
            url = url.Replace("{urlbase}", m_SandBox ? ModuleConstants.urlbaseSandbox : ModuleConstants.urlbase);
            return url;
        }

    }
}
