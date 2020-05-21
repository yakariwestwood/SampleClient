using System;

namespace SampleClient.Helpers
{
    public static class SerilogHelper
    {
        #region Error

        public static void Error(string message)
        {
            Serilog.Log.Error(message);
        }

        public static void Error(string methodname, string message)
        {
            Serilog.Log.Error(methodname + " : " + message);
        }

        public static void Error(string classname, string methodname, string message)
        {
            Serilog.Log.Error(classname + " : " + methodname + " : " + message);
        }

        public static void Error(int userid, string methodname, string message)
        {
            Serilog.Log.Error(userid + " : " + methodname + " : " + message);
        }

        public static void Error(int userid, string classname, string methodname, string message)
        {
            Serilog.Log.Error(userid + " : " + classname + " : " + methodname + " : " + message);
        }

        #endregion

        #region Debug

        public static void Debug(string message)
        {
            Serilog.Log.Debug(message);
        }

        public static void Verbose(string message)
        {
            Serilog.Log.Verbose(message);
        }

        #endregion


        #region Information

        public static void Information(string message)
        {
            Serilog.Log.Information(message);
        }

        public static void Information(string methodname, string message)
        {
            Serilog.Log.Information(methodname + " : " + message);
        }

        public static void Information(string classname, string methodname, string message)
        {
            Serilog.Log.Information(classname + " : " + methodname + " : " + message);
        }

        public static void Information(int userid, string methodname, string message)
        {
            Serilog.Log.Information(userid + " : " + methodname + " : " + message);
        }

        public static void Information(int userid, string classname, string methodname, string message)
        {
            Serilog.Log.Information(userid + " : " + classname + " : " + methodname + " : " + message);
        }

        #endregion

        #region Exception

        public static void Exception(string methodname, Exception exception)
        {
            Serilog.Log.Error(methodname + " : " + ExceptionHelper.ExceptionToString(exception));
        }

        public static void Exception(string classname, string methodname, Exception exception)
        {
            Serilog.Log.Error(classname + " : " + methodname + " : " + ExceptionHelper.ExceptionToString(exception));
        }

        public static void Exception(int userid, string methodname, Exception exception)
        {
            Serilog.Log.Error(userid + " : " + methodname + " : " + ExceptionHelper.ExceptionToString(exception));
        }

        public static void Exception(int userid, string classname, string methodname, Exception exception)
        {
            Serilog.Log.Error(userid + " : " + classname + " : " + methodname + " : " +
                              ExceptionHelper.ExceptionToString(exception));
        }

        #endregion
    }
}