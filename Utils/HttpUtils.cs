using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace BilibiliDanmu.Utils
{
    public class HttpUtils
    {
        public static async Task<string> SendGetRequest(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public static async Task<string> SendPostRequest(string url, Dictionary<string, string> headers, string body)
        {
            using (var client = new HttpClient())
            {
                // 添加Header
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpContent content = null;
                // 添加Body
                if (!string.IsNullOrEmpty(body))
                {
                    content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                

                // 发送请求并获取响应
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
