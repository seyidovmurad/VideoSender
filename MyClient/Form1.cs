using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyClient
{
    [AddINotifyPropertyChangedInterface]
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            IpTxtb.Text = "127.0.0.1";
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            
        }

        private async Task Connect()
        {
            await Task.Run(() =>
            {
                try
                {
                    using TcpClient client = new TcpClient(IpTxtb.Text, 27001);
                    var stream = client.GetStream();

                    var bw = new BinaryWriter(stream);
                    var br = new BinaryReader(stream);

                    bw.Write("send");
                    while (true)
                    {
                        var txt = br.ReadString();
                        MessageBox.Show(txt);
                        
                        Listen(stream);

                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void Listen(NetworkStream stream)
        {
            var bw = new BinaryWriter(stream);
            var br = new BinaryReader(stream);
            
            var client = new UdpClient(30001);

            var ep = new IPEndPoint(IPAddress.Any, 0);

            List<byte> imgBytes = new List<byte>();
            byte[] bytes = null;

            while (true)
            {
                var length = br.ReadInt32();
                bw.Write("rec");
                do
                {
                    Task.Run(() =>
                    {
                        bytes = client.Receive(ref ep);
                    }).Wait();

                    imgBytes.AddRange(bytes);
                    length -= bytes.Length;

                } while (length > 0);

                var bit = ByteToImage(imgBytes.ToArray());
                PicBox.Image = bit;
                imgBytes.Clear();
            }
        }


        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

        private async void SendBtn_Click(object sender, EventArgs e)
        {
            await Connect();
        }
    }
}
