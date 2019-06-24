using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Chat
{
    public partial class Form1 : Form
    {
        public Socket client;
        Thread RX;

        delegate void SetTextCallback(string text);


        public Form1()
        {
            InitializeComponent();

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            InputText.Text = ipAddress.ToString();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[1024];

            IPAddress ipAddresseTB = IPAddress.Parse(InputText.Text);
            IPEndPoint remoteEP = new IPEndPoint(ipAddresseTB, 
                                                   Convert.ToInt32(PortTextBox.Text));

            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                SetText("Connecting to Server...");
                client.Connect(remoteEP);
                SetText("Server connected:"+ client.Connected);

                RX = new Thread(new ThreadStart(Receive));
                RX.Start();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }


        private void Receive()
        {
            while(true)
            {
                try
                {
                    byte[] bytes = new Byte[1024];
                    int bytesRec = client.Receive(bytes);
                    SetText(Encoding.ASCII.GetString(bytes, 0, bytesRec));

                }
                catch( Exception ex) { SetText(ex.ToString()); }
            }
        }

        private void SetText(string text)
        {

            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                txtLog.Text += text + Environment.NewLine;
                txtLog.SelectionStart = txtLog.TextLength - 1;
                txtLog.ScrollToCaret();
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            int sended = client.Send(Encoding.ASCII.GetBytes(MsgTextBox.Text));
            SetText("sended " + sended + " bytes; MSG:" + MsgTextBox.Text);

        }
    }
}
