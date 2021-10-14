
using System.Linq;


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
            // System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls13;


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

            System.Threading.Thread thread_server = new System.Threading.Thread(
                new System.Threading.ParameterizedThreadStart(RunServer)
            );
            thread_server.Start(server);


            // client code 
            using (System.Net.Sockets.TcpClient tc = new System.Net.Sockets.TcpClient())
            {

                string connectTo = "127.0.0.1";
                connectTo = "localhost";
                connectTo = "example.int";
                connectTo = System.Environment.MachineName;

                tc.Connect(connectTo, 900);

                System.Net.Security.SslStream stream = new System.Net.Security.SslStream(tc.GetStream(), false
                    , new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate)
                    )
                {
                    ReadTimeout = IOTimeout,
                    WriteTimeout = IOTimeout
                };
                
                // stream.AuthenticateAsClient(connectTo, null, true);
                stream.AuthenticateAsClient(connectTo, certs, System.Security.Authentication.SslProtocols.Tls12, true);
                // stream.AuthenticateAsClient(connectTo, certs, System.Security.Authentication.SslProtocols.Tls13, true);


                // This is where you read and send data
                while (true)
                {
                    string echo = System.Console.ReadLine();
                    byte[] buff = System.Text.Encoding.UTF8.GetBytes(echo + "<EOF>");
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
            , System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }



        private static bool NoValidateServerCertificate(
              object sender
            , System.Security.Cryptography.X509Certificates.X509Certificate certificate
            , System.Security.Cryptography.X509Certificates.X509Chain chain
            , System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // Do not allow this client to communicate with unauthenticated servers.
            bool result = false;
            if (certificate == null)
                return true;

            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate);
            string cn = cert.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false);
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
        public static System.Security.Cryptography.X509Certificates.X509Certificate My(
            object sender, 
            string targetHost,
            System.Security.Cryptography.X509Certificates.X509CertificateCollection localCertificates,
            System.Security.Cryptography.X509Certificates.X509Certificate remoteCertificate, 
            string[] acceptableIssuers)
        {
            System.Console.WriteLine(targetHost);
            return cert;
        }


        public static async void RunServer(object server)
        {
            System.Net.Sockets.TcpListener tcp = (System.Net.Sockets.TcpListener)server;
            tcp.Start();
            System.Console.WriteLine("Listening");
            while (true)
            {
                using (System.Net.Sockets.TcpClient socket = await tcp.AcceptTcpClientAsync())
                {
                    System.Console.WriteLine("Client connected");
                    // SslStream stream = new SslStream(socket.GetStream());
                    // NoValidateServerCertificate
                    // https://stackoverflow.com/questions/57399520/set-sni-in-a-client-for-a-streamsocket-or-sslstream
                    // https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml
                    // SslStream stream = new SslStream(socket.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate))
                    // SslStream stream = new SslStream(socket.GetStream(), false, new RemoteCertificateValidationCallback(NoValidateServerCertificate) ,new LocalCertificateSelectionCallback(My))


                    // ((System.Net.IPEndPoint)socket.Client.RemoteEndPoint).Address.ToString();

#if false  
                    StreamExtended.DefaultBufferPool bufferPool = new StreamExtended.DefaultBufferPool();

                    StreamExtended.Network.CustomBufferedStream yourClientStream = 
                        new StreamExtended.Network.CustomBufferedStream(socket.GetStream(), bufferPool, 4096);

                    StreamExtended.ClientHelloInfo clientSslHelloInfo = 
                        await StreamExtended.SslTools.PeekClientHello(yourClientStream, bufferPool);

                    //will be null if no client hello was received (not a SSL connection)
                    if (clientSslHelloInfo != null)
                    {
                        string sniHostName = clientSslHelloInfo.Extensions?.FirstOrDefault(x => x.Key == "server_name").Value?.Data;
                        System.Console.WriteLine(sniHostName);
                    }
                    else
                        System.Console.WriteLine("ciao");
#else
                    System.Net.Sockets.NetworkStream yourClientStream = socket.GetStream();
#endif
                    
                    System.Net.Security.SslStream stream = new System.Net.Security.SslStream(yourClientStream, false
                        , new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate))
                    {
                        ReadTimeout = IOTimeout,
                        WriteTimeout = IOTimeout
                    };

                    // System.Net.Security.SslStream stream;
                    // .NET 5.0 only stream.TargetHostName

                    // Specifying a delegate instead of directly providing the certificate works
                    System.Net.Security.SslServerAuthenticationOptions sslOptions = 
                        new System.Net.Security.SslServerAuthenticationOptions
                    {
                        // ServerCertificate = certificate,
                        ServerCertificateSelectionCallback = (sender, name) => cert ,
                        CertificateRevocationCheckMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Offline,
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                    };
                    stream.AuthenticateAsServer(sslOptions);

                    // new System.Net.Security.SslStream(null, true, null,null, )


                    // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-5.0
                    // System.Net.Security.ServerCertificateSelectionCallback
                    // System.Net.Security.SslServerAuthenticationOptions
                    System.Console.WriteLine(stream.TargetHostName);
                    // await stream.AuthenticateAsServerAsync(cert);
                    // await stream.AuthenticateAsServerAsync(cert, false, System.Security.Authentication.SslProtocols.Tls13, true);

                    // System.Console.WriteLine(stream.TargetHostName);

                    while (true)
                    {
                        //NetworkStream stream= socket.GetStream();
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        int len = -1;
                        do
                        {
                            byte[] buff = new byte[1000];
                            len = await stream.ReadAsync(buff, 0, buff.Length);
                            await ms.WriteAsync(buff, 0, len);

                            string line = new string(System.Text.Encoding.UTF8.GetChars(buff, 0, len));
                            if (line.EndsWith("<EOF>"))
                                break;
                        } while (len != 0);

                        //string echo=Encoding.UTF8.GetString(buff).Trim('\0');
                        string echo = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                        ms.Close();
                        System.Console.WriteLine(echo);
                        if (echo.Equals("q"))
                        {
                            break;
                        }
                    } // Whend 

                    socket.Close();
                } // End Using socket 

            } // Whend 

        } // End Task RunServer 


    } // End Class ExampleTlsServer 


} // End Namespace SimpleTlsServer 
