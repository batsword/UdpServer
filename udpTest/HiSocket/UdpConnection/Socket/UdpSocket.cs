/***************************************************************
 * Description: 
 *
 * Documents: https://github.com/hiramtan/HiSocket
 * Author: hiramtan@live.com
***************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using HiFramework;

namespace HiSocket
{
    public class UdpSocket : IUdpSocket
    {
        //#region 郑宇剑添加
        ////客户端的IP和端口，端口 0 表示任意端口
        //static IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
        ////实例化客户端 终点
        //EndPoint epSender = (EndPoint)clients;
        //#endregion

        public Socket Socket { get; private set; }
        public event Action<byte[]> OnSocketReceive;
        public int BufferSize { get; }
        private byte[] buffer;
        public UdpSocket(int bufferSize = 1 << 16)
        {
            BufferSize = bufferSize;
            buffer = new byte[BufferSize];
        }

        ///// <summary>
        ///// 郑宇剑增加
        ///// </summary>
        //public void Start()
        //{
        //    IPEndPoint serverIP = new IPEndPoint(0, 60000);//z
        //    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //    Socket.Bind(serverIP);
        //    StartReceiveOnce();
        //}

        public void Connect(IPEndPoint iep)
        {
            

            AssertThat.IsNotNull(iep);
            Socket = new Socket(iep.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                Socket.BeginConnect(iep, x =>
                {
                    try
                    {
                        var socket = x.AsyncState as Socket;
                        AssertThat.IsNotNull(socket);
                        if (!Socket.Connected)
                        {
                            throw new Exception("Connect faild");
                        }
                        socket.EndConnect(x);
                        StartReceive();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }

                }, Socket);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="ip">ipv4/ipv6</param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            var iep = new IPEndPoint(IPAddress.Parse(ip), port);
            Connect(iep);
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress ip, int port)
        {
            var iep = new IPEndPoint(ip, port);
            Connect(iep);
        }

        public void Send(byte[] bytes)
        {
            try
            {
                Socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, x =>
                {
                    try
                    {
                        var socket = x.AsyncState as Socket;
                        AssertThat.IsNotNull(socket);
                        int length = socket.EndSend(x);
                        //Todo: because this is udp protocol
                        if (length != bytes.Length) { }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }

                }, Socket);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public void DisConnect()
        {
            Socket.Close();
        }

        /// <summary>
        /// 郑宇剑添加
        /// </summary>
        //private void StartReceiveOnce()
        //{
        //    try
        //    {
        //         接收数据的字符数组
        //        byte[] receiveData = new byte[1024];
        //        开始异步接收消息  接收后，epSender存储的是发送方的IP和端口
        //        Socket.BeginReceiveFrom(receiveData, 0, receiveData.Length, SocketFlags.None,
        //            ref epSender, new AsyncCallback(ReceiveEnd), epSender);
                
        //        Socket.BeginReceive(buffer, 0, BufferSicze, SocketFlags.None, ReceiveEnd, Socket);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.ToString());
        //    }
        //}


        private void StartReceive()
        {
            try
            {
                Socket.BeginReceive(buffer, 0, BufferSize, SocketFlags.None, ReceiveEnd, Socket);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        private void ReceiveEnd(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                AssertThat.IsNotNull(socket);
                int length = socket.EndReceive(ar);
                byte[] bytes = new byte[length];
                Array.Copy(buffer, 0, bytes, 0, bytes.Length);
                ReceiveEvent(bytes);
                StartReceive();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        void ReceiveEvent(byte[] bytes)
        {
            if (OnSocketReceive != null)
            {
                OnSocketReceive(bytes);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            buffer = null;
            Socket = null;
        }
    }
}