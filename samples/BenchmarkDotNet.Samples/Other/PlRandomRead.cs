using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;
using Regression.PageLocator;

namespace Micro.Benchmark.Benchmarks.PageLocator
{
    [Config(typeof(Config))]
    public class PlRandomRead
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                
                Add( Job.RyuJitX64 );
                Add( Job.Core );
                Add(Job.Dry);

                // Exporters for data
                Add(GetExporters().ToArray());
                // Generate plots using R if %R_HOME% is correctly set
                Add(RPlotExporter.Default);

                Add(StatisticColumn.AllStatistics);

                Add(BaselineValidator.FailOnError);
                Add(JitOptimizationsValidator.FailOnError);
                // TODO: Uncomment next line. See https://github.com/PerfDotNet/BenchmarkDotNet/issues/272
                //Add(ExecutionValidator.FailOnError);
                Add(EnvironmentAnalyser.Default);
            }
        }

        private const int NumberOfOperations = 10000;

        [Params(8, 16, 32, 64, 128, 256, 512)]
        public int CacheSize { get; set; }

        [Params(5)]
        public int RandomSeed { get; set; }

        private List<long> _pageNumbers;

        private PageLocatorV1 _cacheV1;
        private PageLocatorV2 _cacheV2;
        private PageLocatorV7 _cacheV7;

        [Setup]
        public void Setup()
        {
            _cacheV1 = new PageLocatorV1(CacheSize);
            _cacheV2 = new PageLocatorV2(CacheSize);
            _cacheV7 = new PageLocatorV7(CacheSize);

            var generator = new Random(RandomSeed);

            _pageNumbers = new List<long>();
            for (int i = 0; i < NumberOfOperations; i++)
            {
                long valueBuffer = generator.Next();
                valueBuffer += (long)generator.Next() << 32;
                valueBuffer += (long)generator.Next() << 64;
                valueBuffer += (long)generator.Next() << 96;

                _pageNumbers.Add(valueBuffer);
            }
        }

        [Benchmark(OperationsPerInvoke = NumberOfOperations)]
        public void BasicV1()
        {
            foreach (var pageNumber in _pageNumbers)
            {
                _cacheV1.GetReadOnlyPage(pageNumber);
            }
        }

        [Benchmark(OperationsPerInvoke = NumberOfOperations)]
        public void BasicV2()
        {
            foreach (var pageNumber in _pageNumbers)
            {
                _cacheV2.GetReadOnlyPage(pageNumber);
            }
        }      

        [Benchmark(OperationsPerInvoke = NumberOfOperations)]
        public void BasicV7()
        {
            foreach (var pageNumber in _pageNumbers)
            {
                _cacheV7.GetReadOnlyPage(pageNumber);
            }
        }
    }
}