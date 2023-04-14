using Newtonsoft.Json;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Core.Models;
using Xunit;

namespace vc_module_MelhorEnvio.Tests
{
    public class Test
    {
        public Test()
        {
        }

        [Fact]
        public void Run_Json()
        {
            string jconStr = "{\r\n\t\"generate_key\":\"71a08ace-d803-49ac-bfc8-b2e03b236307\",\r\n\t\"5cd1f25c-2865-41b7-b677-012c3359c6c6\":{\"message\":\"Envio encaminhado para gera\\u00e7\\u00e3o\",\"status\":true},\r\n\t\"5cd1f25c-2865-41b7-b677-012c3359c6c7\":{\"message\":\"Envio encaminhado para gera\\u00e7\\u00e3o\",\"status\":true},\r\n\t\"5cd1f25c-2865-41b7-b677-012c3359c6c8\":{\"message\":\"Envio encaminhado para gera\\u00e7\\u00e3o\",\"status\":true}\r\n}";
            dynamic objRetorno = JsonConvert.DeserializeObject(jconStr);
            if (objRetorno.generate_key != null)
                objRetorno.generate_key = null;

            var jconStr2 = JsonConvert.SerializeObject(objRetorno);
            var objst = JsonConvert.DeserializeObject<GenerateOut>(jconStr2);

            //Assert.Equal(jconStr2, jconStr);
        }
        [Fact]
        public void Run_Tracking()
        {
            ////Assert.Equal(0, 0);
            //MelhorEnvioService me = new MelhorEnvioService("", "", true, "ScrubUp", "email@algumacoisa.com.br", "{\"token_type\":\"Bearer\",\"expires_in\":2592000,\"access_token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6ImZhNzNjYjgwNDI1OWZhNGFmYWVlMTRmMWIzMWU3NzNjNWE1NzRjM2YyMTg2OTVhYmY1YzE1MmM4Njk1NWNkNjMzMDJjZDY5NDQ0MDA5ZmRkIn0.eyJhdWQiOiIyMTUyIiwianRpIjoiZmE3M2NiODA0MjU5ZmE0YWZhZWUxNGYxYjMxZTc3M2M1YTU3NGMzZjIxODY5NWFiZjVjMTUyYzg2OTU1Y2Q2MzMwMmNkNjk0NDQwMDlmZGQiLCJpYXQiOjE2MzA1ODY3NjksIm5iZiI6MTYzMDU4Njc2OSwiZXhwIjoxNjMzMTc4NzY5LCJzdWIiOiIxZDg1MDk1Yi01NWY5LTQ3ZWYtYjU4Ny0xMGYwNTk5MzhkNzAiLCJzY29wZXMiOlsiY2FydC1yZWFkIiwiY2FydC13cml0ZSIsInNoaXBwaW5nLWNhbGN1bGF0ZSIsInNoaXBwaW5nLWNhbmNlbCIsInNoaXBwaW5nLWNoZWNrb3V0Iiwic2hpcHBpbmctY29tcGFuaWVzIiwic2hpcHBpbmctZ2VuZXJhdGUiLCJzaGlwcGluZy1wcmV2aWV3Iiwic2hpcHBpbmctcHJpbnQiLCJzaGlwcGluZy1zaGFyZSIsInNoaXBwaW5nLXRyYWNraW5nIiwiZWNvbW1lcmNlLXNoaXBwaW5nIiwidHJhbnNhY3Rpb25zLXJlYWQiXX0.o-XuCQGOpXl3gIi_3mVaekhQM_rE72dEnNbncP3qLwmdUb2ZAI_TSUwEuKXrKUTleSgOjU5DZ4iQqLgpCkSvheL-1kQG7vozsMldlc1Qkbk77Ghj9Bv1YOHemxBxhFqfNHBzYr3s9M5uSeGcX53iLzA5x_2yJp8Z0B8HJ-95c9AzxAcdwyJykrVOG5ekx5PwZA3Tr8VBXE54fE3G3RJzGY7_HUUrXji2PYT8LcYZ3FDl7hH_Zw2ASZmb3pTy5HjZs4OOUWGOMr2Y4cNnqe1bHOHwxygBSM-XYwft9x7hBwkBKPGqGIiehwc6qhkP9CkKtSySeSobMwV3fpJQaUuH4aoM0o2xqmb1uT9zQs9zUyjsx_sjkJe7RBxt6s53W8EqFoBAl5W3OIrm9CmXiWjENwPMwBf6DkboFNoIXplWoyqriJ27RWK0M3Qmb0kYXGoS-e8_gb70vnR6IDKy8JhXPf45dz4B1ua4pZOkoCna3hKSJdFqG2_gQVZmq44hIVIK-HIoNKFO97CSzwJogQ-trknzlC8K1-a9SS4qkYmyeGJ83X6tx3rFipx7uCLiJmGAzFCDGTgGbZGOjrHMvoziwMM0pU_5X-mX7Rss-bfhnvoRsAYy5nQ9TJQ-Pbrg7MuqUkbZg0i2POCEE1q4GRYqihVA2FzbtFnhPCy4pSvLNPs\",\"refresh_token\":\"def502005323b0ca988dbd8ae47af641171813f6085613fd1a61c055d03afdbd239af69f31a04b315873069bfb2c1360cc039863b52511a27e1125eb8311ef6568bc31e49036f639bf98e0027913aa444ff7d4f7a59efa47f4d84057ea1bd5b17d9686050368cf7b8d1893e81dbfb8b05e3a388bd44b4d0a9253c4324862e24b1cd510b9adc8a3b7fa752ad3e78a64d86ee7be8d59b09dc84b47a07d4078e5841fb4fc9304c221fd3f637e81c098ed98102481d4d5a82cb74c7bc577c551623b84f3f36fe42d0494da1195a9b1cf7c3618c53f1b8466c1f032bbc8b0110f83f5ff90afdfbfd12d8c7f0e451beac9c673af415963bb5642a14879bf4f64f21fb6dfe723608f19ba4a2d58d2fb93a782400bd320e593587bbf59624d2b5dc0afbc44d26b52a648d351e92f5fc08f0e6018884b0b8f7ec486c39c31a616bc1e5fd3f8866eeaa2b907520a8cf9931d6a3cd6f4d962d94745c2cdc6e824f14117070942033243e82c027e4a913442f8676d30b1f71b5f7fae5eb20f24f66a9a42586da171067a921e789456f482c0b25d677cd8deb0bb06f39c0c0ece8fa26b7bf8b19920441c9475b415c847f3ccca16e244bb96abc3eb6d50bddbfd7d016ad3b8c1bdee7c7d2b9c1d23d5e86d5502f0443bf42b2df78dfbfbd9016704e9ebe7e537db31d9a14264b16b7fe2d4f4b90c2d4a239a7fde0658a2cf007891145df5ae84841333e2dd5322bfac89023f515ccd5dc6993923e385e7a593fc26a9e42d3fb1b6315640bdc6b194ffc2841b3187adb6b971e6f82d41fd1a40e5348bec5f5fbfda8f2d06117324bac021ad2e2f5ece05a50db635b93443766c2e0ed93c6132897c2e8a4291eaa3c4ce8db22088c2fe17901977c42c46ad\"}");
            //var ret = me.Tracking(new Core.Models.TrackingIn()
            //{
            //    Orders = new System.Collections.Generic.List<string>() {
            //    { "ac26033a-1282-46e8-8dd3-9341a11fb161" },
            //    { "bd6d630d-9783-481c-aac4-ce0a1bd63406" }
            //    }
            //});
            //Assert.Equal(2, ret.Count);
        }
    }
}
