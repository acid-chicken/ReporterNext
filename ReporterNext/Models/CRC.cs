using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ReporterNext.Models
{
    public class CRC
    {
        private KeyedHashAlgorithm _keyedHash;

        public CRC(KeyedHashAlgorithm keyedHash) =>
            _keyedHash = keyedHash;

        public CRC(KeyedHashAlgorithm keyedHash, string key) :
            this(keyedHash) =>
            keyedHash.Key = Encoding.ASCII.GetBytes(key);

        public CRCResponse GenerateResponse(string value)
            => new CRCResponse()
            {
                ResponseToken = value is null ? default : $"sha256={Convert.ToBase64String(_keyedHash.ComputeHash(Encoding.ASCII.GetBytes(value)))}"
            };
    }

    [JsonObject]
    public class CRCResponse
    {
        [JsonProperty("response_token")]
        public string ResponseToken { get; set; }
    }
}
