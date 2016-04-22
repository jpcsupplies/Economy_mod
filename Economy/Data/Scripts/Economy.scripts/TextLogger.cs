namespace Economy.scripts
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Sandbox.ModAPI;
    using VRage;

    /// <summary>
    /// Generic text file logger devloped by Midspace for Space Engineers mods.
    /// </summary>
    public class TextLogger
    {
        #region fields

        private string _logFileName;
        private TextWriter _logWriter;
        private bool _isInitialized;
        private int _delayedWrite;
        private int _writeCounter;
        private readonly FastResourceLock _executionLock = new FastResourceLock();

        #endregion

        #region properties

        public string LogFileName { get { return _logFileName; } }

        public string LogFile { get { return Path.Combine(MyAPIGateway.Utilities.GamePaths.UserDataPath, "Storage", _logFileName); } }

        public bool IsActive { get { return _isInitialized; } }

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
        /// <param name="delayedWrite"></param>
        public void Init(string filename, bool addTimestamp = false, int delayedWrite = 0)
        {
            _isInitialized = true;
            if (addTimestamp)
                _logFileName = string.Format("TextLog_{0}_{1:yyyy-MM-dd_HH-mm-ss}{2}", Path.GetFileNameWithoutExtension(filename), DateTime.Now, Path.GetExtension(filename));
            else
                _logFileName = filename;

            _delayedWrite = delayedWrite;
        }

        ~TextLogger()
        {
            Terminate();
        }

        #endregion

        public void WriteStart(string text, params object[] args)
        {
            Write(TraceEventType.Start, false, text, args);
        }

        public void WriteStop(string text, params object[] args)
        {
            Write(TraceEventType.Stop, false, text, args);
        }

        public void WriteVerbose(string text, params object[] args)
        {
            Write(TraceEventType.Verbose, false, text, args);
        }

        public void WriteInfo(string text, params object[] args)
        {
            Write(TraceEventType.Information, false, text, args);
        }

        public void WriteWarning(string text, params object[] args)
        {
            Write(TraceEventType.Warning, false, text, args);
        }

        public void WriteError(string text, params object[] args)
        {
            Write(TraceEventType.Error, false, text, args);
        }

        public void WriteRaw(TraceEventType eventType, string text, params object[] args)
        {
            Write(eventType, true, text, args);
        }

        public void WriteException(Exception ex, string additionalInformation = null)
        {
            string msg = ex + "\r\n";

            if (!string.IsNullOrEmpty(additionalInformation))
            {
                msg += string.Format("Additional information on {0}:\r\n", ex.Message);
                msg += additionalInformation + "\r\n";
            }

            Write(TraceEventType.Error, false, msg);
        }

        private void Write(TraceEventType eventType, bool writeRaw, string text, params object[] args)
        {
            if (!_isInitialized)
                return;

            // we create the writer when it is needed to prevent the creation of empty files
            if (_logWriter == null)
            {
                try
                {
                    _logWriter = MyAPIGateway.Utilities.WriteFileInGlobalStorage(_logFileName);
                }
                catch (Exception ex)
                {
                    Terminate();
                    WriteGameLog("## TextLogger Exception caught in mod. Message: {0}", ex.Message);
                    return;
                }
            }

            string message;
            if (args == null || args.Length == 0)
                message = text;
            else
                message = string.Format(text, args);

            if (writeRaw)
                _logWriter.Write(message);
            else
                _logWriter.WriteLine("{0:yyyy-MM-dd HH:mm:ss.fff} - {1}", DateTime.Now, message);
            _writeCounter++;
            if (_delayedWrite == 0 || _writeCounter > _delayedWrite || eventType <= TraceEventType.Error)
            {
                _logWriter.Flush();
                _writeCounter = 0;
            }
        }

        public void Flush()
        {
            if (!_isInitialized)
                return;

            if (_logWriter != null)
                _logWriter.Flush();
        }

        public void Terminate()
        {
            using (_executionLock.AcquireExclusiveUsing())
            {
                _isInitialized = false;
                if (_logWriter != null)
                {
                    try
                    {
                        _logWriter.Flush();
                        _logWriter.Dispose();
                    }
                    catch
                    {
                        // catch exception caused by SE Server Extender Essential plugin during auto restart
                        // which causes file stream to be already closed during flush.
                    }
                    _logWriter = null;
                }
            }
        }

        public static void WriteGameLog(string text, params object[] args)
        {
            string message = text;
            if (args != null && args.Length != 0)
                message = string.Format(text, args);

            if (MyAPIGateway.Utilities.IsDedicated)
                VRage.Utils.MyLog.Default.WriteLineAndConsole(message);
            else
                VRage.Utils.MyLog.Default.WriteLine(message);
        }
    }
}
