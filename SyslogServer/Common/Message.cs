
namespace SyslogServer
{

    public class Message
    {
        public FacilityType Facility { get; set; }
        public SeverityType Severity { get; set; }
        public System.DateTime Datestamp { get; set; }
        public string Hostname { get; set; }
        public string Content { get; set; }
        public string RemoteIP { get; set; }
        public System.DateTime LocalDate { get; set; }
    }


}
