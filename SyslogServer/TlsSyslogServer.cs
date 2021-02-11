
namespace SyslogServer
{


    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using NetCoreServer;



    class SyslogTlsSession 
        : SslSession
    {
        public SyslogTlsSession(SslServer server) 
            : base(server) 
        { }

        protected override void OnConnected()
        {
            Console.WriteLine($"Syslog SSL session with Id {Id} connected!");
        }

        protected override void OnHandshaked()
        {
            Console.WriteLine($"Syslog SSL session with Id {Id} handshaked!");

            // Send invite message
            string message = "Hello from SSL Syslog! Please send a message or '!' to disconnect the client!";
            Send(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Syslog SSL session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);

            // Multicast message to all connected sessions
            Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Syslog SSL session caught an error with code {error}");
        }
    }

    class TlsSyslogServer 
        : SslServer
    {
        public TlsSyslogServer(SslContext context, IPAddress address, int port) 
            : base(context, address, port) 
        { }

        protected override SslSession CreateSession() 
        { 
            return new SyslogTlsSession(this); 
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Syslog SSL server caught an error with code {error}");
        }

        public static bool AllowAnything(object sender, X509Certificate certificate, X509Chain chain
            , System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static void Test()
        {            
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                new System.Net.Security.RemoteCertificateValidationCallback(AllowAnything);

            // SSL server port
            int port = 6514;
            
            Console.WriteLine($"SSL server port: {port}");

            Console.WriteLine();


            string[] altNames = SelfSignedCertificate.SelfSigned.GetAlternativeNames(new string[0]);
            byte[] pfx = SelfSignedCertificate.SelfSigned.CreateSelfSignedCertificate(altNames, "");
            
            X509Certificate2 cert = new X509Certificate2(pfx,"", 
                  X509KeyStorageFlags.Exportable
                // https://github.com/dotnet/runtime/issues/23749
                // | X509KeyStorageFlags.EphemeralKeySet // Error ! 
            );
            
            // Create and prepare a new SSL server context
            SslContext context = new SslContext(
                // SslProtocols.Tls
                // SslProtocols.Tls13
                SslProtocols.Tls12
                , cert
            );

            // Create a new SSL Syslog server
            TlsSyslogServer server = new TlsSyslogServer(context, IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                server.Multicast(line);
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
    }
}
