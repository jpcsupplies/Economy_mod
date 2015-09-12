﻿namespace Economy.scripts
{
    using Sandbox.ModAPI;
    using System;
    using System.IO;

    public class TextLogger
    {
        #region fields

        private string _logFileName;
        private TextWriter _logWriter;
        private bool _isInitialized;

        #endregion

        #region properties

        public string LogFileName { get { return _logFileName; } }

        #endregion

        #region ctor

        /// <summary>
        /// Initialize the TextLogger with a default filename.
        /// The TextLogger must be Initialized before it can write log entries.
        /// This allows a TextLogger to be created and the Write(...) methods invoked without the TextLogger initialized so you don't have to wrap the TextLogger variable with if statements.
        /// </summary>
        public void Init()
        {
            _isInitialized = true;
            _logFileName = string.Format("TextLog_{0}_{1:yyyy-MM-dd_HH-mm-ss}.log", MyAPIGateway.Session != null ? Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath) : "0", DateTime.Now);
        }

        /// <summary>
        /// Initialize the TextLogger with a custom filename.
        /// The TextLogger must be Initialized before it can write log entries.
        /// This allows a TextLogger to be created and the Write(...) methods invoked without the TextLogger initialized so you don't have to wrap the TextLogger variable with if statements.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="addTimestamp"></param>
        public void Init(string filename, bool addTimestamp = false)
        {
            _isInitialized = true;
            if (addTimestamp)
                _logFileName = string.Format("TextLog_{0}_{1:yyyy-MM-dd_HH-mm-ss}{2}", Path.GetFileNameWithoutExtension(filename), DateTime.Now, Path.GetExtension(filename));
            else
                _logFileName = filename;
        }

        ~TextLogger()
        {
            Terminate();
        }

        #endregion

        public void Write(string text, params object[] args)
        {
            if (!_isInitialized)
                return;

            // we create the writer when it is needed to prevent the creation of empty files
            if (_logWriter == null)
                _logWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(_logFileName, typeof(TextLogger));

            string message;
            if (args == null || args.Length == 0)
                message = text;
            else
                message = string.Format(text, args);

            _logWriter.WriteLine("{0:yyyy-MM-dd HH:mm:ss:fff} - {1}", DateTime.Now, message);
            _logWriter.Flush();
        }

        public void WriteException(Exception ex, string additionalInformation = null)
        {
            if (!_isInitialized)
                return;

            // we create the writer when it is needed to prevent the creation of empty files
            if (_logWriter == null)
                _logWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(_logFileName, typeof(TextLogger));

            _logWriter.WriteLine("{0:yyyy-MM-dd HH:mm:ss:fff} Error - {1}", DateTime.Now, ex);

            if (!string.IsNullOrEmpty(additionalInformation))
            {
                _logWriter.WriteLine("Additional information on {0}:", ex.Message);
                _logWriter.WriteLine(additionalInformation);
            }

            _logWriter.Flush();
        }

        public void Terminate()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter = null;
            }
        }
    }
}
