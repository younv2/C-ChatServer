using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

class Client
{
    static void Main()
    {
        using var client = new TcpClient("127.0.0.1", 9000);
        using NetworkStream stream = client.GetStream();

        ChatManager chatManager= new ChatManager();

        string nickname = Console.ReadLine() ?? "null";

        chatManager.Send(stream, chatManager.MakePacket("SetNickname",nickname));

        //받기 처리

        Console.WriteLine("echo: " + Encoding.UTF8.GetString(recvBuf, 0, n));
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

    public byte[] MakePacket(string protocol,params object[] objects)
    {
        byte[] packet = new byte[1];
        if (protocol == "SetNickname")
        {
            if (objects == null || objects.Length < 1)
                return packet;
            byte[] nicknameBytes = Encoding.UTF8.GetBytes(objects[0].ToString());
            if (nicknameBytes.Length > 255)
            {
                return packet;
            }
            packet = new byte[2 + nicknameBytes.Length];
            packet[0] = (byte)ChatProtocol.SetNickName;
            packet[1] = (byte)nicknameBytes.Length;
            Array.Copy(nicknameBytes, 0, packet, 2, nicknameBytes.Length);
        }

        return packet;
    }
    public void SetNicknameOnResponse(string _nickname)
    {
        // 닉네임 설정 응답 처리
        m_Chat.Nickname = _nickname;
    }
    public void Send(NetworkStream _stream, byte[] _bytes)
    {
        _stream.Write(_bytes, 0, _bytes.Length);
    }
}
public struct ChatMessage
{
    public string Nickname;
    public string Message;

}