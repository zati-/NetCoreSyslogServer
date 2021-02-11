
using System; // System.Console ?
using System.Net.Sockets;
using System.Text; // Encoding.ASCII ? 


namespace libSyslogServer
{


    public partial class SyslogServer
    {


        public async System.Threading.Tasks.Task TestTCP()
        {
            // argument: System.Threading.CancellationToken cancellationToken;

            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, _Settings.UdpPort);
            listener.Start();
            // cancellationToken.Register(listener.Stop);

            TcpClient client = await listener.AcceptTcpClientAsync();

            //var clientTask = protocol.HandleClient(client, cancellationToken)
            // .ContinueWith(antecedent => client.Dispose())
            // .ContinueWith(antecedent => logger.LogInformation("Client disposed."));

            // https://stackoverflow.com/questions/19750624/how-to-build-a-robust-scalable-async-await-echo-server
            // https://thiscouldbebetter.wordpress.com/2015/01/13/an-echo-server-and-client-in-c-using-tcplistener-and-tcpclient/
            NetworkStream stream = client.GetStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.ASCII);

            await System.Threading.Tasks.Task.CompletedTask;

            while (true)
            {
                string inputLine = "";
                while (inputLine != null)
                {
                    inputLine = reader.ReadLine();
                    writer.WriteLine("Echoing string: " + inputLine);
                    Console.WriteLine("Echoing string: " + inputLine);
                }
                Console.WriteLine("Server saw disconnect from client.");
            }


        }

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
