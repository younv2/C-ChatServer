using System;
using System.Net.Sockets;

namespace Network
{
    public static class NetworkSystem
    {
        /// <summary>
        /// 데이터 보내기
        /// </summary>
        /// <param name="_socket"></param>
        /// <param name="_bytes"></param>
        /// <param name="_socketFlags"></param>
        public static void Send(Socket _socket, byte[] _bytes, SocketFlags _socketFlags = SocketFlags.None)
        {
            if (_bytes == null || _bytes.Length == 0)
            {
                Console.WriteLine("보낼 데이터가 없습니다.");
                return;
            }
            _socket.Send(_bytes, _bytes.Length, _socketFlags);
        }
        /// <summary>
        /// 패킷 읽어오기(사이즈를 모를 경우)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_socket"></param>
        /// <param name="_socketFlags"></param>
        /// <returns></returns>
        public static T ReadPacket<T>(Socket _socket, SocketFlags _socketFlags = SocketFlags.None) where T : struct
        {
            ushort packetSize = GetTotalSize(_socket, _socketFlags);

            byte[] fullPacket = new byte[packetSize];
            byte[] headerBytes = BitConverter.GetBytes(packetSize);
            Array.Copy(headerBytes, 0, fullPacket, 0, 2);

            int totalRead = 2;
            while (totalRead < packetSize)
            {
                int n = _socket.Receive(fullPacket, totalRead, (packetSize - totalRead), _socketFlags);
                if (n <= 0)
                    break;
                totalRead += n;
            }

            return ByteConverter.BytesToStructure<T>(fullPacket);
        }
        /// <summary>
        /// 패킷 읽어오기(사이즈를 이미 알고 있을 경우)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_socket"></param>
        /// <param name="_packetSize"></param>
        /// <param name="_socketFlags"></param>
        /// <returns></returns>
        public static T ReadPacket<T>(Socket _socket,ushort _packetSize, SocketFlags _socketFlags = SocketFlags.None) where T : struct
        {
            byte[] fullPacket = new byte[_packetSize];

            int totalRead = 0;

            while (totalRead < _packetSize)
            {
                int n = _socket.Receive(fullPacket, totalRead, (_packetSize - totalRead), _socketFlags);
                if (n <= 0)
                    break;
                totalRead += n;
            }

            return ByteConverter.BytesToStructure<T>(fullPacket);
        }
        /// <summary>
        /// 헤더를 Peek로 읽어오기
        /// </summary>
        /// <param name="_socket"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static bool TryPeekHeader(Socket _socket, out PacketHeader header)
        {
            header = default;
            int headerSize = 3; // Size(2) + Protocol(1)
            byte[] headerBuffer = new byte[headerSize];

            // 1. 현재 소켓 버퍼에 쌓여있는 양을 확인 (읽지는 않음)
            if (_socket.Available < headerSize)
            {
                return false; // 아직 헤더조차 다 안 왔으니 다음에 다시 시도!
            }

            // 2. 헤더만큼 데이터가 있다면 안전하게 Peek
            _socket.Receive(headerBuffer, 0, headerSize, SocketFlags.Peek);
            header = ByteConverter.BytesToStructure<PacketHeader>(headerBuffer);
            return true;
        }
        /// <summary>
        /// 사이즈 얻어오기
        /// </summary>
        /// <param name="_socket"></param>
        /// <param name="_socketFlags"></param>
        /// <returns></returns>
        public static ushort GetTotalSize(Socket _socket, SocketFlags _socketFlags = SocketFlags.None)
        {
            byte[] sizeBuffer = new byte[2];
            
            int readLen = _socket.Receive(sizeBuffer,0, 2, _socketFlags);
            while(readLen < 2)
            {
                int n = _socket.Receive(sizeBuffer, readLen,2 - readLen, _socketFlags);
                if (n <= 0)
                    break;
                readLen += n;
            }

            ushort packetSize = BitConverter.ToUInt16(sizeBuffer, 0);

            return packetSize;
        }
    }
}
