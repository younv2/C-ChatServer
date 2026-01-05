using Network;

public class RoomManager
{
    public List<RoomData> m_RoomList;
    public void EnterRoom()
    {

    }
    public void CreateRoom(ClientSession _maker, string _roomName, string _roomPassword)
    {
        RoomData room = new RoomData();

        room.RoomLeader = _maker;
        room.RoomName = _roomName;
        room.RoomPassword = _roomPassword;
        //room.RoomPlayerList.Add()
    }
    public void ExitRoom()
    {

    }

    private void DestroyRoom()
    {

    }
    public void BroadcastToRoomUser()
    {

    }
}
