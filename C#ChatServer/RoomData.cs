using Network;

public class RoomData
{
    public int RoomIndex;
    public string RoomName;
    public string RoomPassword;

    public List<ClientSession> RoomPlayerList = new();
    public ClientSession RoomLeader;
    public byte[] Serialize()
    {
        // 1. MemoryStream이라는 통을 준비함 (메모리 빌림)
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(RoomIndex);
                bw.Write(RoomName);
                bw.Write(RoomPassword);
                

                return ms.ToArray();
            }
        }
    }
    public static RoomData Deserialize(byte[] data)
    {
        RoomData room = new RoomData();

        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                // 1. 순서대로 읽기 (Serialize와 순서가 같아야 함)
                room.RoomIndex = br.ReadInt32();
                room.RoomName = br.ReadString();
                room.RoomPassword = br.ReadString();


            }
        }
        return room;
    }
}