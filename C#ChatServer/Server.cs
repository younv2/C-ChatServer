using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
class Server
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        listener.Start();

        Socket listenSocket =  listener.Server;

        List<Socket> clientSockets = new List<Socket>();
        List<Socket> readList = new List<Socket>();

        byte[] buf = new byte[1024];
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

                    recvLen = client.Receive(buf);


                    if (recvLen <= 0)
                    {
                        client.Close();
                        clientSockets.RemoveAt(i);
                        continue;
                    }

                    client.Send(buf, 0, recvLen, SocketFlags.None);

                    client.Close();
                    clientSockets.RemoveAt(i);
                }
            }
            
        }

    }
}