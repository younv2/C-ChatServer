using System.Runtime.InteropServices;

namespace Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {
        public ushort Size;
        public ChatProtocol Protocol;
    }
    //패킹 1Byte로
    //채팅 메세지 보내는 패킷 구조체
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReqChatDataPacket
    {
        public PacketHeader Header;
        public ChatData ChatData;
        public long TimeStamp;
    }
    //닉네임 세팅시 필요..일단은?
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResChatDataPacket
    {
        public ChatProtocol Protocol;
        public ResultCode ResultCode;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReqCreateRoomPacket
    {
        public PacketHeader Header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string RoomName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string RoomPassword;
        public long TimeStamp;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReqEnterRoomPacket
    {
        public PacketHeader Header;
        public int RoomIndex;
        public string RoomPassword;
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
