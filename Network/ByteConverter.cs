using Network;
using System;
using System.Runtime.InteropServices;

public class ByteConverter
{
    public static ushort GetMarshalTypeSize<T>(T _type)
    {
        return (ushort)Marshal.SizeOf(_type);
    }
    public static ushort GetMarshalTypeSize(Type _type)
    {
        return (ushort)Marshal.SizeOf(_type);
    }
    public static byte[] StructureToBytes<T>(T str) where T : struct
    {
        // 1. 구조체가 마샬링되었을 때 차지할 실제 바이트 크기
        int size = Marshal.SizeOf(str);

        // 2. 데이터를 담을 바이트 배열을 생성
        byte[] arr = new byte[size];

        // 3. 비관리 메모리(HGlobal) 영역에 구조체 크기만큼 공간을 할당
        IntPtr ptr = Marshal.AllocHGlobal(size);

        // 4. 관리 메모리의 구조체 데이터를 위에서 만든 비관리 메모리(ptr)로 복사
        Marshal.StructureToPtr(str, ptr, true);

        // 5. 비관리 메모리(ptr)에 복사된 데이터를 다시 바이트 배열(arr)로 복사
        Marshal.Copy(ptr, arr, 0, size);

        // 6. 할당했던 비관리 메모리를 해제(안 하면 메모리 누수 발생)
        Marshal.FreeHGlobal(ptr);

        return arr;
    }
    public static T BytesToStructure<T>(byte[] byteArr) where T : struct
    {
        // 1. 리턴할 구조체 변수를 초기화
        T str = default(T);

        // 2. 이 구조체가 차지해야 할 메모리 크기를 계산
        int size = Marshal.SizeOf(str);

        // 3. 비관리 메모리 영역을 구조체 크기만큼 할당
        IntPtr ptr = Marshal.AllocHGlobal(size);

        // 4. 받은 바이트 배열(byteArr)의 데이터를 비관리 메모리(ptr)로 복사
        Marshal.Copy(byteArr, 0, ptr, size);

        // 5. 비관리 메모리의 데이터를 구조체 형식으로 마샬링하여 객체로 변환
        // 여기서 byte들이 다시 string, ushort, long 등의 필드로 쪼개져 들어갑니다.
        str = (T)Marshal.PtrToStructure(ptr, typeof(T));

        // 6. 사용한 비관리 메모리를 해제
        Marshal.FreeHGlobal(ptr);

        return str;
    }
}
