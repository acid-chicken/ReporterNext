using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ReporterNext.Models
{
    [JsonObject]
    public class CRCResponse
    {
        private static HMAC _hmac = HMACSHA256.Create();

        public CRCResponse()
        {
        }

        public CRCResponse(string key, string value)
        {
            _hmac.Key = Encoding.ASCII.GetBytes(key);
            ResponseToken = $"sha256={Convert.ToBase64String(_hmac.ComputeHash(Encoding.ASCII.GetBytes(value)))}";
        }

        [JsonProperty("response_token")]
        public string ResponseToken { get; set; }
    }
}
