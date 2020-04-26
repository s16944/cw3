using System;
using System.IO;

namespace cw3.Services
{
    public class FileRequestLogger : IRequestLogger, IDisposable
    {
        private readonly StreamWriter _fileWriter;

        public FileRequestLogger(string filePath)
        {
            _fileWriter = new StreamWriter(filePath, true);
        }

        public void Log(string method, string path, string query, string body)
        {
            _fileWriter.WriteLine($"{method} {path}{query}");
            if(!string.IsNullOrEmpty(body)) _fileWriter.WriteLine(body);
            _fileWriter.WriteLine();
        }

        public void Dispose()
        {
            _fileWriter.Close();
        }
    }
}