using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Foundation.Commerce.Payment.Alepay
{
    public class RSACrypt
    {
        private readonly RsaKeyParameters _rsaKeyParameters;
        public RSACrypt(string publicKey)
        {
            var keyInfoData = Convert.FromBase64String(publicKey);
            _rsaKeyParameters = PublicKeyFactory.CreateKey(keyInfoData) as RsaKeyParameters;
        }

        public string Encrypt(object obj)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new TickDateTimeConverter() }
            };

            var serialized = JsonConvert.SerializeObject(obj, settings);
            var payloadBytes = Encoding.UTF8.GetBytes(serialized);

            var cipher = GetAsymmetricBlockCipher(true);
            var encrypted = Process(cipher, payloadBytes);

            var encoded = Convert.ToBase64String(encrypted);
            return encoded;
        }

        public T Decrypt<T>(string encryptedText)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new TickDateTimeConverter() }
            };

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            var cipher = GetAsymmetricBlockCipher(false);
            var decrypted = Process(cipher, cipherTextBytes);

            var decoded = Encoding.UTF8.GetString(decrypted);

            return JsonConvert.DeserializeObject<T>(decoded, settings);
        }

        private IAsymmetricBlockCipher GetAsymmetricBlockCipher(bool forEncryption)
        {
            var cipher = new Pkcs1Encoding(new RsaEngine());
            cipher.Init(forEncryption, _rsaKeyParameters);

            return cipher;
        }

        private byte[] Process(IAsymmetricBlockCipher cipher, byte[] payloadBytes)
        {
            var length = payloadBytes.Length;
            var blockSize = cipher.GetInputBlockSize();

            var plainTextBytes = new List<byte>();
            for (int chunkPosition = 0; chunkPosition < length; chunkPosition += blockSize)
            {
                var chunkSize = Math.Min(blockSize, length - chunkPosition);
                plainTextBytes.AddRange(cipher.ProcessBlock(
                    payloadBytes, chunkPosition, chunkSize
                ));
            }

            return plainTextBytes.ToArray();
        }

    }

    public class TickDateTimeConverter : DateTimeConverterBase
    {
        private static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long tick = (long)reader.Value;

            return unixEpoch.AddMilliseconds(tick).ToLocalTime();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}