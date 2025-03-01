using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Packaging.Signing;
using prjGroupB.DTO;
using System.Configuration;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Web;
using System.Net.Http;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Caching.Memory;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniLogisticController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        public UniLogisticController(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache) 
        { 
            _httpClient = httpClient;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        [HttpPost("openStoreMap")]
        public async Task<IActionResult> UniStoreSelect()
        {
            var requestUrl = "https://sandbox-api.payuni.com.tw/api/logistics/ship_map";
            var hashKey = _configuration["PayUni:HashKey"];
            var hashIV = _configuration["PayUni:HashIV"]; 

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var transactionDataDict = new Dictionary<string, string>
            {
                { "MerID", "S01804938" },
                { "Timestamp", timestamp.ToString() },
                { "MerKeyNo", $"{timestamp}{new Random().Next(1000, 9999)}" },
                { "GoodsType", "1" },
                { "LgsType", "C2C" },
                { "ShipType", "1" },
                { "MapType", "1" },
                { "MapReturnURL", "https://special-publicly-humpback.ngrok-free.app/api/UniLogistic/uniPay/store-info"},
                { "Tag", "2" }
            };

            // 轉換為 URL Encoded 字串
            var transactionDataEncoded = ConvertToUrlEncodedString(transactionDataDict);
            string urlEncodedData = HttpUtility.UrlEncode(transactionDataEncoded);
            Console.WriteLine($"transactionDataEncoded:{transactionDataEncoded}");
            Console.WriteLine($"urlEncodedData:{urlEncodedData}");

            // 加密 EncryptInfo
            var encryptInfo = EncryptAES256GCM(transactionDataEncoded, hashKey, hashIV);
            Console.WriteLine($"EncryptInfo: {encryptInfo}");


            try
            {
                byte[] decodedBytes = Convert.FromBase64String(encryptInfo);
                Console.WriteLine("EncryptInfo 是 Base64，應該轉換為 HEX！");
            }
            catch (FormatException)
            {
                Console.WriteLine("EncryptInfo 已經是 HEX！");
            }
            // 計算 HashInfo
            var hashInfo = Hash(encryptInfo, hashKey, hashIV);

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("MerID", "S01804938"),
                new KeyValuePair<string, string>("Version", "1.1"),
                new KeyValuePair<string, string>("EncryptInfo", encryptInfo),
                new KeyValuePair<string, string>("HashInfo", hashInfo)
            };

            foreach (var pair in formData)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }

            // 設定 Content-Type 為 application/x-www-form-urlencoded
            var content = new FormUrlEncodedContent(formData);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                // 發送 POST 請求
                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);
                // 顯示錯誤回應內容
                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"HTTP Error {response.StatusCode}: {errorBody}");
                    return null;
                }

                // 取得回應內容
                string responseBody = await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { Status = "ERROR", Message = responseBody });
                }
                // 解析回傳的 HTML `<form>`，改為 JSON 格式
                var match = Regex.Match(responseBody, @"action='(.*?)'.*?name='tempvar' value='(.*?)'.*?name='url' value='(.*?)'", RegexOptions.Singleline);
                if (match.Success)
                {
                    return Ok(new
                    {
                        redirectUrl = match.Groups[1].Value,
                        tempvar = match.Groups[2].Value,
                        reMapUrl = match.Groups[3].Value
                    });
                }
                return BadRequest(new { Status = "ERROR", Message = "Invalid API Response" });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return null;
            }
        }

        private string ConvertToUrlEncodedString(Dictionary<string, string> data)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kvp in data)
            {
                query[kvp.Key] = kvp.Value;
            }
            return query.ToString();
        }

        public static string EncryptAES256GCM(string transactionDataEncoded, string key, string iv)
        {
            try
            {
                var tagLength = 16;
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var ivBytes = Encoding.UTF8.GetBytes(iv);
                var plainTextBytes = Encoding.UTF8.GetBytes(transactionDataEncoded);
                var cipherText = new byte[plainTextBytes.Length+tagLength];
                Byte[] encrypted = new byte[plainTextBytes.Length];
                Byte[] tag = new Byte[tagLength];

                var cipher = new GcmBlockCipher(new AesEngine());
                var keyParameters = new AeadParameters(new KeyParameter(keyBytes), tagLength * 8, ivBytes);
                cipher.Init(true, keyParameters);
                var offset = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, cipherText, 0);
                //加密:密文+tag
                cipher.DoFinal(cipherText, offset);
                //分解密文和tag
                Array.Copy(cipherText, encrypted, plainTextBytes.Length);
                Array.Copy(cipherText, plainTextBytes.Length, tag, 0, tagLength);

                return bin2hex(Encoding.UTF8.GetBytes(Convert.ToBase64String(encrypted) + ":::" + Convert.ToBase64String(tag))).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption Error: {ex.Message}");
                return null;
            }
        }


        private string Hash(string encryptStr, string MerKey , string MerIV)
        {
            var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(MerKey + encryptStr + MerIV));
            return bin2hex(byteArray).ToUpper();
        }


        private static string bin2hex(byte[] result)
        {
            StringBuilder sb = new StringBuilder(result.Length * 2);
            for (int i = 0; i < result.Length; i++)
            {
                int hight = ((result[i] >> 4) & 0x0f);
                int low = result[i] & 0x0f;
                sb.Append(hight > 9 ? (char)((hight - 10) + 'a') : (char)(hight + '0'));
                sb.Append(low > 9 ? (char)((low - 10) + 'a') : (char)(low + '0'));
            }
            return sb.ToString();
        }

        [HttpPost("uniPay/store-info")]
        [EnableCors("AllowWebSite")]
        public async Task<IActionResult> ReceiveStoreInfo()
        {
            try
            {
                Request.EnableBuffering(); // 允許多次讀取 Body

                Console.WriteLine($"Request ContentType: {Request.ContentType}");
                Console.WriteLine($"HasFormContentType: {Request.HasFormContentType}"); // Debug: 確認 Content-Type

                string requestBody;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    Request.Body.Position = 0; // **重置 Stream 讓 Request.Form 可用**
                }
                // 檢查 Form Data
                if (Request.HasFormContentType)
                {
                    var hashKey = _configuration["PayUni:HashKey"];
                    var hashIV = _configuration["PayUni:HashIV"];
                    var formData = new Dictionary<string, string>();

                    Console.WriteLine($"FormData Count: {Request.Form.Count}");

                    foreach (var key in Request.Form.Keys)
                    {
                        formData[key] = Request.Form[key];
                        Console.WriteLine($"{key}: {Request.Form[key]}");
                    }

                    if (formData.Count == 0)
                    {
                        Console.WriteLine("收到的 FormData 為空，請檢查 PayUni API 是否正確回傳");
                    }

                    // 解密 EncryptInfo
                    if (formData.TryGetValue("EncryptInfo", out string encryptInfo))
                    {
                        string decryptedInfo = DecryptAES256GCM(encryptInfo, hashKey, hashIV);
                        Console.WriteLine($"解密後的超商資訊: {decryptedInfo}");

                        string decodedInfo = HttpUtility.UrlDecode(decryptedInfo);
                        Console.WriteLine($"解碼後的 `decryptedInfo`: {decodedInfo}");

                        // 解析 URL 查詢字串為 Dictionary
                        var queryParams = HttpUtility.ParseQueryString(decodedInfo);
                        var parsedData = queryParams.AllKeys.ToDictionary(key => key, key => queryParams[key]);

                        // 顯示所有解析結果
                        foreach (var kvp in parsedData)
                        {
                            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                        }
                        // 解析 MapJson
                        if (parsedData.TryGetValue("MapJson", out string mapJsonString))
                        {
                            var mapInfo = JsonConvert.DeserializeObject<MapStoreInfo>(mapJsonString);
                            Console.WriteLine($"StoreID: {mapInfo.storeID}");
                            Console.WriteLine($"StoreName: {mapInfo.storeName}");
                            // 儲存解析後的資訊到記憶體，供 `GET` API 查詢
                            _memoryCache.Set("SelectedStoreInfo", mapInfo, TimeSpan.FromMinutes(10));
                            Console.WriteLine($"SelectedStoreInfo:{mapInfo}");

                            // 回傳 HTML 讓視窗自動關閉
                            string htmlResponse = @"
                                    <html>
                                    <head>
                                        <script>
                                            setTimeout(function() {
                                                window.close();
                                            }, 1000);
                                        </script>
                                    </head>
                                    <body>
                                    </body>
                                    </html>";
                            return Content(htmlResponse, "text/html");
                        }
                    }
                    else
                    {
                        Console.WriteLine("未收到 EncryptInfo");
                        return BadRequest("缺少 EncryptInfo");
                    }
                }


                // 檢查 JSON Body
                if (!string.IsNullOrEmpty(requestBody))
                {
                    Console.WriteLine($"收到的 JSON Body: {requestBody}");

                    return Ok(new
                    {
                        Status = "Success",
                        ContentType = "Json",
                        Data = requestBody
                    });
                }
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "No data received"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析錯誤: {ex.Message}");
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        private string DecryptAES256GCM(string encryptInfo, string? hashKey, string? hashIV)
        {
            if (string.IsNullOrEmpty(encryptInfo))
            {
                return encryptInfo;
            }
            var encryptStrByt = Encoding.UTF8.GetString(hex2bin(encryptInfo));
            var key = Encoding.UTF8.GetBytes(hashKey);
            var iv = Encoding.UTF8.GetBytes(hashIV);
            string[] spliter = { ":::" };
            string[] data = encryptStrByt.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            Byte[] encryptData = Convert.FromBase64String(data[0]);
            Byte[] tagData = Convert.FromBase64String(data[1]);
            //組成密文:密文+tag
            Byte[] plainData = new Byte[encryptData.Length + tagData.Length];
            Array.Copy(encryptData, plainData, encryptData.Length);
            Array.Copy(tagData, 0, plainData, encryptData.Length, tagData.Length);
            var result = new Byte[encryptData.Length + tagData.Length];
            //解密設定
            var keyParameters = new AeadParameters(new KeyParameter(key), tagData.Length * 8, iv);
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(false, keyParameters);
            var offset = cipher.ProcessBytes(plainData, 0, plainData.Length, result, 0);

            cipher.DoFinal(result, offset);

            return Encoding.UTF8.GetString(result);
        }

        private byte[] hex2bin(string hexstring)
        {
            hexstring = hexstring.Replace(" ", "");
            if ((hexstring.Length % 2) != 0)
            {
                hexstring += " ";
            }
            byte[] returnBytes = new byte[hexstring.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexstring.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        //取得選擇的門市資訊(在記憶體裡)
        [HttpGet("uniPay/storeInfoCache")]
        public IActionResult GetSelectedStoreInfo()
        {
            if (_memoryCache.TryGetValue("SelectedStoreInfo", out MapStoreInfo storeInfo))
            {
                return Ok(new
                {
                  storeInfo.storeID,
                  storeInfo.storeName,
                  storeInfo.address
                });
            }

            return NotFound(new { Status = "Error", Message = "尚未選擇門市資訊" });
        }
    }
}
