using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Data.Xml.Dom;
using BilibiliDanmu.Models;
using System.Threading.Tasks;
using BilibiliDanmu.Http;
using System.Diagnostics;
using Microsoft.Gaming.XboxGameBar;
using Windows.UI;
using Windows.UI.Text;
using System.Threading;
using System.Net.WebSockets;
using Liluo.BiliBiliLive;


namespace BilibiliDanmu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private User bilibliClient;

        private Models.LoginUrl loginUrl;

        private ConfigModel config;

        private XboxGameBarWidget widget;

        // 创建一个队列，用于存储要显示的弹幕文本
        private Queue<string> danmuQueue;

        IBiliBiliLiveRequest req;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task<ConfigModel> InitConfigAsync()
        {
            // 读取配置文件
            StorageFile configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///config.xml"));
            string configContent = await FileIO.ReadTextAsync(configFile);

            // 解析XML内容
            XmlDocument configXml = new XmlDocument();
            configXml.LoadXml(configContent);

            // 获取配置项
            Models.ConfigModel config = new ConfigModel
            {
                RoomId = configXml.SelectSingleNode("/config/room_id").InnerText,
                WsServerUrl = configXml.SelectSingleNode("/config/ws_server_url").InnerText,
                QrCodePath = configXml.SelectSingleNode("/config/qr_code_path").InnerText
            };

            return config;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
             base.OnNavigatedTo(e);
            // you will need access to the XboxGameBarWidget, in this case it was passed as a parameter when navigating to the widget page, your implementation may differ.
            widget = e.Parameter as XboxGameBarWidget;

            if (bilibliClient == null)
            {
                bilibliClient = new User();
            }

            if (danmuQueue == null)
            {
                danmuQueue = new Queue<string>();
            }

            if (widget != null)
            {
                // Hook up events for when the ui is updated.
                widget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;
            }
        
            // 读取配置文件
            config = await InitConfigAsync();
            Task<ClientWebSocket> liveRoomClient = null;
            try
            {
                // 扫描登录二维码
                //loginUrl = await bilibliClient.GetLoginUrl();
                //BitmapImage qrBitMap = await QRCodeUtils.GenerateQRCode(loginUrl.url, 100, 100, config.QrCodePath);
                //imgQRCode.Source = qrBitMap;

                // 在页面加载完成后检查用户登录情况
                //LoginInfoData loginInfo = await bilibliClient.GetLoginInfo(loginUrl.oauthKey);
                //Debug.WriteLine("login succ! loginInfo is" + loginInfo.ToString());

                // 登录成功了改变页面样式等等
                // imgQRCode.Visibility = Visibility.Collapsed;
                backgroundGrid.Opacity = 0.6;

                // 初始化读取弹幕机
                InitDanmuShower();
                //Task.Run(() => TestDamu());

                // 连接弹幕服务器
                StartLiveRoom(int.Parse(config.RoomId));

            } 
            catch (Exception ex)
            {
                Debug.WriteLine("Something err! ex is" + ex.ToString());
            }
        }

        private async void StartLiveRoom(int RoomID)
        {
            // 创建一个监听对象
            req = await BiliBiliLive.Connect(RoomID);
            req.OnDanmuCallBack += GetDanmu;
            req.OnGiftCallBack += GetGift;
            req.OnSuperChatCallBack += GetSuperChat;
            req.OnEnterRoomCallBack += GetEnterRoom;
            req.OnLikeCallBack += GetLike;
            req.OnRoomViewer += number =>
            {
                currentNum.Text = $"当前房间人数为: {number}";
                Debug.WriteLine($"当前房间人数为: {number}");
            };
        }

        private void TestDamu()
        {
            // 将文本添加到队列中
            int i = 0;
            do
            {
                danmuQueue.Enqueue("这是一条测试弹幕-" + i++);
                Thread.Sleep(1000);
            } while (true);
        }

        private async void Widget_RequestedOpacityChanged(XboxGameBarWidget sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (widget != null)
                {
                    backgroundGrid.Opacity = widget.RequestedOpacity;
                }
            });
        }

        private void InitDanmuShower()
        {
            // 定义一个定时器，每隔一段时间读取一个弹幕文本并追加到 TextBlock 控件中
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, args) =>
            {
                if (danmuQueue.Count > 0)
                {
                    // 滚动到最底部
                    string danmuText = danmuQueue.Dequeue();
                    TextBlock danmuBlock = new TextBlock
                    {
                        Text = danmuText,
                        Margin = new Thickness(5, 0, 5, 10),
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap
                    };
                    danmuStackPanel.Children.Add(danmuBlock);
                    // 滚动到最底部
                    danmuScrollViewer.UpdateLayout();
                    danmuScrollViewer.ChangeView(null, danmuScrollViewer.ScrollableHeight, null);
                }
            };

            // 启动定时器
            timer.Start();
        }

        /// <summary>
        /// 接收到礼物的回调
        /// </summary>
        public async void GetGift(BiliBiliLiveGiftData data)
        {
            danmuQueue.Enqueue($"【礼物】{data.username} 送了 [{data.giftName}] × {data.num}， 价值[{data.total_coin}]");
        }

        /// <summary>
        /// 接收到弹幕的回调
        /// </summary>
        public async void GetDanmu(BiliBiliLiveDanmuData data)
        {
            danmuQueue.Enqueue($"【弹幕】{data.username} : {data.content}");
        }

        /// <summary>
        /// 接收到SC的回调
        /// </summary>
        public async void GetSuperChat(BiliBiliLiveSuperChatData data)
        {
            danmuQueue.Enqueue($"【SC】{data.username}发送SC：{data.content}");
        }

        /// <summary>
        /// 进入房间的回调
        /// </summary>
        public async void GetEnterRoom(string username)
        {
            danmuQueue.Enqueue($"{username} 进入房间");
        }

        /// <summary>
        /// 点赞的回调
        /// </summary>
        public async void GetLike(string username)
        {
            danmuQueue.Enqueue($"【点赞】{username} 点赞了直播间");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (req != null)
            {
                req.DisConnect();
            }
        }

    }
}
