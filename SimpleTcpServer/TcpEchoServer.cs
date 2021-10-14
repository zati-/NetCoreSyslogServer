
namespace SimpleTcpServer
{


	// This code is adapted from a sample found at the URL 
	// "http://blogs.msdn.com/b/jmanning/archive/2004/12/19/325699.aspx"


	// https://thiscouldbebetter.wordpress.com/2015/01/13/an-echo-server-and-client-in-c-using-tcplistener-and-tcpclient/
	public class TcpEchoServer
	{


		public static async System.Threading.Tasks.Task Test()
		{
			System.Console.WriteLine("Starting echo server...");

			int port = 1234;
			System.Net.IPAddress listenerAddress = System.Net.IPAddress.Loopback;

			System.Net.Sockets.TcpListener listener = 
				new System.Net.Sockets.TcpListener(listenerAddress, port);

			listener.Start();

			System.Net.Sockets.TcpClient client = await listener.AcceptTcpClientAsync();
			System.Net.Sockets.NetworkStream stream = client.GetStream();
			System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, System.Text.Encoding.ASCII) 
			{ AutoFlush = true };

			System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.ASCII);

			while (true)
			{
				string inputLine = "";
				while (inputLine != null)
				{
					inputLine = await reader.ReadLineAsync();
					await writer.WriteLineAsync("Echoing string: " + inputLine);
					System.Console.WriteLine("Echoing string: " + inputLine);
				}

				System.Console.WriteLine("Server saw disconnect from client.");
			}
		} // End Task Test 


	} // End Class TcpEchoServer 


}

