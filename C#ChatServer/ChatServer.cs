using Network;
using System.Net;
using System.Net.Sockets;

public class ChatServer
{
    private readonly int m_Port;
    //TCPListener안에 Socket이 이미 포함됨(리슨소켓)
    private TcpListener m_Listener;
    private Dictionary<Socket, ChatSession> m_ClientSessions = new();
    private RoomManager m_RoomMgr;
    private bool m_IsRunning;

    public ChatServer(int _port)
    {
        m_Port = _port;
        m_Listener = new TcpListener(IPAddress.Any, m_Port);
        m_RoomMgr = new RoomManager();
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
        m_ClientSessions.Add(clientSocket,new ChatSession(clientSocket));

        // client.RemoteEndPoint == 클라이언트의 IP주소와 포트번호
        Console.WriteLine($"[접속] {clientSocket.RemoteEndPoint}");
    }
    private void HandleSessionData(ChatSession _chatSession)
    {
        while (_chatSession.Socket.Available >= 3)
        {
            var result = ProcessPacket(_chatSession);

            if (result == PacketResult.Pending)
            {
                break;
            }
            else if (result == PacketResult.Error)
            {
                RemoveClient(_chatSession);
                break;
            }
            
        }
    }
    private PacketResult ProcessPacket(ChatSession _chatSession)
    {
        try
        {
            if (!NetworkSystem.TryPeekHeader(_chatSession.Socket, out var header)) 
                return PacketResult.Pending;
            if (_chatSession.Socket.Available < header.Size) 
                return PacketResult.Pending;

            switch (header.Protocol)
            {
                case ChatProtocol.CreateRoom:
                    var createRoomPacket = NetworkSystem.ReadPacket<ReqCreateRoomPacket>(_chatSession.Socket, header.Size);
                    m_RoomMgr.CreateRoom(_chatSession,createRoomPacket.RoomName,createRoomPacket.RoomPassword);
                    Console.WriteLine($"[로그] [{DateTime.Now}]방 생성 {createRoomPacket.RoomName}");
                    break;
                case ChatProtocol.EnterRoom:
                    var enterRoomPacket = NetworkSystem.ReadPacket<ReqEnterRoomPacket>(_chatSession.Socket, header.Size);
                    m_RoomMgr.TryEnterRoom(_chatSession, enterRoomPacket.RoomIndex, enterRoomPacket.RoomPassword);
                    Console.WriteLine($"[로그] [{DateTime.Now}]방 입장 {_chatSession.Nickname}");
                    break;
                case ChatProtocol.SetNickname:
                    var nicknamePacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(_chatSession.Socket, header.Size);
                    Console.WriteLine($"[로그] [{DateTime.Now}]{nicknamePacket.ChatData.Nickname}님이 닉네임을 설정함.");
                    _chatSession.Nickname = nicknamePacket.ChatData.Nickname;
                    break;
                case ChatProtocol.GetRoomList:
                    var reqGetListPacket = NetworkSystem.ReadPacket<PacketHeader>(_chatSession.Socket, header.Size);
                    Console.WriteLine($"[로그] [{DateTime.Now}]{_chatSession.Nickname}님이 방 목록을 요청");
                    var data = m_RoomMgr.GetRoomListToBytes();
                    NetworkSystem.Send(_chatSession.Socket, data);
                    break;
                case ChatProtocol.Message:
                    var msgPacket = NetworkSystem.ReadPacket<ReqChatDataPacket>(_chatSession.Socket, header.Size);
                    Console.WriteLine($"[채팅] [{DateTime.Now}]{_chatSession.Nickname}: {msgPacket.ChatData.Msg}");
                    BroadcastToRoomUser(ByteConverter.StructureToBytes(msgPacket), _chatSession.CurRoomIndex);
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

    private void BroadcastToAll(byte[] _data)
    {
        List<Socket> disconnectedSockets = new List<Socket>();

        foreach (var pair in m_ClientSessions)
        {
            try
            {
                NetworkSystem.Send(pair.Key, _data);
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
    private void BroadcastToRoomUser(byte[] _data,int _roomIndex)
    {
        if (_roomIndex < 0)
            return;

        var curRoom = m_RoomMgr.GetRoomDataByIndex(_roomIndex);
        if (curRoom == null)
            return;
        List<Socket> disconnectedSockets = new List<Socket>();

        foreach (var player in curRoom.RoomPlayerList)
        {
            try
            {
                NetworkSystem.Send(player.Socket, _data);
            }
            catch
            {
                disconnectedSockets.Add(player.Socket);
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
