
using System; // Console 
using System.IO; // MemoryStream 
using System.Net.Security; // SslStrea
// using System.Security.Cryptography.X509Certificates;
using System.Text; // Encoding 
using System.Threading; // remember the thread 


namespace SimpleTlsServer
{


    // https://www.programmersought.com/article/4670276171/
    public class ExampleTlsServer 
    {
        private const int IOTimeout = -1;

        public static System.Security.Cryptography.X509Certificates.X509Certificate cert;


        public static void Test()
        {
            System.Security.Cryptography.X509Certificates.X509Store store = 
                new System.Security.Cryptography.X509Certificates.X509Store(
                    System.Security.Cryptography.X509Certificates.StoreName.Root
            );

            store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadWrite);
            
            // Retrieve the certificate 
            System.Security.Cryptography.X509Certificates.X509Certificate2Collection certs = 
                store.Certificates.Find(
                    System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectName
                    , "SSL-SERVER"
                    , false
            ); // There is no result when vaildOnly = true.

            if (certs.Count == 0)
            {
                // return;

                // cert = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                // foreach (System.Security.Cryptography.X509Certificates.X509Certificate2 certificate in store.Certificates) { cert = certificate; break; }
                
                string[] altNames = SelfSignedCertificate.SelfSigned.GetAlternativeNames(new string[0]);
                byte[] pfx = SelfSignedCertificate.SelfSigned.CreateSelfSignedCertificate(altNames, "");

                cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(pfx, "",
                      System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable
                // | System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.EphemeralKeySet // Error ! 
                );                
            }
            else 
                cert = certs[0];

            store.Close(); // Close the storage area.

            System.Net.Sockets.TcpListener server = 
                new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 900);

            Thread thread_server = new Thread(new ParameterizedThreadStart(RunServer));
            thread_server.Start(server);


            // client code 
            using (System.Net.Sockets.TcpClient tc = new System.Net.Sockets.TcpClient())
            {
                tc.Connect("127.0.0.1", 900);
                // SslStream stream = new SslStream(tc.GetStream());

                SslStream stream = new SslStream(tc.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate))
                {
                    ReadTimeout = IOTimeout,
                    WriteTimeout = IOTimeout
                };

                stream.AuthenticateAsClient("127.0.0.1");

                // This is where you read and send data
                while (true)
                {
                    string echo = Console.ReadLine();
                    byte[] buff = Encoding.UTF8.GetBytes(echo + "<EOF>");
                    stream.Write(buff, 0, buff.Length);
                    stream.Flush();
                    //tc.GetStream().Write(buff, 0, buff.Length);
                    //tc.GetStream().Flush();
                }

                // tc.Close();
            }

        }

        public static bool ValidateServerCertificate(
              object sender
            , System.Security.Cryptography.X509Certificates.X509Certificate certificate
            , System.Security.Cryptography.X509Certificates.X509Chain chain
            , SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        public static async void RunServer(object server)
        {
            System.Net.Sockets.TcpListener tcp = (System.Net.Sockets.TcpListener)server;
            tcp.Start();
            Console.WriteLine("Listening");
            while (true)
            {
                using (System.Net.Sockets.TcpClient socket = await tcp.AcceptTcpClientAsync())
                {
                    Console.WriteLine("Client connected");
                    // SslStream stream = new SslStream(socket.GetStream());
                    SslStream stream = new SslStream(socket.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate))
                    {
                        ReadTimeout = IOTimeout,
                        WriteTimeout = IOTimeout
                    };

                    await stream.AuthenticateAsServerAsync(cert);
                    while (true)
                    {
                        //NetworkStream stream= socket.GetStream();
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        MemoryStream ms = new MemoryStream();
                        int len = -1;
                        do
                        {
                            byte[] buff = new byte[1000];
                            len = await stream.ReadAsync(buff, 0, buff.Length);
                            await ms.WriteAsync(buff, 0, len);

                            string line = new string(Encoding.UTF8.GetChars(buff, 0, len));
                            if (line.EndsWith("<EOF>"))
                                break;
                        } while (len != 0);

                        //string echo=Encoding.UTF8.GetString(buff).Trim('\0');
                        string echo = Encoding.UTF8.GetString(ms.ToArray());
                        ms.Close();
                        Console.WriteLine(echo);
                        if (echo.Equals("q"))
                        {
                            break;
                        }
                    }

                    socket.Close();
                } // End Using socket 
            }
        }
    }

}
