﻿using System;
using System.Diagnostics;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Reports
{
    /// <summary>
    /// The basic captured statistics for a benchmark.
    /// </summary>
    public sealed class Measurement
    {
        public IterationMode IterationMode { get; }

        public int LaunchIndex { get; }

        public int IterationIndex { get; }

        /// <summary>
        /// Gets the number of operations performed.
        /// </summary>
        public long Operations { get; }

        /// <summary>
        /// Gets the total number of nanoseconds it took to perform all operations.
        /// </summary>
        public double Nanoseconds { get; }

        /// <summary>
        /// Creates an instance of <see cref="Measurement"/> class.
        /// </summary>
        /// <param name="launchIndex"></param>
        /// <param name="iterationMode"></param>
        /// <param name="iterationIndex"></param>
        /// <param name="operations">The number of operations performed.</param>
        /// <param name="nanoseconds">The total number of nanoseconds it took to perform all operations.</param>
        public Measurement(int launchIndex, IterationMode iterationMode, int iterationIndex, long operations, double nanoseconds)
        {
            IterationMode = iterationMode;
            IterationIndex = iterationIndex;
            Operations = operations;
            Nanoseconds = nanoseconds;
            LaunchIndex = launchIndex;
        }

        public string ToOutputLine() => $"{IterationMode} {IterationIndex}: {GetDisplayValue()}";
        private string GetDisplayValue() => $"{Operations} op, {Nanoseconds.ToStr()} ns, {GetAverageTime()}";
        private string GetAverageTime() => $"{(Nanoseconds / Operations).ToTimeStr()}/op";

        /// <summary>
        /// Parses the benchmark statistics from the plain text line.
        /// 
        /// E.g. given the input <paramref name="line"/>:
        /// 
        ///     Target 1: 10 op, 1005842518 ns
        /// 
        /// Will extract the number of <see cref="Operations"/> performed and the 
        /// total number of <see cref="Nanoseconds"/> it took to perform them.
        /// </summary>
        /// <param name="logger">The logger to write any diagnostic messages to.</param>
        /// <param name="line">The line to parse.</param>
        /// <returns>An instance of <see cref="Measurement"/> if parsed successfully. <c>Null</c> in case of any trouble.</returns>
        public static Measurement Parse(ILogger logger, string line, int processIndex)
        {
            try
            {
                var lineSplit = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                var iterationInfo = lineSplit[0];
                var iterationInfoSplit = iterationInfo.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var iterationMode = ParseIterationMode(iterationInfoSplit[0]);
                var iterationIndex = 0;
                int.TryParse(iterationInfoSplit[1], out iterationIndex);

                var measurementsInfo = lineSplit[1];
                var measurementsInfoSplit = measurementsInfo.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var op = 1L;
                var ns = double.PositiveInfinity;
                foreach (var item in measurementsInfoSplit)
                {
                    var measurementSplit = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var value = measurementSplit[0];
                    var unit = measurementSplit[1];
                    switch (unit)
                    {
                        case "ns":
                            ns = double.Parse(value, EnvironmentInfo.MainCultureInfo);
                            break;
                        case "op":
                            op = long.Parse(value);
                            break;
                    }
                }
                return new Measurement(processIndex, iterationMode, iterationIndex, op, ns);
            }
            catch (Exception)
            {
                logger.WriteLineError("Parse error in the following line:");
                logger.WriteLineError(line);
                return null;
            }
        }

        private static IterationMode ParseIterationMode(string name)
        {
            IterationMode mode;
            return Enum.TryParse(name, out mode) ? mode : IterationMode.Unknown;
        }

        public override string ToString() => $"#{LaunchIndex}/{IterationMode} {IterationIndex}: {Operations} op, {Nanoseconds.ToTimeStr()}";
    }
}