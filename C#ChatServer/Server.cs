class Server
{
    static void Main()
    {
        ChatServer chatServer = new ChatServer(9000);
        chatServer.Start();
    }
}