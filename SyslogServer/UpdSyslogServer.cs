
namespace SyslogServer
{


    public class UpdSyslogServer 
        : NetCoreServer.UdpServer
    {

        protected MessageHandler m_messageHandler;


        public UpdSyslogServer(System.Net.IPAddress address, int port, MessageHandler handler) 
            : base(address, port) 
        {
            this.m_messageHandler = handler;
        }


        protected override void OnStarted()
        {
            // Start receive datagrams
            ReceiveAsync();
        } // End Sub OnStarted 


        protected override void OnReceived(
              System.Net.EndPoint endpoint
            , byte[] buffer
            , long offset
            , long size)
        {
            System.Console.WriteLine("Incoming: " + System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
            this.m_messageHandler.OnReceived(endpoint, buffer, offset, size);


            // Echo the message back to the sender
            // SendAsync(endpoint, buffer, 0, size);
        } // End Sub OnReceived 


        protected override void OnSent(System.Net.EndPoint endpoint, long sent)
        {
            // Continue receive datagrams
            ReceiveAsync();
        } // End Sub OnSent 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            // System.Console.WriteLine($"Echo UDP server caught an error with code {error}");
            this.m_messageHandler.OnError(error);
        } // End Sub OnError 


        public static void Test()
        {
            // UDP server port
            int port = 514;

            System.Console.WriteLine($"UDP server port: {port}");

            System.Console.WriteLine();

            // Create a new UDP echo server
            UpdSyslogServer server = 
                new UpdSyslogServer(System.Net.IPAddress.Any, port, MessageHandler.CreateInstance(123, port));

            // Start the server
            System.Console.Write("Server starting...");
            server.Start();
            System.Console.WriteLine("Done!");

            System.Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = System.Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    System.Console.Write("Server restarting...");
                    server.Restart();
                    System.Console.WriteLine("Done!");
                }

            } // Next 

            // Stop the server
            System.Console.Write("Server stopping...");
            server.Stop();
            System.Console.WriteLine("Done!");
        } // End Sub Test 


    } // End Class UpdSyslogServer 


} // End Namespace SyslogServer 
