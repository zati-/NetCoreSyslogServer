
using System; // System.Console ?
using System.Text; // Encoding.ASCII ? 

namespace libSyslogServer
{


    public partial class SyslogServer
    {


        static void ReceiverThread()
        {
            if (_ListenerUdp == null) _ListenerUdp = 
                    new System.Net.Sockets.UdpClient(_Settings.UdpPort);

                    try
            {

                System.Net.IPEndPoint endpoint = 
                    new System.Net.IPEndPoint(System.Net.IPAddress.Any, _Settings.UdpPort);

                string receivedData;
                byte[] receivedBytes; 

                while (true)
                {
                    
                    receivedBytes = _ListenerUdp.Receive(ref endpoint);
                    receivedData = Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);
                    string msg = null;
                    if (_Settings.DisplayTimestamps) msg = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " ";
                    msg += receivedData;
                    Console.WriteLine(msg);
                    
                    

                    
                    lock (_WriterLock)
                    {
                        _MessageQueue.Add(msg);
                    }

                    
                } 

                
            }
            catch (System.Exception e)
            {
                _ListenerUdp.Close();
                _ListenerUdp = null;
                Console.WriteLine("***");
                Console.WriteLine("ReceiverThread exiting due to exception: " + e.Message);
                return;
            }
        }
    }
}
