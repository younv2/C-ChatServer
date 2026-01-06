using Network;
using System.Net.Sockets;

public class RoomManager
{
    private List<RoomData> m_RoomList;
    public int m_RoomIndexCounter = 0;
    public bool TryEnterRoom(ChatSession _joiner, int _roomIndex, string _roomPassword)
    {
        if(m_RoomList == null || m_RoomList.Count <= 0)
            return false;
        var room = m_RoomList.Find(x=>x.RoomIndex == _roomIndex);
        if (room == null)
            return false;
        if (room.RoomPassword != _roomPassword)
            return false;

        room.RoomPlayerList.Add(_joiner);
        _joiner.CurRoomIndex = room.RoomIndex;
        return true;
    }
    public void CreateRoom(ChatSession _maker, string _roomName, string _roomPassword)
    {
        RoomData room = new RoomData();
        room.RoomIndex = ++m_RoomIndexCounter;
        room.RoomLeader = _maker;
        room.RoomName = _roomName;
        room.RoomPassword = _roomPassword;

        room.RoomPlayerList.Add(_maker);

        _maker.CurRoomIndex = room.RoomIndex;
    }
    public RoomData GetRoomDataByIndex(int _roomIndex)
    {
        for (int i = 0; i < m_RoomList.Count; i++)
        {
            if (m_RoomList[i].RoomIndex == _roomIndex)
            {
                return m_RoomList[i];
            }
        }
        return null;
    }
    public byte[] GetRoomListToBytes()
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(m_RoomList.Count);

                foreach (var room in m_RoomList)
                {
                    bw.Write(room.RoomIndex);
                    bw.Write(room.RoomName);
                    bw.Write(room.RoomPlayerList.Count);
                }

                return ms.ToArray();
            }
        }
    }
    public void ExitRoom()
    {

    }

    private void DestroyRoom()
    {

    }
}
