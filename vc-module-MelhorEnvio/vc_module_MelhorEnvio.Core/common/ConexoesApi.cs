using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace vc_module_MelhorEnvio.Core
{
    public static class ConexoesApi
    {

        /// <summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pMethod">POST/GET</param>
        /// <param name="pUrl"></param>
        /// <param name="pObjetoEntrada"></param>
        /// <param name="pAuthorization"></param>
        /// <returns>Efetua chamada POST/GET para uma determinada API</returns>
        /// </summary>
        /// <param name="pUrl"></param><param name="pAuthorization"></param><param name="pMethod">POST/GET</param><param name="pObjetoEntrada"></param>
        public static T EfetuarChamadaApi<T2, T>(string pUrl, string pAuthorization, string pMethod, string pAgent = null, object pObjetoEntrada = null, Func<string,string> pPreprocResult = null) where T2 : Models.ErrorOut, new() where T : Models.IErrorOut, new()
        {
            string objEntradaSerializado = string.Empty;
            if (pObjetoEntrada != null)
                objEntradaSerializado = JsonConvert.SerializeObject(pObjetoEntrada);

            using (MyWebClient client = new MyWebClient())
            {
                T objRetorno = default(T);
                try
                {

                    // Seta o certificado como válido em caso de utilizar SSL
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                    //client.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");

                    if (!string.IsNullOrEmpty(pAuthorization))
                        client.Headers.Add("Authorization", pAuthorization);

                    if (!string.IsNullOrEmpty(pAgent))
                        client.Headers["User-Agent"] = pAgent;

                    string objRetornoSerializado;
                    if (pMethod == "GET")
                        objRetornoSerializado = client.DownloadString(pUrl);
                    else
                        objRetornoSerializado = client.UploadString(pUrl, pMethod, objEntradaSerializado);

                    if (pPreprocResult != null)
                        objRetornoSerializado = pPreprocResult(objRetornoSerializado);

                    objRetorno = JsonConvert.DeserializeObject<T>(objRetornoSerializado);
                }
                catch (WebException ex)
                {
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            throw;
                        case WebExceptionStatus.ProtocolError:
                            HttpWebResponse response = (HttpWebResponse)ex.Response;
                            if (response != null)
                            {
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.GatewayTimeout:
                                    case HttpStatusCode.RequestTimeout:
                                        throw;
                                    case HttpStatusCode.Conflict:
                                    case HttpStatusCode.BadRequest:
                                    case HttpStatusCode.Unauthorized:
                                    case HttpStatusCode.PaymentRequired:
                                    case HttpStatusCode.Forbidden:
                                    case HttpStatusCode.NotFound:
                                    case HttpStatusCode.InternalServerError:
                                    case HttpStatusCode.BadGateway:
                                    case HttpStatusCode.UnprocessableEntity:
                                        using (StreamReader s = new StreamReader(ex.Response.GetResponseStream()))
                                        {
                                            string error = s.ReadToEnd();

                                            T2 typeError = JsonConvert.DeserializeObject<T2>(error);
                                            if (string.IsNullOrWhiteSpace(typeError.message))
                                                typeError.message = typeError.error;

                                            // injeta no retornos json's atuais class faze status_code, para pegar o código do http code e propriedade de erro
                                            typeError.status_code = (int)response.StatusCode;

                                            // inserta classe de erro no objeto 
                                            if (objRetorno == null)
                                                objRetorno = new T();
                                            objRetorno.errorOut = typeError;
                                            return objRetorno;
                                        }
                                }
                            }
                            break;
                        default:
                            {
                                throw;
                            }
                    }

                }

                return objRetorno;
            }

        }

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }
    }
}
