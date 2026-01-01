using Network;
using System.Net.Sockets;

public class Client
{
    static void Main()
    {
        var client = new TcpClient("127.0.0.1", 9000);
        NetworkStream stream = client.GetStream();
        Socket socket = client.Client;

        ChatManager chatManager= new ChatManager();
        Console.Write("닉네임을 입력하세요: ");

        string nickname = Console.ReadLine() ?? "null";
        chatManager.SetNickname(nickname);
        chatManager.ReqSetNickname(socket, nickname);
        
        Thread thread = new Thread(() =>
        {
            while (true)
            {
                chatManager.ACKChatMessage(socket);
            }
        });
        thread.Start();
        while (true)
        {
            Console.Write("메시지를 입력하세요: ");
            string message = Console.ReadLine() ?? "null";
            chatManager.ReqSetMessage(socket, message);
        }
    }
}
public class ChatManager
{
    private ChatData m_ChatData;

    public ChatManager()
    {
        m_ChatData = new ChatData();
    }
    public void ReqSetNickname(Socket _socket, string _nickname)
    {
        ReqChatDataPacket packet = new ReqChatDataPacket();

        packet.Header.Size = ByteConverter.GetMarshalTypeSize(packet);
        packet.Header.Protocol = ChatProtocol.SetNickname;
        packet.ChatData.Nickname = _nickname;
        packet.TimeStamp = DateTime.Now.Ticks;

        var bytes = ByteConverter.StructureToBytes(packet);
        NetworkSystem.Send(_socket, bytes);
    }
    public void SetNickname(string _nickname)
    {
        m_ChatData.Nickname = _nickname;
    }
    public void ReqSetMessage(Socket _socket,string _message)
    {
        ReqChatDataPacket packet = new ReqChatDataPacket();

        packet.Header.Size = ByteConverter.GetMarshalTypeSize(packet);
        packet.Header.Protocol = ChatProtocol.Message;
        packet.ChatData.Nickname = m_ChatData.Nickname;
        packet.ChatData.Msg = _message;
        packet.TimeStamp = DateTime.Now.Ticks;

        var bytes = ByteConverter.StructureToBytes(packet);
        NetworkSystem.Send(_socket, bytes);
    }

    public void ACKChatMessage(Socket _socket)
    {
        if(NetworkSystem.TryPeekHeader(_socket, out PacketHeader header) == false)
        {
            return;
        }
        if(header.Protocol != ChatProtocol.Message)
        {
            return;
        }
        ACKChatMessageDataPacket _packet = NetworkSystem.ReadPacket<ACKChatMessageDataPacket>(_socket);

        Console.WriteLine($"{_packet.ChatData.Nickname} : {_packet.ChatData.Msg}");
    }

}