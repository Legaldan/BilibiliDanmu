using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.Storage.Streams;

namespace BilibiliDanmu.Utils
{
    public class WebSocketUtils
    {
        // websocket header related
        internal const int headerLen = 16;
        internal const int headerLenOffset = 4;
        internal const int versionOffset = 6;
        internal const int opcodeOffset = 8;
        internal const int magicOffset = 12;
        internal const int magicNumber = 1;

        internal const string danmuMsg = "DANMU_MSG"; // 弹幕消息
        internal const string welcomeGuard = "WELCOME_GUARD"; // 欢迎xxx老爷
        internal const string entryEffect = "ENTRY_EFFECT"; // 欢迎舰长进入房间      
        internal const string welcome = "WELCOME"; // 欢迎xxx进入房间
        internal const string interactWord = "INTERACT_WORD"; // 进入房间
        internal const string sendGift = "SEND_GIFT"; // 发现送礼物

        internal enum Version : int
        {
            NormalJson = 0, // 正文为json格式的弹幕
            HeartOrCertification = 1,// 心跳或认证包正文不压缩，客户端发送的心跳包无正文，服务队发送的心跳包正文为4字节数据，表示人气值
            NormalZlib = 2, // 普通包正文使用zlib压缩
            NormalBrotli = 3, // 普通包正文使用brotli压缩，解压后为一个普通包（头部协议为0），需要再次解析出正文
        }

        internal enum Opcode : int
        {
            HeartBeat = 2, // 心跳包
            Command = 5, // 命令包
            Certification = 7, // 命令包
            EnterRoom = 8, // 进入房间
        }

        internal class CertificationPackageBody
        {
            public int roomId { get; set; }
        }

        public static async Task<ClientWebSocket> InitLiveRoom(string url, int roomId)
        {
            // 创建 WebSocket 客户端
            using (var client = new ClientWebSocket())
            {
                // 设置 WebSocket 客户端选项
                client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

                // 连接到指定的 WebSocket 服务器
                await ConnectWebSocket(client, new Uri(url));

                // 发送认证包
                CertificationPackageBody body = new CertificationPackageBody
                {
                    roomId = roomId
                };
                string bodyStr = JsonConvert.SerializeObject(body);
                await SendPacket(client, Encoding.UTF8.GetBytes(bodyStr), Opcode.Certification);

                // 每30秒发送一次心跳包
                while (client.State == WebSocketState.Open)
                {
                    await Task.Delay(30000);
                    await SendPacket(client, new byte[0], Opcode.HeartBeat);
                }

                return client;
            }
        }


        public static async Task ReceiveWebSocket(ClientWebSocket client)
        {
            byte[] buffer = new byte[10240];
            while (client.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closed by server.");
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // 解析数据包
                    ReceivedMessageHandler(buffer);
                }
            }
        }

        private static void ReceivedMessageHandler(byte[] message)
        {
            int totalLength = 0;
            for (int i = 0; i < message.Length; i += totalLength)
            {
                totalLength = BitConverter.ToInt32(message, 0 + i);
                int headerLength = BitConverter.ToInt16(message, headerLenOffset + i);
                int version = BitConverter.ToInt16(message, versionOffset + i);
                int opcode = BitConverter.ToInt32(message, opcodeOffset + i);
                int sequence = BitConverter.ToInt32(message, magicOffset + i);
                byte[] data = new byte[totalLength - headerLength];
                Array.Copy(message, headerLength + i, data, 0, data.Length);
                // 输出数据包信息
                Console.WriteLine($"Received packet: totalLength={totalLength}, headerLength={headerLength}, version={version}, opcode={opcode}, sequence={sequence}, data={BitConverter.ToString(data)}");
            }
        }


        public static async Task CloseLiveRoom(ClientWebSocket client)
        {
            // 关闭 WebSocket 连接
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }


        private static async Task ConnectWebSocket(ClientWebSocket client, Uri uri)
        {
            int retryCount = 0;
            while (client.State != WebSocketState.Open)
            {
                try
                {
                    await client.ConnectAsync(uri, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to connect to WebSocket server: {ex.Message}");
                    retryCount++;
                    if (retryCount > 3)
                    {
                        Console.WriteLine("Failed to connect to WebSocket server after 3 retries. Exiting...");
                        return;
                    }
                    await Task.Delay(5000);
                }
            }

            await ReceiveWebSocket(client);
        }

        private static async Task SendPacket(ClientWebSocket client, byte[] data, Opcode opcode)
        {
            // 计算数据包总长度
            int totalLength = headerLen + data.Length;

            // 创建数据包头部
            byte[] header = new byte[headerLen];
            BitConverter.GetBytes(totalLength).CopyTo(header, 0);
            BitConverter.GetBytes(headerLen).CopyTo(header, headerLenOffset);
            BitConverter.GetBytes((int)Version.HeartOrCertification).CopyTo(header, versionOffset); //协议版本号
            BitConverter.GetBytes(((int)opcode)).CopyTo(header, opcodeOffset);
            BitConverter.GetBytes(magicNumber).CopyTo(header, magicNumber);

            // 合并头部和数据
            byte[] packet = new byte[header.Length + data.Length];
            header.CopyTo(packet, 0);
            data.CopyTo(packet, header.Length);

            // 发送数据包
            await client.SendAsync(new ArraySegment<byte>(packet), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}
