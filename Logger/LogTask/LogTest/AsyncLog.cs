namespace LogTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class AsyncLog : ILog
    {
        private Thread _runThread;
        private List<LogLine> _lines = new List<LogLine>();

        private StreamWriter _writer; 

        private bool _exit;


        //DateTimeProvider ir DirectoryProvider mockinimui
        public static class DateTimeProvider
        {
            private static Func<DateTime> _dateTimeNowFunc = () => DateTime.Now;
            public static DateTime Now => _dateTimeNowFunc();

            public static void Set(Func<DateTime> dateTimeNowFunc)
            {
                _dateTimeNowFunc = dateTimeNowFunc;
            }
        }
        public static class DirectoryProvider
        {
            private static Func<String> _directoryFunc = () => @"C:\LogTest";

            public static string directory => _directoryFunc();
           
            public static void Set(Func<String> directoryFunc)
            {
                _directoryFunc = directoryFunc;
            }
        }

        public AsyncLog()
        {
            if (!Directory.Exists(DirectoryProvider.directory)) 
                Directory.CreateDirectory(DirectoryProvider.directory);

            this._writer = File.AppendText(@""+ DirectoryProvider.directory + @"\Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");
            
            this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

            this._writer.AutoFlush = true;

            this._runThread = new Thread(this.MainLoop);
            this._runThread.Start();
        }

        private bool _QuitWithFlush = false;


        DateTime _curDate = DateTimeProvider.Now;
      
        private void MainLoop()
        {
            while (!this._exit)
            {
                if (this._lines.Count > 0)
                {
                    int f = 0;
                    List<LogLine> _handled = new List<LogLine>();
                    try // sugaut errors
                    {
                        foreach (LogLine logLine in this._lines.ToList()) // ToList() kad dirbtu su list'o instance, siekiant isvengt klaidu, kai i lista kreipiasi keletas threads.
                        {                     
                            f++;

                            if (f > 5)
                                continue;

                            if (!this._exit || this._QuitWithFlush)
                            {
                                _handled.Add(logLine);

                                StringBuilder stringBuilder = new StringBuilder();

                                if (DateTimeProvider.Now.TimeOfDay == TimeSpan.Zero) // vietoj tikrinimo ar 24h praejo padariau tikrinima ar dabar 00:00:00, senas kodas: if ((DateTime.Now - _curDate).Days != 0) 
                                {
                                    _curDate = DateTimeProvider.Now;

                                    this._writer = File.AppendText(@"" + DirectoryProvider.directory + @"\Log" + DateTimeProvider.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

                                    this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

                                    stringBuilder.Append(Environment.NewLine);

                                    this._writer.Write(stringBuilder.ToString());

                                    this._writer.AutoFlush = true;
                                }

                                stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                                stringBuilder.Append("\t");
                                stringBuilder.Append(logLine.LineText());
                                stringBuilder.Append("\t");

                                stringBuilder.Append(Environment.NewLine);

                                this._writer.Write(stringBuilder.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    { 
                        System.Console.WriteLine("Error Caught:"+ e.Message); // kad matytumem kas per klaida
                    }

                    for (int y = 0; y < _handled.Count; y++)
                    {
                        this._lines.Remove(_handled[y]);   
                    }

                    if (this._QuitWithFlush == true && this._lines.Count == 0) 
                        this._exit = true;

                    Thread.Sleep(50); 
        
                }
            }
        }

        public void CloseFile()
        {
            this._writer.Close();
        }
        public void StopWithoutFlush()
        {
            this._exit = true;
        }

        public void StopWithFlush()
        {
            this._QuitWithFlush = true;
        }

        public void Write(string text)
        {
            this._lines.Add(new LogLine() { Text = text, Timestamp = DateTime.Now });
        }
    }
}