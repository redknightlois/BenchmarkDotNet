﻿using System.Linq;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using Xunit;

namespace BenchmarkDotNet.Tests
{
    public class TypeParserTests
    {
        [Fact]
        public void CanSelectMethods()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "method=Method2,Method3" });

            Assert.Equal(2, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && 
                                                   match.Methods.Any(m => m.Name == "Method2")));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassB" && 
                                                   match.Methods.Any(m => m.Name == "Method2") &&
                                                   match.Methods.Any(m => m.Name == "Method3")));
        }

        [Fact]
        public void CanSelectMethodsWithFullName()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "method=BenchmarkDotNet.Tests.ClassA.Method2,BenchmarkDotNet.Tests.ClassB.Method3" });

            Assert.Equal(2, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" &&
                                                   match.Methods.Any(m => m.Name == "Method2")));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassB" &&
                                                   match.Methods.Any(m => m.Name == "Method3")));
        }

        [Fact]
        public void CanSelectClasses()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "class=ClassC,ClassA" });

            // ClassC not matched as it has NO methods with the [Benchmark] attribute
            Assert.Equal(1, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && match.AllMethodsInType));
        }

        [Fact]
        public void CanSelectClassesWithFullName()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "class=BenchmarkDotNet.Tests.ClassC,BenchmarkDotNet.Tests.ClassA" });

            // ClassC not matched as it has NO methods with the [Benchmark] attribute
            Assert.Equal(1, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && match.AllMethodsInType));
        }

        [Fact]
        public void CanSelectNamespaces()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC), typeof(NOTTests.ClassD) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "namespace=BenchmarkDotNet.Tests" });

            Assert.Equal(2, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && match.AllMethodsInType));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassB" && match.AllMethodsInType));
        }

        [Fact]
        public void CanSelectPluralVersions()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC), typeof(NOTTests.ClassD) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            // Note we are using "classes" here rather than "class" (we want to be nicer to our users!!)
            // Likewise you can also use "methods" and "namespaces"
            var matches = typeParser.MatchingTypesWithMethods(new[] { "classes=ClassC,ClassA", "methods=Method2" });

            // ClassC not matched as it has NO methods with the [Benchmark] attribute
            Assert.Equal(3, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && match.AllMethodsInType));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassB" && match.Methods.Any(m => m.Name == "Method2")));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassD" && match.Methods.Any(m => m.Name == "Method2")));
        }

        [Fact]
        public void EntireClassOverridesIndividualMethods()
        {
            var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC) };
            var typeParser = new TypeParser(types, ConsoleLogger.Default);

            var matches = typeParser.MatchingTypesWithMethods(new[] { "method=Method2,Method3", "class=ClassA" });

            Assert.Equal(2, matches.Count());
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassA" && match.AllMethodsInType));
            Assert.Equal(1, matches.Count(match => match.Type.Name == "ClassB" &&
                                                   match.Methods.Any(m => m.Name == "Method2") &&
                                                   match.Methods.Any(m => m.Name == "Method3")));
        }
    }
}

namespace BenchmarkDotNet.Tests
{
    public class ClassA
    {
        [Benchmark] public void Method1() { }
        [Benchmark] public void Method2() { }
    }

    public class ClassB
    {
        [Benchmark] public void Method1() { }
        [Benchmark] public void Method2() { }
        [Benchmark] public void Method3() { }
    }

    public class ClassC
    {
        // None of these methods are actually Benchmarks!!
        public void Method1() { }
        public void Method2() { }
        public void Method3() { }
    }
}

namespace BenchmarkDotNet.NOTTests
{
    public class ClassD
    {
        [Benchmark]
        public void Method1() { }
        [Benchmark]
        public void Method2() { }
    }
}
