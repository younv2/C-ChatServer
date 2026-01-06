using Network;

public class RoomData
{
    public int RoomIndex;
    public string RoomName;
    public string? RoomPassword;
    public List<ClientSession> RoomPlayerList = new();
    public ClientSession RoomLeader;
}