using HiSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace udpTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private UdpConnection udp;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            udp = new UdpConnection();
            udp.OnReceive += Udp_OnReceive;

            //udp.Start();
            //udp.Send(new byte[] { 1,2,3});
        }

        private void Udp_OnReceive(byte[] obj)
        {
            this.Dispatcher.Invoke(new Action(()=> {
                textBlock.Text = "";
                udp.Send(new byte[] { 1, 2, 3 });
            }));
            
        }

        //服务器端Socket对象
        private static Socket serverSocket;
        //接收数据的字符数组
        private static byte[] receiveData = new byte[1024];

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //实例化服务器端Socket对象
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //服务器端的IP和端口，IPAddress.Any实际是：0.0.0.0，表示任意，基本上表示本机IP
            IPEndPoint server = new IPEndPoint(IPAddress.Any, 60000);
            //Socket对象跟服务器端的IP和端口绑定
            serverSocket.Bind(server);
            //客户端的IP和端口，端口 0 表示任意端口
            IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
            //实例化客户端 终点
            EndPoint epSender = (EndPoint)clients;
            //开始异步接收消息  接收后，epSender存储的是发送方的IP和端口
            serverSocket.BeginReceiveFrom(receiveData, 0, receiveData.Length, SocketFlags.None,
                ref epSender, new AsyncCallback(ReceiveData), epSender);
            Console.WriteLine("Listening...");
            Console.ReadLine();
        }

        private static void SendData(IAsyncResult iar)
        {
            serverSocket.EndSend(iar);
        }

        private static void ReceiveData(IAsyncResult iar)
        {
            //客户端的IP和端口，端口 0 表示任意端口
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            //实例化客户端 终点
            EndPoint epSender = (EndPoint)client;
            //结束异步接收消息  recv 表示接收到的字符数
            int recv = serverSocket.EndReceiveFrom(iar, ref epSender);
            //将接收到的数据打印出来，发送方采用什么编码方式，此处就采用什么编码方式 转换成字符串
            Console.WriteLine("Client:" + Encoding.ASCII.GetString(receiveData, 0, recv));
            //定义要发送回客户端的消息，采用ASCII编码，
            //如果要发送汉字或其他特殊符号，可以采用UTF-8            
            byte[] sendData = Encoding.ASCII.GetBytes("hello");
            //开始异步发送消息  epSender是上次接收消息时的客户端IP和端口信息
            serverSocket.BeginSendTo(sendData, 0, sendData.Length, SocketFlags.None,
                epSender, new AsyncCallback(SendData), epSender);
            //重新实例化接收数据字节数组
            receiveData = new byte[1024];
            //开始异步接收消息，此处的委托函数是这个函数本身，递归
            serverSocket.BeginReceiveFrom(receiveData, 0, receiveData.Length, SocketFlags.None,
                ref epSender, new AsyncCallback(ReceiveData), epSender);
        }

        SimpleUdpHelper sudp;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sudp = new SimpleUdpHelper();
            sudp.OnSocketReceive += Udp_OnSocketReceive;
            sudp.Start(60000);
            
        }

        private void Udp_OnSocketReceive(byte[] obj)
        {
            this.Dispatcher.Invoke(new Action(() => {
                textBlock.Text = "";
                sudp.Send(new byte[] { 1, 2, 3 });
            }));

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            sudp.Send(new byte[] { 1, 2, 3 });
        }
    }
}
