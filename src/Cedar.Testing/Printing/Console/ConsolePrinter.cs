namespace Cedar.Testing.Printing.Console
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing.PlainText;

    public class ConsolePrinter : IScenarioResultPrinter
    {
        private readonly IScenarioResultPrinter _inner;
        private bool _disposed;

        public ConsolePrinter(CreateTextWriter _ = null)
        {
            _inner = new PlainTextPrinter(file => Console.Out);
        }

        private class TraceTextWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void WriteLine(string value)
            {
                Trace.WriteLine(value);
            }

            public override Task WriteAsync(string value)
            {
                Trace.Write(value);
                return Task.FromResult(0);
            }

            public override Task WriteLineAsync()
            {
                Trace.WriteLine("");
                return Task.FromResult(0);
            }

            public override Task WriteLineAsync(string value)
            {
                Trace.WriteLine(value);
                return Task.FromResult(0);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Flush().Wait();
        }

        public Task PrintCategoryFooter(Type foundOn)
        {
            return _inner.PrintCategoryFooter(foundOn);
        }

        public Task PrintCategoryHeader(Type foundOn)
        {
            return _inner.PrintCategoryHeader(foundOn);
        }

        public Task PrintResult(ScenarioResult result)
        {
            return _inner.PrintResult(result);
        }

        public Task Flush()
        {
            return _inner.Flush();
        }

        public string FileExtension
        {
            get { return string.Empty; }
        }
    }
}