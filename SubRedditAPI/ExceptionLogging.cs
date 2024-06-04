
namespace SubRedditAPI
{
    public static class ExceptionLogging
    {
        private static string errorLineNo, errorMsg, exType, errorLocation;

        public static void SendErrorToText(Exception ex)
        {
            var line = Environment.NewLine + Environment.NewLine;

            errorLineNo = ex?.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
            errorMsg = ex.GetType().Name.ToString();
            exType = ex.GetType().ToString();
            errorLocation = ex.Message.ToString();

            try
            {
                string filePath = AppDomain.CurrentDomain.BaseDirectory + @"/ExceptionDetailsFile/";

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + DateTime.Today.ToString("dd-MM-yy") + ".txt";
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    string error = "Log Written Date:" + " " + DateTime.Now.ToString() + line + "Error Line No :" + " " + errorLineNo + line + "Error Message:" + " " + errorMsg + line + "Exception Type:" + " " + exType + line + "Error Location :" + " " + errorLocation + line;
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("-------------------------------------------------------------------------------");
                    sw.WriteLine(line);
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------*End*------------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

    }
}
