using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    class Program
    {

        static void Main()
        {
            Listen();
        }


        static void Listen()
        {
            var listener = new TcpListener(27001);

            listener.Start(5);

            var client = listener.AcceptTcpClient();

            var stream = client.GetStream();

            var bw = new BinaryWriter(stream);

            var br = new BinaryReader(stream);

            var str = br.ReadString();
                
            if (str == "send")
            {
                
                Console.WriteLine("Successful connection");
                bw.Write("Successful connection");
                while (true)
                {
                    SendScreenShot(stream);
                    Thread.Sleep(50);
                }
            }
        }

        static void SendScreenShot(NetworkStream stream)
        {
            var bw = new BinaryWriter(stream);
            var br = new BinaryReader(stream);
            var udpClient = new UdpClient();
            var connectEP = new IPEndPoint(IPAddress.Loopback, 30001);
            var bytes = ImageToByte(TakeScreenShot());
            bw.Write(bytes.Length);

            var str = br.ReadString();

            
            var skipCount = 0;
            var maxValue = ushort.MaxValue - 100;
            var bytesLen = bytes.Length;

            if (bytes.Length > maxValue)
            {
                while (skipCount + maxValue <= bytesLen)
                {
                    udpClient.Send(bytes
                        .Skip(skipCount)
                        .Take(maxValue)
                        .ToArray(), maxValue, connectEP);
                    skipCount += maxValue;
                    
                }

                if (skipCount != bytesLen)
                {
                    udpClient.Send(bytes.Skip(skipCount).Take(bytesLen - skipCount).ToArray(), bytesLen - skipCount, connectEP);
                }
            }
            else
                udpClient.Send(bytes, bytes.Length, connectEP);
            
                
        }

        static Bitmap TakeScreenShot()
        {
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);
            Size s = new Size(memoryImage.Width, memoryImage.Height);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);


            return memoryImage;
        }


        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
