using System;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using ZXing.Common;
using ZXing;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;

namespace BilibiliDanmu.Utils
{
    public class QRCodeUtils
    {

        public static async Task<BitmapImage> GenerateQRCode(string url, int width, int height, string fileName)
        {
            // 创建BarcodeWriter对象
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE, // 设置编码方式为QR码
                Options = new EncodingOptions
                {
                    Height = height, // 设置二维码高度
                    Width = width, // 设置二维码宽度
                    Margin = 0 // 设置二维码边距
                }
            };

            // 调用Write方法生成二维码
            var result = writer.Write(url);

            // 将生成的二维码转换为字节数组
            var pixelBuffer = result.PixelBuffer;
            var bytes = new byte[pixelBuffer.Length];
            using (var stream = pixelBuffer.AsStream())
            {
                await stream.ReadAsync(bytes, 0, bytes.Length);
            }

            // 使用BitmapEncoder将二维码保存为PNG格式的图片
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, await file.OpenAsync(FileAccessMode.ReadWrite));
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)result.PixelWidth, (uint)result.PixelHeight, 96, 96, bytes);
            await encoder.FlushAsync();

            // 将BitmapImage对象赋值给Image控件的Source属性
            var bitmap = new BitmapImage();
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                await bitmap.SetSourceAsync(stream);
            }

            return bitmap;
        }
    }

}
