using System.Net.Sockets;
using System.Text;
public class Client
{
    
    static void Main()
    {
        using var client = new TcpClient("127.0.0.1", 9000);
        using NetworkStream stream = client.GetStream();

        ChatManager chatManager= new ChatManager();
        Console.Write("닉네임을 입력하세요: ");
        string nickname = Console.ReadLine() ?? "null";

        chatManager.RequestSetNickname(stream, nickname);

        chatManager.SetNicknameOnResponse(stream);
        //받기 처리
    }
}
public class ChatManager
{
    public enum ChatProtocol : byte
    {
        SetNickName,
        Message,
        Notice
    }
    private ChatMessage m_Chat;

    public ChatManager()
    {
        m_Chat = new ChatMessage();
    }

    public byte[] MakePacket(ChatProtocol _protocol, string _value)
    {
        byte[] packet = Array.Empty<byte>();
        if (_protocol == ChatProtocol.SetNickName)
        {
            
            byte[] nicknameBytes = Encoding.UTF8.GetBytes(_value);
            if (nicknameBytes.Length > 255)
               throw new Exception("닉네임의 최대 길이를 벗어났습니다.");
            
            packet = new byte[2 + nicknameBytes.Length];
            packet[0] = (byte)ChatProtocol.SetNickName;
            packet[1] = (byte)nicknameBytes.Length;
            Array.Copy(nicknameBytes, 0, packet, 2, nicknameBytes.Length);
        }

        return packet;
    }
    public void RequestSetNickname(NetworkStream _stream,string _nickname)
    {
        Send(_stream,MakePacket(ChatProtocol.SetNickName, _nickname));
    }
    public void SetNicknameOnResponse(NetworkStream _stream)
    {
        byte[] buffer = new byte[2];
        _stream.Read(buffer, 0, 0);
    }
    public void Send(NetworkStream _stream, byte[] _bytes)
    {
        if(_bytes == null ||_bytes.Length == 0)
        {
            Console.WriteLine("보낼 데이터가 없습니다.");
            return;
        }
        _stream.Write(_bytes, 0, _bytes.Length);
    }
}
public struct ChatMessage
{
    public string Nickname;
    public string Message;

}