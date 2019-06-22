using System.IO;

namespace Assets.Scripts.Utility
{

    /// <summary>
    /// Provided for logging test data.
    /// </summary>
    class Test
    {

        private static string LogPath = "./test/TestLog";
        private static Test Instance = new Test();
        private StreamWriter LogFile;

        private Test()
        {
        }

        ~Test()
        {
            if (LogFile != null)
                LogFile.Close();
        }

        /// <summary>
        /// Opens file to log. File is closed on destruction of this object.
        /// </summary>
        /// <param name="name"></param>
        public static void StartLog(string name)
        {
            Instance.LogFile = new StreamWriter(File.Open(LogPath+name+".txt", FileMode.Create));
        }

        /// <summary>
        /// Logs message to file and flushes.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            if (Settings.TEST_LOG)
            {
                Instance.LogFile.WriteLine(message);
                Instance.LogFile.Flush();
            }
        }


    }
}
