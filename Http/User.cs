using BilibiliDanmu.Models;
using BilibiliDanmu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace BilibiliDanmu.Http
{
    public class User
    {
        public async Task<LoginUrl> GetLoginUrl()
        {
            string url = Constants.GET_LOGIN_URL;
            string responseStr = await HttpUtils.SendGetRequest(url);

            LoginUrlResponse response = JsonConvert.DeserializeObject<LoginUrlResponse>(responseStr);
            if (response == null || !response.status)
            {
                throw new ApplicationException("err in GetLoginUrl!"); 
            }

            return response.data;
        }

        public async Task<LoginInfoData> GetLoginInfo(string oauthKey)
        {
            string url = Constants.GET_LOGIN_INFO + oauthKey;

            do
            {
                var headers = new Dictionary<string, string>
                {
                    { "user-agent", "Bilibili" }
                };
                string responseStr = await HttpUtils.SendPostRequest(url, headers, null);
                Debug.WriteLine(responseStr);
                if (responseStr != null)
                {
                    var jsonObj = JObject.Parse(responseStr);
                    if ((bool)jsonObj["status"])
                    {
                        //如果status是true，再获取data
                        LoginInfoResponse response = JsonConvert.DeserializeObject<LoginInfoResponse>(responseStr);
                        return response.data;
                    }
                }
                Thread.Sleep(5000);
            } while (true);
        }
    }
}
