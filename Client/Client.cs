using Network;
using System.Net.Sockets;

public class Client
{
    static void Main()
    {
        var client = new TcpClient("127.0.0.1", 9000);
        NetworkStream stream = client.GetStream();

        ChatManager chatManager= new ChatManager();
        Console.Write("닉네임을 입력하세요: ");
        string nickname = Console.ReadLine() ?? "null";
        chatManager.SetNickname(nickname);
        chatManager.ReqSetNickname(stream, nickname);
        
        while(true)
        {
            Console.Write("메시지를 입력하세요: ");
            string message = Console.ReadLine() ?? "null";
            chatManager.ReqSetMessage(stream, message);
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
    public void ReqSetNickname(NetworkStream _stream, string _nickname)
    {
        ReqChatDataPacket packet = new ReqChatDataPacket();

        packet.Size = 0;
        packet.Protocol = ChatProtocol.SetNickname;
        packet.ChatData.Nickname = _nickname;
        packet.TimeStamp = DateTime.Now.Ticks;

        var bytes = ByteConverter.StructureToBytes(packet);
        NetworkSystem.Send(_stream, bytes);
    }
    public void SetNickname(string _nickname)
    {
        m_ChatData.Nickname = _nickname;
    }
    public void ReqSetMessage(NetworkStream _stream,string _message)
    {
        ReqChatDataPacket packet = new ReqChatDataPacket();

        packet.Size = 0;
        packet.Protocol = ChatProtocol.Message;
        packet.ChatData.Nickname = m_ChatData.Nickname;
        packet.ChatData.Msg = _message;
        packet.TimeStamp = DateTime.Now.Ticks;
    }

    
}