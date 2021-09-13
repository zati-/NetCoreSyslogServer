
namespace SyslogServer
{


    public class Program
    {


        public static void Main(string[] args)
        {
            // TODO: Use a performant TCP/UPD/TLS libary
            // https://github.com/chronoxor/NetCoreServer
            // https://chronoxor.github.io/NetCoreServer/

            // UpdSyslogServer.Test();
            // TlsSyslogServer.Test();
            TcpSyslogServer.Test();

            // libSyslogServer.SyslogServer.StartServer();
        } // End Sub Main 


    } // End Class Program 


} // End Namespace SyslogServer 
