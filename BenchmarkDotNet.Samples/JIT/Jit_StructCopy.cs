using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchmarkDotNet.Samples.JIT
{
    [Config(typeof(Config))]
    public class Jit_StructCopy
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.RyuJitX64, Job.LegacyJitX64);
            }
        }

        public struct S4
        {
            public long A1;
            public long A2;
            public long A3;
            public long A4;
        }

        public struct S8
        {
            public long A1;
            public long A2;
            public long A3;
            public long A4;
            public long A5;
            public long A6;
            public long A7;
            public long A8;
        }

        private S4 src32;
        private S4 dest32;

        private S8 src64;
        private S8 dest64;

        [Setup]
        public void Setup()
        {
            src32 = new S4 { A1 = 23, A2 = 45, A3 = 311, A4 = 4122 };
            src64 = new S8 { A1 = 23, A2 = 45, A3 = 311, A4 = 4122, A5 = 23, A6 = 45, A7 = 311, A8 = 4122 };
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public S4 CopyStruct32Bytes()
        {
            dest32 = src32;
            return dest32;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public S4 CopyProperties32Bytes()
        {
            dest32.A1 = src32.A1;
            dest32.A2 = src32.A2;
            dest32.A3 = src32.A3;
            dest32.A4 = src32.A4;
            return dest32;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public S8 CopyStruct64Bytes()
        {
            dest64 = src64;
            return dest64;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public S8 CopyProperties64Bytes()
        {
            dest64.A1 = src64.A1;
            dest64.A2 = src64.A2;
            dest64.A3 = src64.A3;
            dest64.A4 = src64.A4;
            dest64.A5 = src64.A5;
            dest64.A6 = src64.A6;
            dest64.A7 = src64.A7;
            dest64.A8 = src64.A8;
            return dest64;
        }

    }
}