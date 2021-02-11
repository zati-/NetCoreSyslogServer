
namespace SimpleTcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = TcpEchoServer.Test();
            TcpEchoClient.Test();

            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }
    }
}
