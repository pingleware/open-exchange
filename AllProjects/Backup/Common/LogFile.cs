//////////////////////////////////////////////////////////////////////////////
//
// Attribution-NonCommercial-ShareAlike 3.0 Unported (CC BY-NC-SA 3.0)
// Disclaimer
// 
// This file is part of OpEx.
// 
// You are free:
// 	* to Share : to copy, distribute and transmit the work;
//	* to Remix : to adapt the work.
//		
// Under the following conditions:
// 	* Attribution : You must attribute OpEx to Marco De Luca
//     (marco.de.luca@algotradingconsulting.com);
// 	* Noncommercial : You may not use this work for commercial purposes;
// 	* Share Alike : If you alter, transform, or build upon this work,
//     you may distribute the resulting work only under the same or similar
//     license to this one. 
// 	
// With the understanding that: 
// 	* Waiver : Any of the above conditions can be waived if you get permission
//     from the copyright holder. 
// 	* Public Domain : Where the work or any of its elements is in the public
//     domain under applicable law, that status is in no way affected by the
//     license. 
// 	* Other Rights : In no way are any of the following rights affected by
//     the license: 
//		 . Your fair dealing or fair use rights, or other applicable copyright
//          exceptions and limitations; 
//		 . The author's moral rights; 
//		 . Rights other persons may have either in the work itself or in how
//          the work is used, such as publicity or privacy rights. 
//	* Notice : For any reuse or distribution, you must make clear to others
//     the license terms of this work. The best way to do this is with a link
//     to this web page. 
// 
// You should have received a copy of the Legal Code of the CC BY-NC-SA 3.0
// Licence along with OpEx.
// If not, see <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>.
//
//////////////////////////////////////////////////////////////////////////////


﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace OPEX.Common
{
    /// <summary>
    /// Represents a buffered Log that writes to a file.
    /// </summary>
    public class LogFile : Log
    {
        private static long DefaultBufferSize = 1;
        private static readonly bool DefaultAddTimeStamp = true;
        private static readonly string DefaultPrefix = "log";
        private static readonly string DefaultPath = ".";
        private static readonly string DefaultSuffix = ".txt";

        private long _counter;
        private long _bufferSize;
        private string _filePrefix;
        private string _path;
        private bool _addTimeStamp;

        private string _fullFileName;
        private TextWriter _theLogFile;

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.LogFile class.
        /// </summary>
        /// <param name="filePrefix">The first part of the log file name.</param>
        /// <param name="path">The path of the log file.</param>
        /// <param name="addTimeStamp">If true, append the creation date and time
        /// timestamp to the filename, in the form "_yyyyMMdd_HHmmss".</param>
        public LogFile(string filePrefix, string path, bool addTimeStamp)
        {
            _filePrefix = filePrefix;
            _path = path;
            _addTimeStamp = addTimeStamp;
            _counter = 0;
            _bufferSize = DefaultBufferSize;
        }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.LogFile class.
        /// </summary>
        /// <param name="filePrefix">The first part of the log file name.</param>
        /// <param name="path">The path of the log file.</param>
        public LogFile(string filePrefix, string path)
            : this(filePrefix, path, DefaultAddTimeStamp)
        { }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.LogFile class.
        /// </summary>
        /// <param name="filePrefix">The first part of the log file name.</param>
        public LogFile(string filePrefix)
            : this(filePrefix, DefaultPath)
        { }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.LogFile class.
        /// </summary>
        public LogFile()
            : this(DefaultPrefix)
        { }

        /// <summary>
        /// Gets or sets the size of the log buffer, expressed in lines.
        /// </summary>
        public long BufferSize
        {
            get { return _bufferSize; }
            set
            {
                if (value <= 1)
                {
                    throw new ApplicationException("Buffer size must be positive!");
                }

                _bufferSize = value;
            }
        }

        /// <summary>
        /// Creates the file. LogFile is ready after this is called.
        /// </summary>
        public override void Start()
        {
            string fileName = _filePrefix;

            if (_addTimeStamp)
            {
                fileName += DateTime.Now.ToString("_yyyyMMdd_HHmmss");
            }
            fileName += DefaultSuffix;
            
            _fullFileName = Path.Combine(_path, fileName);

            try
            {
                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                }
                _theLogFile = File.CreateText(_fullFileName);
            }
            catch (Exception ex)            
            {
                Console.WriteLine("Exception while trying to open filelog {0} : {1}", _fullFileName, ex.Message);
            }
        }

        /// <summary>
        /// Closes the file. LogFile shouldn't be used after this is called.
        /// </summary>
        public override void Stop()
        {
            if (_theLogFile != null)
            {
                _theLogFile.Flush();
                _theLogFile.Close();
            }
        }

        protected override void InnerTrace(LogLevel level, string message, params object[] args)
        {
            _theLogFile.WriteLine(FormatLine(level, message, args));
            if (++_counter == _bufferSize)
            {
                _theLogFile.Flush();
                _counter = 0;
            }
        }
    }
}
