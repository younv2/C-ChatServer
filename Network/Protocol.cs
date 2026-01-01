namespace Network
{
    public enum ResultCode : byte
    {
        Success,
        Fail
    }
    public enum ChatProtocol : byte
    {
        SetNickname,
        Message
    }
    public enum PacketResult
    {
        Success,       // 패킷 하나를 성공적으로 처리함 (다음 패킷을 또 읽어봐야 함)
        Pending,       // 데이터가 아직 다 오지 않음 (기다려야 함, 루프 탈출)
        Error          // 연결이 끊겼거나 패킷이 손상됨 (클라이언트 제거 대상)
    }
}
