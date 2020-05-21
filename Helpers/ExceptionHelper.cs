using System;

namespace SampleClient.Helpers
{
    public static class ExceptionHelper
    {
        public static string ExceptionToString(Exception ex)
        {
            var exstr = "";
            if (ex == null) return exstr;
            exstr = RecursiveExString(ex, exstr);
            return exstr;
        }

        private static string RecursiveExString(Exception ex, string str)
        {
            str = str + ex.Message;
            if ((ex.InnerException != null) && (str.Length < 800))
                return RecursiveExString(ex.InnerException, str);

            return str;
        }
    }
}