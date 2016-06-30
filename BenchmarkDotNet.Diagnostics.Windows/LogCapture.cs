﻿using System;
using System.Collections.Generic;
using BenchmarkDotNet.Loggers;

namespace BenchmarkDotNet.Diagnostics.Windows
{
    internal class LogCapture : ILogger
    {
        public IList<OutputLine> CapturedOutput = new List<OutputLine>(100);

        public void Write(LogKind logKind, string text)
        {
            CapturedOutput.Add(new OutputLine
            {
                Kind = logKind,
                Text = text
            });
        }

        public void WriteLine()
        {
            CapturedOutput.Add(new OutputLine
            {
                Kind = LogKind.Default,
                Text = Environment.NewLine
            });
        }

        public void WriteLine(LogKind logKind, string text)
        {
            Write(logKind, text);
            WriteLine();
        }

        public void Clear()
        {
            CapturedOutput.Clear();
        }
    }

    internal struct OutputLine
    {
        public LogKind Kind { get; set; }
        public string Text { get; set; }
    }
}
