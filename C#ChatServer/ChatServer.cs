using Network;
using System.Net;
using System.Net.Sockets;
public struct ChatRoom
{
    public int RoomIndex;
    public string RoomName;
    public string RoomPassword;
    public List<string> RoomPlayerList;
    public string RoomLeader;
    public byte[] Serialize()
    {
        // 1. MemoryStream이라는 통을 준비함 (메모리 빌림)
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(RoomIndex);
                bw.Write(RoomName);
                bw.Write(RoomPassword);
                foreach (var data in RoomPlayerList)
                    bw.Write(data);
                bw.Write(RoomLeader);

                return ms.ToArray();
            }
        } 
    }
    public static ChatRoom Deserialize(byte[] data)
    {
        ChatRoom room = new ChatRoom();
        room.RoomPlayerList = new List<string>();

        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                // 1. 순서대로 읽기 (Serialize와 순서가 같아야 함)
                room.RoomIndex = br.ReadInt32();
                room.RoomName = br.ReadString();
                room.RoomPassword = br.ReadString();

                // 만약 플레이어 리스트 정보도 포함되어 있다면 루프를 돌며 읽음
                // (서버에서 생성 요청을 받을 때는 보통 리스트가 비어있겠죠?)
            }
        }
        return room;
    }
}
internal class ChatServer
{
    private readonly int m_Port;
    //TCPListener안에 Socket이 이미 포함됨(리슨소켓)
    private TcpListener m_Listener;
    private List<Socket> m_ClientSockets = new();
    private List<ChatRoom> chatRoomList = new();
    private bool m_IsRunning;

    public ChatServer(int _port)
    {
        m_Port = _port;
        m_Listener = new TcpListener(IPAddress.Any, m_Port);
    }

    public void Start()
    {
        m_Listener.Start();

        m_IsRunning = true;
        Console.WriteLine($"채팅 서버가 {m_Port} 포트에서 시작되었습니다.");

        Run();
    }
    private void Run()
    {
        List<Socket> readList = new List<Socket>();

        while (m_IsRunning)
        {
            readList.Clear();
            
            readList.Add(m_Listener.Server);
            readList.AddRange(m_ClientSockets);
            Socket.Select(readList, null, null, 1000);

            foreach (var sock in readList)
            {
                if (sock == m_Listener.Server)
                {
                    AcceptNewClient();
                }
                else
                {
                    HandleClientData(sock);
                }
            }
        }
    }
    private void AcceptNewClient()
    {
        Socket client = m_Listener.AcceptSocket();
        m_ClientSockets.Add(client);
        // client.RemoteEndPoint == 클라이언트의 IP주소와 포트번호
        Console.WriteLine($"[접속] {client.RemoteEndPoint}");
    }
    private void HandleClientData(Socket client)
    {
        while (client.Available >= 3)
        {
            var result = ProcessPacket(client);

            if (result == PacketResult.Pending)
            {
                break;
            }
            else if (result == PacketResult.Error)
            {
                RemoveClient(client);
                break;
            }
            
        }
    }
    private PacketResult ProcessPacket(Socket client)
    {
        try
        {
            if (!NetworkSystem.TryPeekHeader(client, out var header)) 
                return PacketResult.Pending;
            if (client.Available < header.Size) 
                return PacketResult.Pending;

            switch (header.Protocol)
            {
                case ChatProtocol.SetNickname:
                    var nicknamePacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(client, header.Size);
                    Console.WriteLine($"[로그] [{DateTime.Now}]{nicknamePacket.ChatData.Nickname}님이 닉네임을 설정함.");
                    break;

                case ChatProtocol.Message:
                    var msgPacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(client, header.Size);
                    Console.WriteLine($"[채팅] [{DateTime.Now}]{msgPacket.ChatData.Nickname}: {msgPacket.ChatData.Msg}");
                    Broadcast(ByteConverter.StructureToBytes(msgPacket));
                    break;
            }
            return PacketResult.Success;
        }
        catch
        {
            Console.WriteLine($"[오류] [{DateTime.Now}]클라이언트가 비정상적으로 종료됨.");
            return PacketResult.Error;
        }
    }

    private void Broadcast(byte[] data)
    {
        for (int i = m_ClientSockets.Count - 1; i >= 0; i--)
        {
            try
            {
                NetworkSystem.Send(m_ClientSockets[i], data);
            }
            catch
            {
                RemoveClient(m_ClientSockets[i]);
            }
        }
    }

    private void RemoveClient(Socket socket)
    {
        Console.WriteLine($"[퇴장] {socket.RemoteEndPoint}");
        m_ClientSockets.Remove(socket);
        socket.Close();
    }
}
