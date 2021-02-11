// This code is adapted from a sample found at the URL 
// "http://blogs.msdn.com/b/jmanning/archive/2004/12/19/325699.aspx"

// https://thiscouldbebetter.wordpress.com/2015/01/13/an-echo-server-and-client-in-c-using-tcplistener-and-tcpclient/
namespace SimpleTcpServer
{


	public class TcpEchoClient
	{
		public static void Test()
		{
			System.Console.WriteLine("Starting echo client...");

			int port = 1234;
			System.Net.Sockets.TcpClient client = 
				new System.Net.Sockets.TcpClient("localhost", port);

			System.Net.Sockets.NetworkStream stream = client.GetStream();

			System.IO.StreamReader reader = new System.IO.StreamReader(stream);
			System.IO.StreamWriter writer = new System.IO.StreamWriter(stream) 
			{ AutoFlush = true };

			while (true)
			{
				System.Console.Write("Enter text to send: ");
				string lineToSend = System.Console.ReadLine();
				System.Console.WriteLine("Sending to server: " + lineToSend);
				writer.WriteLine(lineToSend);
				string lineReceived = reader.ReadLine();
				System.Console.WriteLine("Received from server: " + lineReceived);
			}
		}
	}


}
