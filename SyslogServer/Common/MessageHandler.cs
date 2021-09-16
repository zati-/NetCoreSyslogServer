
namespace SyslogServer
{

    public enum ServerType
    { 
        UDP,TCP, SSL_TLS
    }

    public abstract class MessageHandler
    {
        public virtual void OnReceived(System.Net.EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            throw new System.NotImplementedException("OnReceived");
        }


        public virtual void OnError(System.Net.Sockets.SocketError error)
        {
            System.Console.WriteLine($"server caught an error with code {error}");
        }


        public static MessageHandler CreateInstance(int serverType, int port)
        {
            return new DefaultMessageHandler();
        }

    }


    public class DefaultMessageHandler 
            : MessageHandler
        {

        public int ServerPort = 0;
        public ServerType ServerType = ServerType.TCP;


        private static char[] trimChars = new char[] { ' ', '\t', '\f', '\v', '\r', '\n' };



        private static bool IsNumber(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }


        public override void OnReceived(System.Net.EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            bool octetCounting = false;
            bool isRfc5424 = false;
            bool isRfc3164 = false;
            string rawMessage = null;

            try
            {
                rawMessage = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
                System.Console.WriteLine("Incoming: " + rawMessage);

                if (string.IsNullOrWhiteSpace(rawMessage))
                    return;

                string message = rawMessage.TrimStart();


                if (IsNumber(message))
                    return; // Discard - this is just the length of a message 


                // rfc_5424_octet_counting: "218 <134>1 2021-09-16T21:44:22.395060+02:00 DESKTOP-6CN7QMR TestSerilog 31308 - [meta MessageNumber="2" AProperty="0.8263707183424247"] TCP: This is test message 00002
                // rfc_5424_nontransparent_framing: "<134>1 2021-09-16T21:44:22.395060+02:00 DESKTOP-6CN7QMR TestSerilog 31308 - [meta MessageNumber="2" AProperty="0.8263707183424247"] TCP: This is test message 00002

                // rfc_3164_octet_counting: "218 <30>Oct
                // rfc_3164_nontransparent_framing: "<30>Oct

                // 	p = ((int)facility * 8) + (int)severity;
                // ==> severity = p % 8 
                // ==> faciliy = p \ 8 

                // Probe octet-framing and message-type 
                // Let's do this WITHOUT regex - for speed ! 
                int ind = message.IndexOf('<');
                if (ind != 0)
                {
                    if (ind != -1)
                    {
                        octetCounting = true;
                        string octet = message.Substring(0, ind - 1);
                        octet = octet.TrimEnd(trimChars);
                        if (!IsNumber(octet))
                        {
                            throw new System.IO.InvalidDataException("Invalid octet framing ! \r\nMessage: " + rawMessage);
                        }

                        message = message.Substring(ind);
                    }
                    else
                        throw new System.IO.InvalidDataException(rawMessage);

                }

                int closeAngleBracketIndex = message.IndexOf('>');
                if (closeAngleBracketIndex != -1)
                {
                    closeAngleBracketIndex++;
                    string messageContent = message.Substring(closeAngleBracketIndex);
                    messageContent = messageContent.TrimStart(trimChars);
                    System.Console.WriteLine(messageContent);

                    if (messageContent.Length > 0)
                    {
                        if (char.IsDigit(messageContent[0]))
                        {
                            isRfc5424 = true;
                        }
                        else
                        {
                            isRfc3164 = true;
                        }
                    }
                    else
                        throw new System.IO.InvalidDataException(rawMessage);
                }
                else
                    throw new System.IO.InvalidDataException(rawMessage);


                System.Console.WriteLine("Octet counting: {0}", octetCounting);

                if (isRfc5424)
                {
                    System.Console.WriteLine("rfc_5424");
                    Rfc5424SyslogMessage msg5424 = Rfc5424SyslogMessage.Parse(message);
                    msg5424.SetSourceEndpoint(endpoint);
                    System.Console.WriteLine(msg5424);
                }
                else if (isRfc3164)
                {
                    System.Console.WriteLine("rfc_3164");
                    Rfc3164SyslogMessage msg3164 = Rfc3164SyslogMessage.Parse(message);
                    msg3164.RemoteIP = endpoint.ToString();
                    System.Console.WriteLine(msg3164);
                }

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);

                // bool octetCounting = false;

                if (isRfc5424)
                {
                    Rfc5424SyslogMessage msg5424 = Rfc5424SyslogMessage.Invalid(rawMessage, ex);
                }
                else if (isRfc3164)
                {
                    Rfc3164SyslogMessage msg3164 = Rfc3164SyslogMessage.Invalid(rawMessage, ex);
                }
                else
                { 
                
                }
            }

        }


    }
}

