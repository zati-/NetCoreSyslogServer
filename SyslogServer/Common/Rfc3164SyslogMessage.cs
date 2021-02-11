
namespace SyslogServer
{

    public class Rfc3164SyslogMessage
    {
        public FacilityType Facility { get; set; }
        public SeverityType Severity { get; set; }
        public System.DateTime Datestamp { get; set; }
        public string Hostname { get; set; }
        public string Content { get; set; }
        public string RemoteIP { get; set; }
        public System.DateTime LocalDate { get; set; }

        private const string RegexExpression = @"^
(?<PRI>\<\d{1,3}\>)?
(?<HDR>
  (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s
  [0-3][0-9]\s
  [0-9]{2}\:[0-9]{2}\:[0-9]{2}\s
  [^ ]+?\s
)?
(?<MSG>.+)
";


        // Rfc3164SyslogMessage.IsRfc3164SyslogMessage
        public static bool IsRfc3164SyslogMessage(string syslogMessage)
        {
            System.Text.RegularExpressions.Regex _re =
                new System.Text.RegularExpressions.Regex(
                    RegexExpression
                    , System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
                    | System.Text.RegularExpressions.RegexOptions.Singleline
                    | System.Text.RegularExpressions.RegexOptions.Compiled
            );

            System.Text.RegularExpressions.Match m = _re.Match(syslogMessage);
            return m.Success;
        }


        public static Rfc3164SyslogMessage Parse(string syslogMessage)
        {
            Rfc3164SyslogMessage msg = null;

            System.Text.RegularExpressions.Regex _re =
                new System.Text.RegularExpressions.Regex(RegexExpression, System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
| System.Text.RegularExpressions.RegexOptions.Singleline
| System.Text.RegularExpressions.RegexOptions.Compiled);


            System.Text.RegularExpressions.Match m = _re.Match(syslogMessage);
            if (!m.Success)
                return msg;


            msg = new Rfc3164SyslogMessage();

            if (m.Groups["PRI"].Success)
            {
                string pri = m.Groups["PRI"].Value;
                int priority = int.Parse(pri.Substring(1, pri.Length - 2));
                msg.Facility = (FacilityType)System.Math.Floor((double)priority / 8);
                msg.Severity = (SeverityType)(priority % 8);
            }
            else
            {
                msg.Facility = FacilityType.User;
                msg.Severity = SeverityType.Notice;
            }

            if (m.Groups["HDR"].Success)
            {
                string hdr = m.Groups["HDR"].Value.TrimEnd();
                int idx = hdr.LastIndexOf(' ');
                msg.Datestamp = System.DateTime.ParseExact(hdr.Substring(0, idx), "MMM dd HH:mm:ss", null);
                msg.Hostname = hdr.Substring(idx + 1);
            }
            else
            {
                msg.Datestamp = System.DateTime.Now;

                try
                {
                    // IPHostEntry he = Dns.GetHostEntry(receiveResult.RemoteEndPoint.Address);
                    // msg.Hostname = he.HostName;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    // msg.Hostname = receiveResult.RemoteEndPoint.Address.ToString();
                }
            } // End else of if (m.Groups["HDR"].Success) 

            msg.Content = m.Groups["MSG"].Value;
            // msg.RemoteIP = receiveResult.RemoteEndPoint.Address.ToString();
            msg.LocalDate = System.DateTime.Now;

            // if (MessageReceived != null) MessageReceived(msg);

            return msg;
        } // End Function ParseMessage 


    }


}
