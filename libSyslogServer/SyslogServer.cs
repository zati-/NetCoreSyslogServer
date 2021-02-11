
using System;
using System.Text;
using System.Threading.Tasks;


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
                _SettingsContents = Encoding.UTF8.GetString(
                    System.IO.File.ReadAllBytes("syslog.json")
                );
            } 

            if (System.String.IsNullOrEmpty(_SettingsContents))
            {
                Console.WriteLine("Unable to read syslog.json, using default configuration:");
                _Settings = Settings.Default();
                Console.WriteLine(Common.SerializeJson(_Settings));
            }
            else
            {
                try
                {
                    _Settings = Common.DeserializeJson<Settings>(_SettingsContents);
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Unable to deserialize syslog.json, please check syslog.json for correctness, exiting");
                    System.Environment.Exit(-1);
                }
            }

            if (!System.IO.Directory.Exists(_Settings.LogFileDirectory))
                System.IO.Directory.CreateDirectory(_Settings.LogFileDirectory);

            
            Console.WriteLine("---");
            Console.WriteLine(_Settings.Version);
            Console.WriteLine("(c)2017 Joel Christner");
            Console.WriteLine("---");

            InternalStartServer();

            

                         
            while (true)
            {
                string userInput = Common.InputString("[syslog :: ? for help] >", null, false);
                switch (userInput)
                {
                    case "?":
                        Console.WriteLine("---");
                        Console.WriteLine("  q      quit the application");
                        Console.WriteLine("  cls    clear the screen");
                        break;

                    case "q": 
                        Console.WriteLine("Exiting.");
                        System.Environment.Exit(0);
                        break;

                    case "c":
                    case "cls":
                        Console.Clear();
                        break;
                            
                    default:
                        Console.WriteLine("Unknown command.  Type '?' for help.");
                        continue;
                }
            }
                  
            
        }

        static void InternalStartServer()
        {
            try
            { 
                Console.WriteLine("Starting at " + System.DateTime.Now);
                  
                _ListenerThread = new System.Threading.Thread(ReceiverThread);
                _ListenerThread.Start();
                Console.WriteLine("Listening on UDP/" + _Settings.UdpPort + ".");

                Task.Run(() => WriterTask());
                Console.WriteLine("Writer thread started successfully");
            }
            catch (System.Exception e)
            {
                Console.WriteLine("***");
                Console.WriteLine("Exiting due to exception: " + e.Message);
                System.Environment.Exit(-1);
            }
        } 
    }
}
