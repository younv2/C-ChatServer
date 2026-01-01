using System.Net;
using System.Net.Sockets;
using Network;
class Server
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        listener.Start();

        Socket listenSocket = listener.Server;

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
                    while(true)
                    {
                        if (NetworkSystem.TryPeekHeader(client, out var header))
                        {
                            if (header.Protocol == ChatProtocol.SetNickname)
                            {
                                var data = NetworkSystem.ReadPacket<ReqChatDataPacket>(client);

                                Console.WriteLine($"{data.ChatData.Nickname}닉네임을 설정했습니다.");
                                break;
                            }
                            else if (header.Protocol == ChatProtocol.Message)
                            {
                                var data = NetworkSystem.ReadPacket<ReqChatDataPacket>(client);

                                Console.WriteLine($"{data.ChatData.Nickname} : {data.ChatData.Msg}");

                                var sendData = ByteConverter.StructureToBytes(data);
                                // 클라이언트에게 메시지 뿌리기
                                foreach (var targetClient in clientSockets)
                                {
                                    NetworkSystem.Send(targetClient, sendData);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}