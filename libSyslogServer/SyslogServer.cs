
namespace libSyslogServer
{


    public partial class SyslogServer
    {
        public static string _SettingsContents;
        public static Settings _Settings;

        public static System.Threading.Thread _ListenerThread;
        public static System.Net.Sockets.UdpClient _ListenerUdp; 
        public static bool _ConsoleDisplay;
        
        public static System.DateTime _LastWritten;
        private static System.Collections.Generic.List<string> _MessageQueue;
        private static readonly object _WriterLock;


        static SyslogServer()
        {
            _SettingsContents = "";
            _ConsoleDisplay = true;
            _LastWritten = System.DateTime.Now;
            _WriterLock = new object();
            _MessageQueue = new System.Collections.Generic.List<string>();
        }


        public static void StartServer()
        {
            
            if (System.IO.File.Exists("syslog.json"))
            {
                _SettingsContents = System.Text.Encoding.UTF8.GetString(
                    System.IO.File.ReadAllBytes("syslog.json")
                );
            } 

            if (System.String.IsNullOrEmpty(_SettingsContents))
            {
                System.Console.WriteLine("Unable to read syslog.json, using default configuration:");
                _Settings = Settings.Default();
                System.Console.WriteLine(Common.SerializeJson(_Settings));
            }
            else
            {
                try
                {
                    _Settings = Common.DeserializeJson<Settings>(_SettingsContents);
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Unable to deserialize syslog.json, please check syslog.json for correctness, exiting");
                    System.Environment.Exit(-1);
                }
            }

            if (!System.IO.Directory.Exists(_Settings.LogFileDirectory))
                System.IO.Directory.CreateDirectory(_Settings.LogFileDirectory);


            System.Console.WriteLine("---");
            System.Console.WriteLine(_Settings.Version);
            System.Console.WriteLine("(c)2017 Joel Christner");
            System.Console.WriteLine("---");

            InternalStartServer();

            

                         
            while (true)
            {
                string userInput = Common.InputString("[syslog :: ? for help] >", null, false);
                switch (userInput)
                {
                    case "?":
                        System.Console.WriteLine("---");
                        System.Console.WriteLine("  q      quit the application");
                        System.Console.WriteLine("  cls    clear the screen");
                        break;

                    case "q":
                        System.Console.WriteLine("Exiting.");
                        System.Environment.Exit(0);
                        break;

                    case "c":
                    case "cls":
                        System.Console.Clear();
                        break;
                            
                    default:
                        System.Console.WriteLine("Unknown command.  Type '?' for help.");
                        continue;
                }
            }
                  
            
        }

        static void InternalStartServer()
        {
            try
            {
                System.Console.WriteLine("Starting at " + System.DateTime.Now);
                  
                _ListenerThread = new System.Threading.Thread(ReceiverThread);
                _ListenerThread.Start();
                System.Console.WriteLine("Listening on UDP/" + _Settings.UdpPort + ".");

                System.Threading.Tasks.Task.Run(() => WriterTask());
                System.Console.WriteLine("Writer thread started successfully");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("***");
                System.Console.WriteLine("Exiting due to exception: " + e.Message);
                System.Environment.Exit(-1);
            }
        } 
    }
}
