
namespace libSyslogServer
{


    public partial class SyslogServer
    {
        static void WriterTask()
        {
            try
            {
                while (true)
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    if (System.DateTime.Compare(_LastWritten.AddSeconds(_Settings.LogWriterIntervalSec), System.DateTime.Now) < 0)
                    {
                        lock (_WriterLock)
                        {
                            if (_MessageQueue == null || _MessageQueue.Count < 1)
                            {
                                _LastWritten = System.DateTime.Now;
                                continue;
                            }
                             
                            foreach (string currMessage in _MessageQueue)
                            { 
                                string currFilename = _Settings.LogFileDirectory + System.DateTime.Now.ToString("MMddyyyy") + "-" + _Settings.LogFilename;
                                 
                                if (!System.IO.File.Exists(currFilename))
                                {
                                    System.Console.WriteLine("Creating file: " + currFilename + System.Environment.NewLine);
                                    {
                                        using (System.IO.FileStream fsCreate = 
                                            System.IO.File.Create(currFilename))
                                        {
                                            byte[] createData = new System.Text.UTF8Encoding(true).GetBytes("--- Creating log file at " + System.DateTime.Now + " ---" + System.Environment.NewLine);
                                            fsCreate.Write(createData, 0, createData.Length);
                                        }
                                    }
                                }
                                 
                                using (System.IO.StreamWriter swAppend = 
                                    System.IO.File.AppendText(currFilename))
                                {
                                    swAppend.WriteLine(currMessage);
                                } 
                            }

                            _LastWritten = System.DateTime.Now;
                            _MessageQueue = new System.Collections.Generic.List<string>();
                        } 
                    } 
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("***");
                System.Console.WriteLine("WriterTask exiting due to exception: " + e.Message);
                System.Environment.Exit(-1);
            }
        }
    }
}
