
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
                    // Encoding.ASCII ? 
                    receivedData = System.Text.Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);
                    string msg = null;
                    if (_Settings.DisplayTimestamps) msg = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " ";
                    msg += receivedData;
                    System.Console.WriteLine(msg);
                    

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
                System.Console.WriteLine("***");
                System.Console.WriteLine("ReceiverThread exiting due to exception: " + e.Message);
                return;
            }
        }
    }
}
