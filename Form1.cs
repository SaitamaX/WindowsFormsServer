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
//.Net版本4.5.2
struct CI
{
    public Socket iScoket;
    public int chatNumber;
}

namespace WindowsFormsServer
{
    public partial class Form1 : Form
    {
        const int MAXCLIENT = 10;
        Dictionary<string, CI> listClient = new Dictionary<string, CI>();
        int counter = 0;//用于客户端计数
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
                button1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
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
                    System.Console.WriteLine(point);
                    CI temp = new CI();
                    temp.iScoket = tSocket;
                    listClient.Add(point, temp);
                    Thread th = new Thread(ReceiveMsg);
                    th.IsBackground = true;
                    th.Start(tSocket);
                    richTextBox1.AppendText("客户端" + tSocket.RemoteEndPoint.ToString() + 
                        "已连接" + System.Environment.NewLine);
                    counter++;
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        void ReceiveMsg(object obj)
        {
            Socket client = obj as Socket;
            int index = counter;//该线程的index记录该客户端对应的List索引
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int n = client.Receive(buffer);
                    if(client.Poll(0, SelectMode.SelectRead))
                    {
                        int nRead = client.Receive(buffer);
                        if(nRead == 0)
                        {
                            richTextBox1.AppendText("客户端" + client.RemoteEndPoint.ToString() +
                       "已断开连接" + System.Environment.NewLine);
                            break;
                        }
                    }
                    string content = Encoding.Unicode.GetString(buffer, 0, n);
                    //System.Console.WriteLine(content);
                    string[] s = content.Split('#', '$');
                    string currAddress = client.RemoteEndPoint.ToString(); 
                    if (s[1] == "1")
                    {//该线程对应客户端已更改聊天房间
                        CI temp = new CI();
                        temp.chatNumber = int.Parse(s[3]);
                        temp.iScoket = client;
                        listClient[currAddress] = temp;
                    }
                    else if(s[1] == "0")//该客户端发送了消息
                    {
                        byte[] sendBuf = Encoding.Unicode.GetBytes(content);
                        foreach (var item in listClient)
                        {
                            if(item.Value.chatNumber == listClient[currAddress].chatNumber &&
                                item.Key != currAddress)//同一房间且非发送者自身
                            {
                               item.Value.iScoket.Send(sendBuf);
                                //Socket temp = item.Value.iScoket;
                                //temp.Send(sendBuf);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("客户端" + currAddress + "发送的消息格式有误");
                    }
                    //处理信息并转发给其余客户端的逻辑
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(SocketException))
                    {
                        richTextBox1.AppendText("客户端" + client.RemoteEndPoint.ToString() +
                       "已断开连接" + System.Environment.NewLine);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                }
            }
        }
    }
}
