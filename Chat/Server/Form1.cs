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


namespace Server
{
    public partial class Server : Form
    {
        public static string data = null;
        private TcpListener tcpListener;
        private Thread listenThread;

        delegate void SetTextCallback(string text);


        public Server()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            #region OLD
            // Data buffer für incoming data.
            //byte[] bytes = new Byte[1024];

            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 
            //                                  Convert.ToInt32(PortTextBox.Text));

            //writeToLog(localEndPoint.ToString());

            //Socket listener = new Socket(
            //    AddressFamily.InterNetwork,
            //    SocketType.Stream, ProtocolType.Tcp);

            //listener.Bind(localEndPoint);
            //listener.Listen(10);

            //// Endlossschleife die auf eine Verbindung wartet. 
            //while (true)
            //{
            //    Console.WriteLine("Waiting for a connection...");
            //    Socket handler = listener.Accept();
            //    data = null;

            //    while (true)
            //    {
            //        bytes = new byte[1024];
            //        int bytesRec = handler.Receive(bytes);
            //        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            //        if (data.IndexOf("<EOF>") > -1)
            //        {  // Wenn ein <EOF> kommt ist die msg zuende
            //            break;
            //        }
            //    }
            //    // Textausgabe in der Konsole.
            //    Console.WriteLine("Text received : {0}", data);
            //    // text zurücksenden.
            //    byte[] msg = Encoding.ASCII.GetBytes("SAErver->"+data);

            //    handler.Send(msg);
            //    handler.Shutdown(SocketShutdown.Both);
            //    handler.Close();
            //}
            #endregion

            this.tcpListener = new TcpListener(IPAddress.Any, 
                                                 Convert.ToInt32(PortTextBox.Text));
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            Console.WriteLine("Listen");
            SetText("Listen");

            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                SetText("Client Connected:" + client.Client.ToString());

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {//blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {//a socket error has occured
                    break;
                }
                if (bytesRead == 0)
                {//the client has disconnected from the server
                    break;
                }
                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();

                String clientMessage = "" + encoder.GetString(message, 0, bytesRead);
                SetText(clientMessage);
                byte[] buffer = encoder.GetBytes("SAERVER:" + clientMessage);
                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();
            }
            tcpClient.Close();
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


        public void writeToLog(string text)
        {
            txtLog.Text += text + Environment.NewLine;
            txtLog.SelectionStart = txtLog.TextLength - 1;
            txtLog.ScrollToCaret();
        }

    }
}
