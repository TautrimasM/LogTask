
namespace LogComponent.nUnitTests

{
    using LogTest;
    using static LogTest.AsyncLog;

    public class LogComponentTests
    {

        private readonly string _logDirectory = @"C:\TestDir";
        [SetUp]
        public void Setup()
        {
            Func<String> _directoryFunc = () => _logDirectory;
            DirectoryProvider.Set(_directoryFunc);
        }



        [Test, Order(1)]
        public void Write()
        {

            ILog logger = new AsyncLog();
            logger.Write("kazkas");
            logger.StopWithFlush();
            Thread.Sleep(100);
            logger.CloseFile();

            var latestLogFile = GetLatestLogFile();
            var secondRow = File.ReadLines(latestLogFile).Skip(1).FirstOrDefault();
            DeleteLogFile(latestLogFile);

            Assert.IsTrue(secondRow.Contains("kazkas"));

        }

        [Test, Order(2)]
        public void StopWithFlush()
        {
            ILog logger = new AsyncLog();
            for (int i = 0; i < 15; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
            }

            logger.StopWithFlush();
            Thread.Sleep(500);
            logger.CloseFile();

            var latestLogFile = GetLatestLogFile();
            var rowCount = File.ReadLines(latestLogFile).Count();
            DeleteLogFile(latestLogFile);

            Assert.AreEqual(rowCount, 16);

        }

        [Test, Order(3)]
        public void StopWithoutFlush()
        {
            ILog logger = new AsyncLog();
            for (int i = 50; i > 0; i--)
            {
                logger.Write("Number with No flush: " + i.ToString());
            }
            logger.StopWithoutFlush();
            logger.CloseFile();

            var latestLogFile = GetLatestLogFile();
            var rowCount = File.ReadLines(latestLogFile).Count();
            DeleteLogFile(latestLogFile);

            Assert.LessOrEqual(rowCount, 50);

        }

        [Test, Order(4)]
        public void CreateNewFileAfterMidnight()
        {
            var logger = new AsyncLog();

            logger.Write("some string");
            Thread.Sleep(100);
            logger.CloseFile();
            DateTimeProvider.Set(() => new DateTime(2023, 05, 04, 00, 00, 00));
            logger.Write("some string");
            Thread.Sleep(100);
            logger.CloseFile();

            var logFiles = Directory.GetFiles(_logDirectory, "*.log");
            DeleteLogFile(logFiles[1]);
            DeleteLogFile(logFiles[0]);
            DeleteDirectory(_logDirectory);

            Assert.AreEqual(logFiles.Length, 2);


        }


        private string GetLatestLogFile()
        {
            var logFiles = Directory.GetFiles(_logDirectory, "*.log")
                                    .OrderByDescending(f => File.GetCreationTime(f));
            return logFiles.FirstOrDefault();
        }
        private void DeleteLogFile(String file)
        {

            File.Delete(Path.Combine(_logDirectory, file));
        }
        private void DeleteDirectory(String directory)
        {
            Directory.Delete(directory);
        }

    }
}