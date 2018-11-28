using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

struct CI
{
    public Dictionary<string, Socket> dic;
    public int chatNumber;
}

namespace WindowsFormsServer
{
    public partial class Form1 : Form
    {
        const int MAXCLIENT = 10;
        List<CI> listClient = new List<CI>();
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox1.Text == "")
            {
                MessageBox.Show("输入项不能为空");
                return;
            }
            try
            {
                IPAddress ip = IPAddress.Parse(textBox1.Text);
                IPEndPoint point = new IPEndPoint(ip, int.Parse(textBox2.Text));
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(point);
                socket.Listen(MAXCLIENT);
                MessageBox.Show("服务器开始监听");
                Thread thread = new Thread(AcceptInfo);
                thread.IsBackground = true;
                thread.Start(socket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        void AcceptInfo(object obj)
        {
            Socket socket = obj as Socket;
            while (true)
            {
                try
                {
                    Socket tSocket = socket.Accept();
                    string point = tSocket.RemoteEndPoint.ToString();
                    CI temp = new CI();
                    temp.dic = new Dictionary<string, Socket>();
                    temp.dic.Add(point, tSocket);
                    listClient.Add(temp);
                    Thread th = new Thread(ReceiveMsg);
                    th.IsBackground = true;
                    th.Start(tSocket);
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        void ReceiveMsg(object obj)
        {
            Socket client = obj as Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int n = client.Receive(buffer);
                    string content = Encoding.Unicode.GetString(buffer, 0, n);
                    System.Console.WriteLine(content);
                    //处理信息并转发给其余客户端的逻辑
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
        }
    }
}
