using System.Net;
using System.Net.Sockets;
using Network;
class Server
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        listener.Start();

        Socket listenSocket =  listener.Server;

        List<Socket> clientSockets = new List<Socket>();
        List<Socket> readList = new List<Socket>();

        byte[] recvBuf = new byte[4096];
        while (true)
        {
            readList.Clear();
            readList.Add(listenSocket);
            readList.AddRange(clientSockets);

            // select + fd_set 대응 == Socket.Select(...)
            // 무한 대기(이벤트 오면 넘어감)
            Socket.Select(readList, null, null, -1);

            // 리슨 소켓에 이벤트가 발생했을 경우
            if (readList.Contains(listenSocket))
            {
                Socket client = listenSocket.Accept(); // accept == AcceptSocket()
                clientSockets.Add(client);
            }

            for (int i = clientSockets.Count - 1; i >= 0; i--)
            {
                Socket client = clientSockets[i];

                //클라이언트 소켓에 이벤트가 발생했을 경우
                if (readList.Contains(client))
                {
                    int recvLen;
                    
                    recvLen = client.Receive(recvBuf);
                    
                    if (recvLen <= 0)
                    {
                        client.Close();
                        clientSockets.RemoveAt(i);
                        continue;
                    }
                    

                    if ((ChatProtocol)recvBuf[0] == ChatProtocol.SetNickname)
                    {
                        byte nicknameLen = recvBuf[1];
                        string nickname = System.Text.Encoding.UTF8.GetString(recvBuf, 2, nicknameLen);
                        Console.WriteLine(nickname + "님이 접속하였습니다.");

                        byte[] sendBuf = new byte[2 + 1 + nicknameLen];
                        sendBuf[0] = (byte)ChatProtocol.SetNickname;
                        sendBuf[1] = (byte)ResultCode.Success;
                        sendBuf[2] = nicknameLen;
                        Array.Copy(recvBuf, 2, sendBuf, 3, nicknameLen);

                        client.Send(sendBuf, 0, sendBuf.Length, SocketFlags.None);
                    }
                    if ((ChatProtocol)recvBuf[0] == ChatProtocol.Message)
                    {
                        byte nicknameLen = recvBuf[1];
                        string nickname = System.Text.Encoding.UTF8.GetString(recvBuf, 2, nicknameLen);
                        byte messageLen = recvBuf[2 + nicknameLen];
                        string message = System.Text.Encoding.UTF8.GetString(recvBuf, 3 + nicknameLen, messageLen);
                        Console.WriteLine($"{nickname} : {message}");

                        byte[] sendBuf = new byte[2];
                        sendBuf[0] = (byte)ChatProtocol.Message;
                        sendBuf[1] = (byte)ResultCode.Success;

                        client.Send(sendBuf, 0, sendBuf.Length, SocketFlags.None);
                    }

                    
                }
            }
            
        }

    }
}