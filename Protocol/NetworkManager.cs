using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public static class NetworkSystem
    {
        public static void Send(NetworkStream _stream, byte[] _bytes)
        {
            if (_bytes == null || _bytes.Length == 0)
            {
                Console.WriteLine("보낼 데이터가 없습니다.");
                return;
            }
            _stream.Write(_bytes, 0, _bytes.Length);
        }
        public static T ReadPacket<T>(NetworkStream _stream) where T : struct
        {
            byte[] sizeBuffer = new byte[2];
            int readLen = _stream.Read(sizeBuffer, 0, 2);
            if (readLen < 2)
                throw new Exception();

            ushort packetSize = BitConverter.ToUInt16(sizeBuffer, 0);

            byte[] fullPacket = new byte[packetSize];
            Array.Copy(sizeBuffer, 0, fullPacket, 0, 2);

            int totalRead = 2;
            while (totalRead < packetSize)
            {
                int n = _stream.Read(fullPacket, totalRead, packetSize - totalRead);
                if (n <= 0)
                    break;
                totalRead += n;
            }

            return ByteConverter.BytesToStructure<T>(fullPacket);
        }

        public static void GetSize()
        {

        }
    }
}
