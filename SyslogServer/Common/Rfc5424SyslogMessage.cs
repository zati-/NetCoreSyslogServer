
using System.Text.RegularExpressions;


namespace SyslogServer
{


    // https://stackoverflow.com/a/53617099/155077
    public class Rfc5424SyslogMessage
    {
        private static readonly string _SyslogMsgHeaderPattern = @"\<(?<PRIVAL>\d{1,3})\>(?<VERSION>[1-9]{0,2}) (?<TIMESTAMP>(\S|\w)+) (?<HOSTNAME>-|(\S|\w){1,255}) (?<APPNAME>-|(\S|\w){1,48}) (?<PROCID>-|(\S|\w){1,128}) (?<MSGID>-|(\S|\w){1,32})";
        private static readonly string _SyslogMsgStructuredDataPattern = @"(?<STRUCTUREDDATA>-|\[[^\[\=\x22\]\x20]{1,32}( ([^\[\=\x22\]\x20]{1,32}=\x22.+\x22))?\])";
        private static readonly string _SyslogMsgMessagePattern = @"( (?<MESSAGE>.+))?";
        private static Regex _Expression = new Regex($@"^{_SyslogMsgHeaderPattern} {_SyslogMsgStructuredDataPattern}{_SyslogMsgMessagePattern}$"
            , RegexOptions.None
            , new System.TimeSpan(0, 0, 5)
        );


        public FacilityType Facility
        {
            get
            {
                return (FacilityType)System.Math.Floor((double)this.Prival / 8);
                // return FacilityType.User; // wenn nix
            }
        }

        public SeverityType Severity
        {
            get
            {
                return (SeverityType)(this.Prival % 8);
                // return SeverityType.Notice; // wenn nix 
            }
        }


        public bool IsValid { get; private set; }

        public int Prival { get; private set; }
        public int Version { get; private set; }
        public System.DateTime TimeStamp { get; private set; }
        public string HostName { get; private set; }
        public string AppName { get; private set; }
        public string ProcId { get; private set; }
        public string MessageId { get; private set; }
        public string StructuredData { get; private set; }
        public string Message { get; private set; }
        public string RawMessage { get; private set; }
        public System.Exception Exception { get; private set; }
        public System.Exception EndpointException { get; private set; }

        public System.Guid PrimaryKey { get; private set; }
        public string SourceEndpoint { get; private set; }
        public string SourceIP { get; private set; }
        public string SourceHost { get; private set; }
        public System.DateTime MessageReceivedTime { get; private set; }


        public static Rfc5424SyslogMessage Invalid(string rawMessage, System.Exception ex)
        {
            return new Rfc5424SyslogMessage
            {
                RawMessage = rawMessage,
                IsValid = false,
                MessageReceivedTime = System.DateTime.UtcNow,
                Exception = ex
            };
        }


        
        public void SetSourceEndpoint(System.Net.EndPoint remoteEndpoint)
        {
            try
            {
                this.PrimaryKey = System.Guid.NewGuid();
                this.SourceEndpoint = remoteEndpoint.ToString();
                System.Net.IPEndPoint ipEndPoint = remoteEndpoint as System.Net.IPEndPoint;
                this.SourceIP = ipEndPoint.Address.ToString();
                this.SourceHost = System.Net.Dns.GetHostEntry(ipEndPoint.Address).HostName;
            }
            catch (System.Exception ex)
            {
                this.EndpointException = ex;
            }
        }

        public static Rfc5424SyslogMessage Invalid(string rawMessage)
        {
            return Invalid(rawMessage, null);
        }



        /// <summary>
        /// Parses a Syslog message in RFC 5424 format. 
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static Rfc5424SyslogMessage Parse(string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                throw new System.ArgumentNullException("message");
            }

            Match match = _Expression.Match(rawMessage);
            if (match.Success)
            {
                return new Rfc5424SyslogMessage
                {
                    MessageReceivedTime = System.DateTime.UtcNow,
                    Prival = System.Convert.ToInt32(match.Groups["PRIVAL"].Value),
                    Version = System.Convert.ToInt32(match.Groups["VERSION"].Value),
                    TimeStamp = System.Convert.ToDateTime(match.Groups["TIMESTAMP"].Value),
                    HostName = match.Groups["HOSTNAME"].Value,
                    AppName = match.Groups["APPNAME"].Value,
                    ProcId = match.Groups["PROCID"].Value,
                    MessageId = match.Groups["MSGID"].Value,
                    StructuredData = match.Groups["STRUCTUREDDATA"].Value,
                    Message = match.Groups["MESSAGE"].Value,
                    RawMessage = rawMessage,
                    IsValid = true 
                };
            }
            else
            {
                return Invalid(rawMessage);
            }
        }


        public override string ToString()
        {
            System.Text.StringBuilder message =
                new System.Text.StringBuilder($@"<{Prival:###}>{Version:##} {TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffK")} {HostName} {AppName} {ProcId} {MessageId} {StructuredData}");

            if (!string.IsNullOrWhiteSpace(Message))
            {
                message.Append($" {Message}");
            }

            return message.ToString();
        }
    }
}
