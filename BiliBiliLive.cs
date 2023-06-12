using System.Threading.Tasks;

namespace Liluo.BiliBiliLive
{
    public static class BiliBiliLive
    {
        /// <summary>
        /// 创建一个直播间连接
        /// </summary>
        /// <param name="roomID">房间号</param>
        /// <returns>回调结果</returns>
        public static async Task<IBiliBiliLiveRequest> Connect(int roomID)
        {
            var liveRequest = new BiliBiliLiveRequest();
            bool request = await liveRequest.Connect(roomID);
            if (request)
            {
                return liveRequest;
            }
            return null;
        }

    }
}