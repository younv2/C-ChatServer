using System.Runtime.InteropServices;

namespace Network
{

    //패킹 1Byte로
    //채팅 메세지 보내는 패킷 구조체
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReqChatDataPacket
    {
        public ushort Size;
        public ChatProtocol Protocol;
        public ChatData ChatData;
        public long TimeStamp;

    }
    //닉네임 세팅시 필요..일단은?
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResChatDataPacket
    {
        public ChatProtocol Protocol;
        public ResultCode resultCode;
    }
    // 채팅 메세지를 서버로부터 응답받는 패킷 구조체
    // 채팅을 친 닉네임 필요, 메세지 필요..
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACKChatMessageDataPacket
    {
        public ushort Size;
        public ChatProtocol Protocol;
        public ChatData ChatData;
        public long TimeStamp;
    }


    public struct ChatData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string Nickname;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Msg;
    }
}
