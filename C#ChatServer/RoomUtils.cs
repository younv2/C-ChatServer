using Network;

public class RoomManager
{
    private List<RoomData> m_RoomList;
    public int m_RoomIndexCounter = 0;
    public bool TryEnterRoom(ClientSession _joiner, int _roomIndex, string _roomPassword)
    {
        if(m_RoomList == null || m_RoomList.Count <= 0)
            return false;
        var room = m_RoomList.Find(x=>x.RoomIndex == _roomIndex);
        if (room == null)
            return false;
        if (room.RoomPassword != _roomPassword)
            return false;

        room.RoomPlayerList.Add(_joiner);
        return true;
    }
    public void CreateRoom(ClientSession _maker, string _roomName, string _roomPassword)
    {
        RoomData room = new RoomData();
        room.RoomIndex = ++m_RoomIndexCounter;
        room.RoomLeader = _maker;
        room.RoomName = _roomName;
        room.RoomPassword = _roomPassword;

        room.RoomPlayerList.Add(_maker);
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
