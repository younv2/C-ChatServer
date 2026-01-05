using Network;
using System.Net;
using System.Net.Sockets;

public class ChatServer
{
    private readonly int m_Port;
    //TCPListener안에 Socket이 이미 포함됨(리슨소켓)
    private TcpListener m_Listener;
    private List<Socket> m_ClientSockets = new();
    private List<RoomData> m_ChatRoomList = new();
    private Dictionary<Socket,ClientSession> m_ClientSessions = new();
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
            foreach (var session in m_ClientSessions.Values)
                readList.Add(session.Socket);
            Socket.Select(readList, null, null, 1000);

            foreach (var sock in readList)
            {
                if (sock == m_Listener.Server)
                {
                    AcceptNewClient();
                }
                else
                {
                    HandleSessionData(m_ClientSessions[sock]);
                }
            }
        }
    }
    private void AcceptNewClient()
    {
        Socket clientSocket = m_Listener.AcceptSocket();
        m_ClientSessions.Add(clientSocket,new ClientSession(clientSocket));

        // client.RemoteEndPoint == 클라이언트의 IP주소와 포트번호
        Console.WriteLine($"[접속] {clientSocket.RemoteEndPoint}");
    }
    private void HandleSessionData(ClientSession _clientSession)
    {
        while (_clientSession.Socket.Available >= 3)
        {
            var result = ProcessPacket(_clientSession.Socket);

            if (result == PacketResult.Pending)
            {
                break;
            }
            else if (result == PacketResult.Error)
            {
                RemoveClient(_clientSession);
                break;
            }
            
        }
    }
    private PacketResult ProcessPacket(Socket _client)
    {
        try
        {
            if (!NetworkSystem.TryPeekHeader(_client, out var header)) 
                return PacketResult.Pending;
            if (_client.Available < header.Size) 
                return PacketResult.Pending;

            switch (header.Protocol)
            {
                case ChatProtocol.SetNickname:
                    var nicknamePacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(_client, header.Size);
                    Console.WriteLine($"[로그] [{DateTime.Now}]{nicknamePacket.ChatData.Nickname}님이 닉네임을 설정함.");
                    break;

                case ChatProtocol.Message:
                    var msgPacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(_client, header.Size);
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
        List<Socket> disconnectedSockets = new List<Socket>();

        foreach (var pair in m_ClientSessions)
        {
            try
            {
                NetworkSystem.Send(pair.Key, data);
            }
            catch
            {
                disconnectedSockets.Add(pair.Key);
            }
        }

        foreach (var socket in disconnectedSockets)
        {
            if (m_ClientSessions.TryGetValue(socket, out var session))
            {
                RemoveClient(session);
            }
        }
    }

    private void RemoveClient(ClientSession _session)
    {
        Console.WriteLine($"[퇴장] {_session.Socket.RemoteEndPoint}");
        m_ClientSessions.Remove(_session.Socket);
        _session.Socket.Close();
    }
}
