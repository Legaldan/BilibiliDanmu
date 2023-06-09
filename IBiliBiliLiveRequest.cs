using System;
using System.Threading.Tasks;

namespace Liluo.BiliBiliLive
{
    public interface IBiliBiliLiveRequest
    {
        /// <summary>
        /// 申请异步连接
        /// </summary>
        /// <param name="roomId">房间号</param>
        /// <returns>连接结果</returns>
        Task<bool> Connect(int channelId);

        /// <summary>
        /// 断开连接
        /// </summary>
        void DisConnect();

        /// <summary>
        /// 房间人数改变时触发回调
        /// </summary>
        event Action<int> OnRoomViewer;

        /// <summary>
        /// 监听弹幕回调函数
        /// </summary>
        event Action<BiliBiliLiveDanmuData> OnDanmuCallBack;

        /// <summary>
        /// 监听礼物回调函数
        /// </summary>
        event Action<BiliBiliLiveGiftData> OnGiftCallBack;

        /// <summary>
        /// 监听上舰回调函数
        /// </summary>
        event Action<BiliBiliLiveGuardData> OnGuardCallBack;

        /// <summary>
        /// 监听SC回调函数
        /// </summary>
        event Action<BiliBiliLiveSuperChatData> OnSuperChatCallBack;

        /// <summary>
        /// 监听进入房间回调函数
        /// </summary>
        event Action<string> OnEnterRoomCallBack;

        /// <summary>
        /// 监听点赞回调函数
        /// </summary>
        event Action<string> OnLikeCallBack;

        /// <summary>
        /// 监听运行出错时回调函数
        /// </summary>
        event Action<Exception> OnErrorCallBack;

        /// <summary>
        /// 监听连接释放时回调函数
        /// </summary>
        event Action OnDisconnectCallBack;
    }
}