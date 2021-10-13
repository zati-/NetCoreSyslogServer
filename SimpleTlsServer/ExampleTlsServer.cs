
using System; // Console 
using System.IO; // MemoryStream 
using System.Linq;
using System.Net.Security; // SslStrea
using System.Security.Cryptography.X509Certificates;
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
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            

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

                string connectTo = "127.0.0.1";
                connectTo = "localhost";
                connectTo = "example.int";
                connectTo = System.Environment.MachineName;

                tc.Connect(connectTo, 900);

                SslStream stream = new SslStream(tc.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate)
                    )
                {
                    ReadTimeout = IOTimeout,
                    WriteTimeout = IOTimeout
                };

                stream.AuthenticateAsClient(connectTo, null, true);
                // stream.AuthenticateAsClient(connectTo, certs, System.Security.Authentication.SslProtocols.Tls12, false);




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



        private static bool NoValidateServerCertificate(
            object sender
            , X509Certificate certificate
            , X509Chain chain
            , SslPolicyErrors sslPolicyErrors)
        {
            // Do not allow this client to communicate with unauthenticated servers.
            bool result = false;
            if (certificate == null)
                return true;

            X509Certificate2 cert = new X509Certificate2(certificate);
            string cn = cert.GetNameInfo(X509NameType.SimpleName, false);
            string cleanName = cn.Substring(cn.LastIndexOf('*') + 1);
            // string[] addresses = { _serverAddress, _serverSNIName };

            System.Console.WriteLine(cn);
            System.Console.WriteLine(cleanName);

            return true;
        }


        //
        // Summary:
        //     Selects the local Secure Sockets Layer (SSL) certificate used for authentication.
        //
        // Parameters:
        //   sender:
        //     An object that contains state information for this validation.
        //
        //   targetHost:
        //     The host server specified by the client.
        //
        //   localCertificates:
        //     An System.Security.Cryptography.X509Certificates.X509CertificateCollection containing
        //     local certificates.
        //
        //   remoteCertificate:
        //     The certificate used to authenticate the remote party.
        //
        //   acceptableIssuers:
        //     A System.String array of certificate issuers acceptable to the remote party.
        //
        // Returns:
        //     An System.Security.Cryptography.X509Certificates.X509Certificate used for establishing
        //     an SSL connection.
        public static X509Certificate My(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            System.Console.WriteLine(targetHost);
            return cert;
        }


        public static async void RunServer(object server)
        {
            System.Net.Sockets.TcpListener tcp = (System.Net.Sockets.TcpListener)server;
            tcp.Start();
            Console.WriteLine("Listening");
            while (true)
            {
                // var s = await tcp.AcceptSocketAsync();
                // byte[] buffer2 = new byte[4096];
                // s.Receive(buffer2);
                // System.Console.WriteLine(buffer2);


                using (System.Net.Sockets.TcpClient socket = await tcp.AcceptTcpClientAsync())
                {
                    Console.WriteLine("Client connected");
                    // SslStream stream = new SslStream(socket.GetStream());
                    // NoValidateServerCertificate
                    // SslStream stream = new SslStream(socket.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate))
                    // SslStream stream = new SslStream(socket.GetStream(), false, new RemoteCertificateValidationCallback(NoValidateServerCertificate) ,new LocalCertificateSelectionCallback(My))

                    var ps = socket.GetStream();
                    var bufferSize = 4096;
                    var bufferPool = new StreamExtended.DefaultBufferPool();
                    var yourClientStream = new StreamExtended.Network.CustomBufferedStream(ps, bufferPool, bufferSize);
                    var clientSslHelloInfo = await StreamExtended.SslTools.PeekClientHello(yourClientStream, bufferPool);

                    //will be null if no client hello was received (not a SSL connection)
                    if (clientSslHelloInfo != null)
                    {
                        string sniHostName = clientSslHelloInfo.Extensions?.FirstOrDefault(x => x.Key == "server_name").Value?.Data;
                        System.Console.WriteLine(sniHostName);
                    }
                    else
                        System.Console.WriteLine("ciao");


                    SslStream stream = new SslStream(yourClientStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate))
                    {
                        ReadTimeout = IOTimeout,
                        WriteTimeout = IOTimeout
                    };

                    

                    // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-5.0
                    // System.Net.Security.ServerCertificateSelectionCallback
                    // System.Net.Security.SslServerAuthenticationOptions


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
