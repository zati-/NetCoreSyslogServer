
namespace SyslogServer
{


    public class SyslogTcpSession
        : NetCoreServer.TcpSession
    {
        protected MessageHandler m_messageHandler;

        public SyslogTcpSession(NetCoreServer.TcpServer server, MessageHandler handler) 
            : base(server) 
        {
            this.m_messageHandler = handler;
        }

        protected override void OnConnected()
        {
            System.Console.WriteLine($"Syslog TCP session with Id {Id} connected!");

            // Send invite message
            string message = "Hello from Syslog TCP session ! Please send a message or '!' to disconnect the client!";
            SendAsync(message);
        } // End Sub OnConnected 


        protected override void OnDisconnected()
        {
            System.Console.WriteLine($"Syslog TCP session with Id {Id} disconnected!");
        } // End Sub OnDisconnected 





        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            this.m_messageHandler.OnReceived(this.Socket.RemoteEndPoint, buffer, offset, size);

            // Multicast message to all connected sessions
            // Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            // if (message == "!")
            // Disconnect();

        } // End Sub OnReceived 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            // System.Console.WriteLine($"Syslog TCP session caught an error with code {error}");
            this.m_messageHandler.OnError(error);
        } // End Sub OnError 


    } // End Class SyslogTcpSession 


    public class TcpSyslogServer 
        : NetCoreServer.TcpServer
    {
        protected MessageHandler m_messageHandler;

        public TcpSyslogServer(
              System.Net.IPAddress address
            , int port
            ,MessageHandler handler 
        ) 
            : base(address, port) 
        {
            this.m_messageHandler = handler;
        }


        protected override NetCoreServer.TcpSession CreateSession() 
        { 
            return new SyslogTcpSession(this, this.m_messageHandler);
        } // End Function CreateSession 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            // System.Console.WriteLine($"Syslog TCP server caught an error with code {error}");
            this.m_messageHandler.OnError(error);
        } // End Sub OnError 


        public static void Test()
        {
            // TCP server port
            int port = 1468;

            System.Console.WriteLine($"TCP server port: {port}");

            System.Console.WriteLine();

            // Create a new TCP Syslog server
            TcpSyslogServer server = 
                new TcpSyslogServer(System.Net.IPAddress.Any, port, MessageHandler.CreateInstance(123, port));

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
                    continue;
                } // End if (line == "!") 

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                server.Multicast(line);
            } // Next 

            // Stop the server
            System.Console.Write("Server stopping...");
            server.Stop();
            System.Console.WriteLine("Done!");
        } // End Sub Test  


    } // End Class TcpSyslogServer 


} // End Namespace SyslogServer 
