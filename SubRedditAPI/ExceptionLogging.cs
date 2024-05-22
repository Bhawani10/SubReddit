
namespace SubRedditAPI
{
    public static class ExceptionLogging
    {
        private static string ErrorlineNo, Errormsg, extype, exurl, hostIp, ErrorLocation, HostAdd;

        public static void SendErrorToText(Exception ex)
        {
            var line = Environment.NewLine + Environment.NewLine;

            ErrorlineNo = ex?.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
            Errormsg = ex.GetType().Name.ToString();
            extype = ex.GetType().ToString();
            ErrorLocation = ex.Message.ToString();

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
                    string error = "Log Written Date:" + " " + DateTime.Now.ToString() + line + "Error Line No :" + " " + ErrorlineNo + line + "Error Message:" + " " + Errormsg + line + "Exception Type:" + " " + extype + line + "Error Location :" + " " + ErrorLocation + line + " Error Page Url:" + " " + exurl + line + "User Host IP:" + " " + hostIp + line;
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("-------------------------------------------------------------------------------------");
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
