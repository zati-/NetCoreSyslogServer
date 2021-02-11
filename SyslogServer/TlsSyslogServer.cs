
namespace SyslogServer
{

    class SyslogTlsSession 
        : NetCoreServer.SslSession
    {
        public SyslogTlsSession(NetCoreServer.SslServer server) 
            : base(server) 
        { }

        protected override void OnConnected()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} connected!");
        } // End Sub OnConnected 


        protected override void OnHandshaked()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} handshaked!");

            // Send invite message
            string message = "Hello from SSL Syslog! Please send a message or '!' to disconnect the client!";
            Send(message);
        } // End Sub OnHandshaked 


        protected override void OnDisconnected()
        {
            System.Console.WriteLine($"Syslog SSL session with Id {Id} disconnected!");
        } // End Sub OnDisconnected 


        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = System.Text.Encoding.UTF8
                .GetString(buffer, (int)offset, (int)size);
            System.Console.WriteLine("Incoming: " + message);

            Rfc5424SyslogMessage msg5424 = null;
            // Rfc5424SyslogMessage.IsRfc5424SyslogMessage(message);

            try
            {
                int ind = message.IndexOf(' ');

                if (ind != -1)
                {
                    // string len = message.Substring(0, ind);
                    string rest = message.Substring(ind + 1);
                    msg5424 = Rfc5424SyslogMessage.Parse(rest);
                }
                else
                {
                    msg5424 = Rfc5424SyslogMessage.Invalid(message);
                }

            } // End Try 
            catch (System.Exception ex)
            {
                msg5424 = Rfc5424SyslogMessage.Invalid(message, ex);
            } // End Catch 

            msg5424.SetSourceEndpoint(this.Socket.RemoteEndPoint);
            // System.Console.WriteLine(msg5424);

            // string foo = "<11>1 2021-02-11T19:18:09.686143+01:00 DESKTOP-L73D2V6 TestApplication 34104 - - ﻿test123";

            // Rfc3164SyslogMessage.IsRfc3164SyslogMessage(rest);
            // Rfc3164SyslogMessage msg3164 = Rfc3164SyslogMessage.Parse(message);
            // Rfc3164SyslogMessage msg3164 = Rfc3164SyslogMessage.Parse(rest);
            // System.Console.WriteLine(msg3164);


            // Multicast message to all connected sessions
            // Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                Disconnect();

        } // End Sub OnReceived 


        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            System.Console.WriteLine($"Syslog SSL session caught an error with code {error}");
        } // End Sub OnError 


    } // End Class SyslogTlsSession 


    class TlsSyslogServer 
        : NetCoreServer.SslServer
    {


        public TlsSyslogServer(
              NetCoreServer.SslContext context
            , System.Net.IPAddress address
            , int port
        ) 
            : base(context, address, port) 
        { }

        protected override NetCoreServer.SslSession CreateSession() 
        { 
            return new SyslogTlsSession(this);
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
                new TlsSyslogServer(context, System.Net.IPAddress.Any, port);

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
