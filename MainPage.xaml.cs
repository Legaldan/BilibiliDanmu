using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Data.Xml.Dom;
using BilibiliDanmu.Models;
using System.Threading.Tasks;
using BilibiliDanmu.Http;
using BilibiliDanmu.Utils;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Microsoft.Gaming.XboxGameBar;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Animation;
using System.Threading;
using Windows.UI.Core;
using ZXing.QrCode.Internal;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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

        private XboxGameBarWidgetControl widgetControl;

        // 创建一个队列，用于存储要显示的弹幕文本
        private Queue<string> danmuQueue;

        private DispatcherTimer danmuTimer;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
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

            if (widget != null)
            {
                widget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;
                widgetControl = new XboxGameBarWidgetControl(widget);
                // Hook up events for when the ui is updated.
                widget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;
            }
        
            // 读取配置文件
            config = await InitConfigAsync();
            try
            {
                //// 扫描登录二维码
                //loginUrl = await bilibliClient.GetLoginUrl();
                //BitmapImage qrBitMap = await QRCodeUtils.GenerateQRCode(loginUrl.url, 100, 100, config.QrCodePath);
                //imgQRCode.Source = qrBitMap;

                //// 在页面加载完成后检查用户登录情况
                //LoginInfoData loginInfo = await bilibliClient.GetLoginInfo(loginUrl.oauthKey);
                //Debug.WriteLine("login succ! loginInfo is" + loginInfo.ToString());

                //// 登录成功了改变页面样式等等
                imgQRCode.Visibility = Visibility.Collapsed;
                backgroundGrid.Opacity = 0.6;

                // 初始化读取弹幕机
                initDanmuShower2();

                //initLiveRoom();

                Task.Run(TestDamu);

            } 
            catch (ApplicationException ex)
            {
                //do nothing
            }
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

        //private async void initLiveRoom()
        //{
        //    IBApiClient bApiClient = new BApiClient();

        //    var startInfo = new AppStartInfo();

        //    WebSocketBLiveClient m_WebSocketBLiveClient;
        //    //获取房间信息
        //    startInfo = await bApiClient.StartInteractivePlay("12402729", "1687655979180");
        //    if (startInfo?.Code != 0)
        //    {
        //        Console.WriteLine(startInfo?.Message);
        //        return;
        //    }


        //    m_WebSocketBLiveClient = new WebSocketBLiveClient(startInfo.GetWssLink(), startInfo.GetAuthBody());
        //    m_WebSocketBLiveClient.OnDanmaku += WebSocketBLiveClientOnDanmaku;
        //    m_WebSocketBLiveClient.OnGift += WebSocketBLiveClientOnGift;
        //    m_WebSocketBLiveClient.OnGuardBuy += WebSocketBLiveClientOnGuardBuy;
        //    m_WebSocketBLiveClient.OnSuperChat += WebSocketBLiveClientOnSuperChat;
        //    //m_WebSocketBLiveClient.Connect();
        //    m_WebSocketBLiveClient.Connect(TimeSpan.FromSeconds(30));
        //}

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            bilibliClient = new User();
            danmuQueue = new Queue<string>();
        }

        private async void Widget_RequestedOpacityChanged(XboxGameBarWidget sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                backgroundGrid.Opacity = widget.RequestedOpacity;
            });
        }

        //private void WebSocketBLiveClientOnSuperChat(SuperChat superChat)
        //{
        //    StringBuilder sb = new StringBuilder("收到SC!");
        //    sb.AppendLine();
        //    sb.Append("来自用户：");
        //    sb.AppendLine(superChat.userName);
        //    sb.Append("留言内容：");
        //    sb.AppendLine(superChat.message);
        //    sb.Append("金额：");
        //    sb.Append(superChat.rmb);
        //    sb.Append("元");
        //    Logger.Log(sb.ToString());
        //}

        //private void WebSocketBLiveClientOnGuardBuy(Guard guard)
        //{
        //    StringBuilder sb = new StringBuilder("收到大航海!");
        //    sb.AppendLine();
        //    sb.Append("来自用户：");
        //    sb.AppendLine(guard.userInfo.userName);
        //    sb.Append("赠送了");
        //    sb.Append(guard.guardUnit);
        //    Logger.Log(sb.ToString());
        //}

        //private void WebSocketBLiveClientOnGift(SendGift sendGift)
        //{
        //    StringBuilder sb = new StringBuilder("收到礼物!");
        //    sb.AppendLine();
        //    sb.Append("来自用户：");
        //    sb.AppendLine(sendGift.userName);
        //    sb.Append("赠送了");
        //    sb.Append(sendGift.giftNum);
        //    sb.Append("个");
        //    sb.Append(sendGift.giftName);
        //    Logger.Log(sb.ToString());
        //}

        //private void WebSocketBLiveClientOnDanmaku(Dm dm)
        //{
        //    StringBuilder sb = new StringBuilder("收到弹幕!");
        //    sb.AppendLine();
        //    sb.Append("用户：");
        //    sb.AppendLine(dm.userName);
        //    sb.Append("弹幕内容：");
        //    sb.Append(dm.msg);
        //    Logger.Log(sb.ToString());
        //    danmuQueue.Enqueue(sb.ToString());
        //}

        private void initDanmuShower2()
        {
            // 定义一个定时器，每隔一段时间读取一个弹幕文本并追加到 TextBlock 控件中
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, args) =>
            {
                if (danmuQueue.Count > 0)
                {
                    // 滚动到最底部
                    danmuScrollViewer.ChangeView(null, danmuScrollViewer.ExtentHeight, null);
                    string danmuText = danmuQueue.Dequeue();
                    TextBlock danmuBlock = new TextBlock();
                    danmuBlock.Text = danmuText;
                    danmuBlock.Margin = new Thickness(10, 10, 0, 0);
                    danmuBlock.Foreground = new SolidColorBrush(Colors.White);
                    danmuBlock.FontSize = 14;
                    danmuBlock.TextWrapping = TextWrapping.Wrap;
                    danmuStackPanel.Children.Add(danmuBlock);
                    // 滚动到最底部
                    danmuScrollViewer.ChangeView(null, danmuScrollViewer.ExtentHeight, null);
                }
            };

            // 启动定时器
            timer.Start();
        }

        //private void initDanmuShower()
        //{
        //    // 创建一个定时器，以一定的时间间隔从队列中读取文本并将其显示为弹幕
        //    danmuTimer = new DispatcherTimer();
        //    danmuTimer.Interval = TimeSpan.FromSeconds(1);
        //    danmuTimer.Tick += DanmuTimer_Tick;
        //    danmuTimer.Start();
        //}

        //// 定时器的Tick事件中，从队列中读取文本，并将其添加到文本框中
        //private void DanmuTimer_Tick(object sender, object e)
        //{
        //    if (danmuQueue.Count > 0)
        //    {
        //        string danmuText = danmuQueue.Dequeue();
        //        TextBlock danmuBlock = new TextBlock();
        //        danmuBlock.Text = danmuText;
        //        danmuBlock.Margin = new Thickness(0, 0, 0, 0);
        //        danmuBlock.Foreground = new SolidColorBrush(Colors.White);
        //        danmuBlock.FontSize = 20;
        //        danmuBlock.FontWeight = FontWeights.Bold;
        //        danmuBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //        danmuBlock.Arrange(new Rect(new Point(widget.MaxWindowSize.Width, widget.MaxWindowSize.Height - danmuBlock.DesiredSize.Height), danmuBlock.DesiredSize));
        //        //danmuCanvas.Children.Add(danmuBlock);

        //        // 使用动画效果将文本框向上移动，以模拟弹幕的效果
        //        DoubleAnimation danmuAnimation = new DoubleAnimation();
        //        danmuAnimation.From = widget.MaxWindowSize.Width;
        //        danmuAnimation.To = -danmuBlock.ActualWidth;
        //        danmuAnimation.Duration = TimeSpan.FromSeconds(10);

        //        Storyboard.SetTarget(danmuAnimation, danmuBlock);
        //        Storyboard.SetTargetProperty(danmuAnimation, "(Canvas.Left)");

        //        Storyboard danmuStoryboard = new Storyboard();
        //        danmuStoryboard.Children.Add(danmuAnimation);
        //        danmuStoryboard.Begin();
        //    }
        //}

    }
}
