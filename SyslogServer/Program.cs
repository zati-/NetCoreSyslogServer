
namespace SyslogServer
{

    public class GeneralSyslogServer
    {

        protected UpdSyslogServer m_udpServer;
        protected TcpSyslogServer m_tcpServer;
        protected TlsSyslogServer m_tlsServer;



        public ServerType ServerType;
        public int Port;
        public System.Net.IPAddress ListenAddress;
        public System.Security.Authentication.SslProtocols Protocol;
        public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate;


        private static int GetDefaultPort(ServerType serverType)
        {
            if (serverType == ServerType.UDP)
                return 514;

            if (serverType == ServerType.TCP)
                return 1468;

            if (serverType == ServerType.SSL_TLS)
                return 6514;

            return -1;
        }


        public GeneralSyslogServer(
            ServerType serverType,
            int port,
            System.Net.IPAddress listenIP,
            System.Security.Authentication.SslProtocols protocol,
            System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
        {
            this.ServerType = serverType;
            this.Port = port;
            this.ListenAddress = listenIP;
            this.Protocol = protocol;
            this.Certificate = certificate;
        }


        public GeneralSyslogServer(
            ServerType serverType,
            int port,
            System.Net.IPAddress listenIP
        )
            : this(serverType, port, listenIP, System.Security.Authentication.SslProtocols.None, null)
        {
        }


        public GeneralSyslogServer(
            ServerType serverType,
            System.Net.IPAddress listenIP
        )
            : this(serverType, GetDefaultPort(serverType), listenIP, System.Security.Authentication.SslProtocols.None, null)
        {
        }



        public GeneralSyslogServer(
         ServerType serverType,
         System.Net.IPAddress listenIP,
         System.Security.Authentication.SslProtocols protocol,
         System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
            : this(serverType, GetDefaultPort(serverType), listenIP, protocol, certificate)
        {
        }




        public bool Start()
        {
            // Create a new UDP echo server
            if (this.ServerType == ServerType.UDP)
                this.m_udpServer = new UpdSyslogServer(this.ListenAddress, this.Port, MessageHandler.CreateInstance(123, this.Port));

            // Create a new TCP Syslog server
            if (this.ServerType == ServerType.TCP)
                this.m_tcpServer = new TcpSyslogServer(this.ListenAddress, this.Port, MessageHandler.CreateInstance(123, this.Port));

            if (this.ServerType == ServerType.SSL_TLS)
            {
                
                if (this.Certificate != null && this.Protocol != System.Security.Authentication.SslProtocols.None)
                {
                    // Create and prepare a new SSL server context
                    NetCoreServer.SslContext context = new NetCoreServer.SslContext(this.Protocol, this.Certificate);
                    // Create a new SSL Syslog server
                    this.m_tlsServer = new TlsSyslogServer(context, this.ListenAddress, this.Port, MessageHandler.CreateInstance(123, this.Port));
                }
            }



            if (this.m_udpServer == null && this.m_tcpServer == null && this.m_tlsServer == null)
            {
                // Can't start the server
                System.Console.Write("Server NOT starting...");
                return false;
            }

            // Start the server
            System.Console.Write("Server starting...");

            if (this.m_udpServer != null)
                this.m_udpServer.Start();

            if (this.m_tcpServer != null)
                this.m_tcpServer.Start();

            if (this.m_tlsServer != null)
                this.m_tlsServer.Start();

            System.Console.WriteLine("Done!");
            return true;
        }

        public void Run()
        {
            if (!this.Start())
                return;

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

                    if (this.m_udpServer != null)
                        this.m_udpServer.Restart();

                    if (this.m_tcpServer != null)
                        this.m_tcpServer.Restart();

                    if (this.m_tlsServer != null)
                        this.m_tlsServer.Restart();

                    System.Console.WriteLine("Done!");
                    continue;
                } // End if (line == "!") 

            } // Next 

            // Stop the server
            System.Console.Write("Server stopping...");

            if (this.m_udpServer != null)
                this.m_udpServer.Stop();

            if (this.m_tcpServer != null)
                this.m_tcpServer.Stop();

            if (this.m_tlsServer != null)
                this.m_tlsServer.Stop();

            System.Console.WriteLine("Done!");
        }

    }



    public class Program
    {


        public static void Main(string[] args)
        {
            // TODO: Use a performant TCP/UPD/TLS libary
            // https://github.com/chronoxor/NetCoreServer
            // https://chronoxor.github.io/NetCoreServer/


            // var gsl = new GeneralSyslogServer(ServerType.SSL_TLS, System.Net.IPAddress.Any, System.Security.Authentication.SslProtocols.None, null);
            // var gsl = new GeneralSyslogServer(ServerType.UDP, System.Net.IPAddress.Any);
            var gsl = new GeneralSyslogServer(ServerType.TCP, System.Net.IPAddress.Any);

            gsl.Run();

            // UpdSyslogServer.Test();
            // TlsSyslogServer.Test();
            // TcpSyslogServer.Test();

            // libSyslogServer.SyslogServer.StartServer();
        } // End Sub Main 


    } // End Class Program 


} // End Namespace SyslogServer 
