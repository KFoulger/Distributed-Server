using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.IO;

namespace SecuroteckClient
{
    #region Task 10 and beyond
    class Client
    {
        static private string apiKey = "", userName = "";
        static private string publicKey = "";

        static void Main(string[] args)
        {

            Console.WriteLine("Hello. What would you like to do?");
            while (true)
            {
                string[] response = Console.ReadLine().Split(' ');
                if (response.Length <= 1)
                {
                    Console.WriteLine("Input at least two values");
                }
                else
                {
                    switch (response[0])
                    {
                        case "TalkBack":
                            RunTalkbackAsync(response).Wait();
                            break;
                        case "User":
                            RunUserAsync(response).Wait();
                            break;
                        case "Protected":
                            RunProtectedAsync(response).Wait();
                            break;
                        case "exit":
                            Environment.Exit(0);
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("What would you like to do?");
            }
        }
        static async Task RunTalkbackAsync(string[] input)
        {
            switch (input[1])
            {
                case "Hello":
                    try
                    {
                        Task<string> task = GetStringAsync("/api/talkback/hello");
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Sort":
                    string message = "/api/talkback/sort?";
                    input[2] = input[2].Trim(new char[2] { '[', ']' });
                    string[] numbers = input[2].Split(',');
                    foreach (string number in numbers)
                    {
                        message += "integers=" + number + "&";
                    }
                    message = message.TrimEnd('&');
                    try
                    {
                        Task<string> task = GetStringAsync(message);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                default:
                    Console.WriteLine("Values not recognised");
                    break;
            }

        }

        static async Task RunUserAsync(string[] input)
        {
            switch (input[1])
            {
                case "Get":
                    try
                    {
                        Task<string> task = GetStringAsync("/api/user/new?username=" + input[2]);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Post":
                    try
                    {
                        string jsonInput = "\"" + input[2] + "\"";
                        Task<string> task = PostStringAsync("/api/user/new", jsonInput);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            
                            if (!task.Result.Trim('"').StartsWith("Oops"))
                            {
                                userName = input[2];
                                apiKey = task.Result.Trim('"');
                                Console.WriteLine("Got API Key");
                            }
                            else
                            {
                                Console.WriteLine(task.Result.Trim('"'));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Set":
                    if (input.Length != 4)
                    {
                        Console.WriteLine("Enter only a username and apikey");
                        break;
                    }
                    if (!string.IsNullOrEmpty(input[2]) && !string.IsNullOrEmpty(input[3]))
                    {
                        userName = input[2];
                        apiKey = input[3];
                        Console.WriteLine("Stored");
                    }
                    else
                    {
                        Console.WriteLine("Don't enter blank spaces");
                    }
                    break;
                case "Delete":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = DeleteStringAsync();
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Role":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        string jsonInput = "{ \"username\": \"" + input[2] + "\", \"role\": \"" + input[3] + "\"}";
                        Task<string> task = PostStringAsync("/api/user/changerole", jsonInput);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }

                    break;
                default:
                    Console.WriteLine("Values not recognised");
                    break;
            }
        }

        static async Task RunProtectedAsync(string[] input)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            switch (input[1])
            {
                case "Hello":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = GetStringAsync("/api/protected/hello");
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "SHA1":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = GetStringAsync("/api/protected/sha1?message=" + input[2]);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "SHA256":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = GetStringAsync("/api/protected/sha256?message=" + input[2]);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            Console.WriteLine(task.Result.Trim('"'));
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Get":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = GetStringAsync("/api/protected/getpublickey");
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            Console.Clear();
                            if (task.Result.Trim('"').StartsWith("<RSAKeyValue>"))
                            {
                                publicKey = task.Result.Trim('"');
                                rsaProvider.FromXmlString(publicKey);
                                Console.WriteLine("Got Public Key");
                            }
                            else
                            {
                                Console.WriteLine("Couldn’t Get the Public Key");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "Sign":
                    try
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                            break;
                        }
                        Task<string> task = GetStringAsync("/api/protected/sign?message=" + input[2]);
                        if (await Task.WhenAny(task, Task.Delay(20000)) == task)
                        {
                            if (string.IsNullOrEmpty(publicKey))
                            {
                                Console.Clear();
                                Console.WriteLine("Client doesn’t yet have the public key");
                                break;
                            }

                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                            rsa.FromXmlString(publicKey);
                            byte[] byteMessage = Encoding.ASCII.GetBytes(input[2]);
                            string[] hexString = task.Result.Trim('"').Split('-');
                            byte[] convertedHex = ConvertFromHex(hexString);
                            if (!rsa.VerifyData(byteMessage, CryptoConfig.MapNameToOID("SHA1"), convertedHex))
                            {
                                Console.Clear();
                                Console.WriteLine("Message was not successfully signed");
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Message was successfully signed");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Request timed out");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetBaseException().Message);
                    }
                    break;
                case "AddFifty":
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        Console.WriteLine("You nedd to do a User Post or User Set first");
                        break;
                    }
                    if (string.IsNullOrEmpty(publicKey))
                    {
                        Console.WriteLine("Client doesn't yet have the public key");
                        break;
                    }
                    if(input.Length != 3)
                    {
                        Console.WriteLine("Please input the correct values");
                    }
                    int addedNumber = 0;
                    try
                    {
                        addedNumber = int.Parse(input[2]);
                    }
                    catch
                    {
                        Console.WriteLine("A valid integer must be given");
                    }

                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    aes.GenerateKey();
                    aes.GenerateIV();

                    rsaProvider.FromXmlString(publicKey);
                    string encryptedInt = BitConverter.ToString(rsaProvider.Encrypt(Encoding.ASCII.GetBytes(input[2]), true));
                    string encryptedKey = BitConverter.ToString(rsaProvider.Encrypt(aes.Key, true));
                    string encryptedIV = BitConverter.ToString(rsaProvider.Encrypt(aes.IV, true));

                    string message = "/api/protected/addfifty?encryptedInteger=" + encryptedInt
                                            + "&encryptedSymKey=" + encryptedKey + "&encryptedIV=" + encryptedIV;
                    try
                    {
                        Task<string> task = GetStringAsync("/api/protected/addfifty?encryptedInteger=" + encryptedInt
                                            + "&encryptedSymKey=" + encryptedKey + "&encryptedIV=" + encryptedIV);
                        if (await Task.WhenAny(task, Task.Delay(90000000)) == task)
                        {
                            if(task.Result.Trim('"') == "An Error Occured!")
                            {
                                Console.WriteLine(task.Result.Trim('"'));
                                break;
                            }
                            string[] hexString = task.Result.Trim('"').Split('-');
                            byte[] encryptedMessage = ConvertFromHex(hexString);
                            ICryptoTransform decryptor = aes.CreateDecryptor();

                            string returnedMessage;
                            using (MemoryStream ms = new MemoryStream(encryptedMessage))
                            {
                                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                                {
                                    using (StreamReader sr = new StreamReader(cs))
                                    {
                                        returnedMessage = sr.ReadToEnd();
                                    }
                                }
                            }

                            Console.WriteLine(returnedMessage);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("An Error Occured!");
                    }
                    break;
                default:
                    Console.WriteLine("Values not recognised");
                    break;
            }
        }

        private static byte[] ConvertFromHex(string[] hexString)
        {
            byte[] convertedHex = new byte[hexString.Length];
            for (int i = 0; i < hexString.Length; i++)
            {
                byte converted = Convert.ToByte(hexString[i], 16);
                convertedHex[i] = converted;
            }

            return convertedHex;
        }

        static async Task<string> GetStringAsync(string path)
        {
            Console.WriteLine("...please wait...");
            string responsestring = "";
            using (var client = new HttpClient())
            {
                if (apiKey != "")
                {
                    client.DefaultRequestHeaders.Add("ApiKey", apiKey);
                }
                HttpResponseMessage response = await client.GetAsync("http://localhost:24702" + path);
                responsestring = await response.Content.ReadAsStringAsync();
                return responsestring;
            }
        }

        static async Task<string> DeleteStringAsync()
        {
            Console.WriteLine("...please wait...");
            string responsestring = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ApiKey", apiKey);
                HttpResponseMessage response = await client.DeleteAsync("http://localhost:24702/api/user/removeuser?username=" + userName);
                responsestring = await response.Content.ReadAsStringAsync();
                return responsestring;
            }
        }

        static async Task<string> PostStringAsync(string path, string jsonInput)
        {
            Console.WriteLine("...please wait...");
            string responsestring = "";
            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (var client = new HttpClient())
            {
                if (apiKey != "")
                {
                    client.DefaultRequestHeaders.Add("ApiKey", apiKey);
                }
                HttpResponseMessage response = await client.PostAsync("http://localhost:24702" + path, content);
                responsestring = await response.Content.ReadAsStringAsync();
                return responsestring;
            }
        }

    }
    #endregion
}
