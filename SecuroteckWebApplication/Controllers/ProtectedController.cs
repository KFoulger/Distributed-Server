using SecuroteckWebApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;

namespace SecuroteckWebApplication.Controllers
{
    public class ProtectedController : ApiController
    {
        UserDatabaseAccess udba = new UserDatabaseAccess();

        [APIAuthorise]
        [HttpGet]
        public HttpResponseMessage Hello()
        {
            IEnumerable<string> key;
            Request.Headers.TryGetValues("ApiKey", out key);
            if (key != null)
            {
                User u = udba.ReturnUser(key.First());
                udba.CreateLog(key.First(), "protected/hello");
                return Request.CreateResponse(HttpStatusCode.OK, "Hello " + u.UserName);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Please enter a valid APIkey");
            }
        }

        [APIAuthorise]
        [HttpGet]
        public HttpResponseMessage SHA1([FromUri]string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                byte[] asciiByteMessage = System.Text.Encoding.ASCII.GetBytes(message);

                byte[] sha1ByteMessage;
                SHA1 sha1Provider = new SHA1CryptoServiceProvider();
                sha1ByteMessage = sha1Provider.ComputeHash(asciiByteMessage);

                IEnumerable<string> key;
                Request.Headers.TryGetValues("ApiKey", out key);
                udba.CreateLog(key.First(), "protected/sha1");
                return Request.CreateResponse(HttpStatusCode.OK, ByteArrayToHexString(sha1ByteMessage));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

        }

        [APIAuthorise]
        [HttpGet]
        public HttpResponseMessage SHA256([FromUri]string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                byte[] asciiByteMessage = System.Text.Encoding.ASCII.GetBytes(message);

                byte[] sha256ByteMessage;
                SHA256 sha256Provider = new SHA256CryptoServiceProvider();
                sha256ByteMessage = sha256Provider.ComputeHash(asciiByteMessage);

                IEnumerable<string> key;
                Request.Headers.TryGetValues("ApiKey", out key);
                udba.CreateLog(key.First(), "protected/sha256");
                return Request.CreateResponse(HttpStatusCode.OK, ByteArrayToHexString(sha256ByteMessage));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
        }

        [APIAuthorise]
        [HttpGet]
        public HttpResponseMessage GetPublicKey()
        {
            IEnumerable<string> key;
            Request.Headers.TryGetValues("ApiKey", out key);
            udba.CreateLog(key.First(), "protected/getpublickey");
            try
            {
                string xmlKey = WebApiConfig.rsa.ToXmlString(false);
                return Request.CreateResponse(HttpStatusCode.OK, xmlKey);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Couldn't get the public key");
            }


        }

        [APIAuthorise]
        [HttpGet]
        public HttpResponseMessage Sign([FromUri]string message)
        {
            byte[] asciiByteMessage = System.Text.Encoding.ASCII.GetBytes(message);
            byte[] hashedMessage = WebApiConfig.rsa.SignData(asciiByteMessage, CryptoConfig.MapNameToOID("SHA1"));

            IEnumerable<string> key;
            Request.Headers.TryGetValues("ApiKey", out key);
            udba.CreateLog(key.First(), "protected/sign");
            return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(hashedMessage));
        }

        [APIAuthorise]
        [AdminRole]
        [HttpGet]
        public HttpResponseMessage AddFifty([FromUri]string encryptedInteger, [FromUri]string encryptedSymKey, [FromUri]string encryptedIV)
        {
            try
            {
                byte[] convertedHex;
                ConvertFromHex(encryptedInteger, out convertedHex);
                byte[] intInBytes = WebApiConfig.rsa.Decrypt(convertedHex, true);
                string intInString = System.Text.Encoding.ASCII.GetString(intInBytes);
                int decryptedInt = int.Parse(intInString) + 50;

                ConvertFromHex(encryptedSymKey, out convertedHex);
                byte[] decryptedKey = WebApiConfig.rsa.Decrypt(convertedHex, true);

                ConvertFromHex(encryptedIV, out convertedHex);
                byte[] decryptedIV = WebApiConfig.rsa.Decrypt(convertedHex, true);
                AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider();
                aesCrypto.Key = decryptedKey;
                aesCrypto.IV = decryptedIV;
                ICryptoTransform encryptor = aesCrypto.CreateEncryptor();

                byte[] encryptedMessageBytes;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(decryptedInt);
                        }
                        encryptedMessageBytes = ms.ToArray();
                    }
                }
                IEnumerable<string> key;
                Request.Headers.TryGetValues("ApiKey", out key);
                udba.CreateLog(key.First(), "protected/AddFifty");
                return Request.CreateResponse(HttpStatusCode.OK, BitConverter.ToString(encryptedMessageBytes));
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "An Error Occured!");
            }
        }

        private static void ConvertFromHex(string encryptedData, out byte[] convertedHex)
        {
            string[] hexString = encryptedData.Split('-');
            convertedHex = new byte[hexString.Length];
            for (int i = 0; i < hexString.Length; i++)
            {
                byte converted = Convert.ToByte(hexString[i], 16);
                convertedHex[i] = converted;
            }
        }

        static string ByteArrayToHexString(byte[] byteArray)
        {
            string hexString = "";
            if (null != byteArray)
            {
                foreach (byte b in byteArray)
                {
                    hexString += b.ToString("x2");
                }
            }
            return hexString;
        }

    }


}
