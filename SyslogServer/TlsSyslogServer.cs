
namespace SyslogServer
{

    public class SyslogTlsSession 
        : NetCoreServer.SslSession
    {
        protected MessageHandler m_messageHandler;

        public SyslogTlsSession(NetCoreServer.SslServer server, MessageHandler handler) 
            : base(server) 
        {
            m_messageHandler = handler;
        }

        protected override void OnConnected()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} connected!");
        } // End Sub OnConnected 


        protected override void OnHandshaked()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} handshaked!");

            // Send invite message
            // string message = "Hello from SSL Syslog! Please send a message or '!' to disconnect the client!";
            // Send(message);
        } // End Sub OnHandshaked 


        protected override void OnDisconnected()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} disconnected!");
        } // End Sub OnDisconnected 


        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // string message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            // System.Console.WriteLine("Incoming: " + message);
            this.m_messageHandler.OnReceived(this.Socket.RemoteEndPoint, buffer, offset, size);

            // Multicast message to all connected sessions
            // Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            // if (message == "!") Disconnect();

        } // End Sub OnReceived 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            // System.Console.WriteLine($"Syslog SSL session caught an error with code {error}");
            this.m_messageHandler.OnError(error);
        } // End Sub OnError 


    } // End Class SyslogTlsSession 


    public class TlsSyslogServer 
        : NetCoreServer.SslServer
    {

        protected MessageHandler m_messageHandler;


        public TlsSyslogServer(
              NetCoreServer.SslContext context
            , System.Net.IPAddress address
            , int port
            , MessageHandler handler
        ) 
            : base(context, address, port) 
        {
            this.m_messageHandler = handler;
        }

        protected override NetCoreServer.SslSession CreateSession() 
        { 
            return new SyslogTlsSession(this, this.m_messageHandler);
        } // End Function CreateSession 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            System.Console.WriteLine($"Syslog SSL server caught an error with code {error}");
        }

        public static bool AllowAnything(
              object sender
            , System.Security.Cryptography.X509Certificates.X509Certificate certificate
            , System.Security.Cryptography.X509Certificates.X509Chain chain
            , System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        } // End Function AllowAnything 


        public static void Test()
        {            
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                new System.Net.Security.RemoteCertificateValidationCallback(AllowAnything);

            // SSL server port
            int port = 6514;

            System.Console.WriteLine($"SSL server port: {port}");

            System.Console.WriteLine();


            string[] altNames = SelfSignedCertificate.SelfSigned.GetAlternativeNames(new string[0]);
            byte[] pfx = SelfSignedCertificate.SelfSigned.CreateSelfSignedCertificate(altNames, "");

            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = 
                new System.Security.Cryptography.X509Certificates.X509Certificate2(pfx,"",
                  System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable
            // https://github.com/dotnet/runtime/issues/23749
            // | System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.EphemeralKeySet // Error ! 
            );

            // Create and prepare a new SSL server context
            NetCoreServer.SslContext context = new NetCoreServer.SslContext(
                // System.Security.Authentication.SslProtocols.Tls
                // System.Security.Authentication.SslProtocols.Tls13
                  System.Security.Authentication.SslProtocols.Tls12
                , cert
            );

            // Create a new SSL Syslog server
            TlsSyslogServer server = 
                new TlsSyslogServer(context, System.Net.IPAddress.Any, port,  MessageHandler.CreateInstance(123, port));

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


    } // End Class TlsSyslogServer 


} // End Namespace SyslogServer 
