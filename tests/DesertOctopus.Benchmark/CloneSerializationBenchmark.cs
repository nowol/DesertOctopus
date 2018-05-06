using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.Serialization;
using DesertOctopus.Utilities;
using Force.DeepCloner;
using Xunit;
using Xunit.Abstractions;

#if  NET46
using BenchmarkDotNet.Diagnostics.Windows;
using GeorgeCloney;
using NClone;
using NetSerializer;
using Serializer = NetSerializer.Serializer;
#endif

#pragma warning disable CA1052


// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedVariable

namespace DesertOctopus.Benchmark
{
    public class CloneSerializationBenchmark
    {
        private readonly ITestOutputHelper _output;

        public CloneSerializationBenchmark(ITestOutputHelper output)
        {
            _output = output;
//#if DEBUG
//            throw new Exception("Use release mode");
//#endif
        }



        [Fact]
        [Trait("Category", "Benchmark")]
        public void ProfileCloning()
        {
#if NET46
            var root = BenchmarkObjectNonISerializablerDictionary.GetNewInitialized();
            DesertOctopus.ObjectCloner.Clone(root);

            var c = Clone.ObjectGraph(root);
            var d = DeepClonerExtensions.DeepClone(root);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                DesertOctopus.ObjectCloner.Clone(root);
            }

            sw.Stop();
            _output.WriteLine(sw.Elapsed.ToString());
#endif
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void BenchmarkBenchmarkObjectNormalDictionaryCloningBenchMark()
        {
            var summary = BenchmarkRunner.Run<BenchmarkObjectNormalDictionaryCloningBenchMark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary, NullLogger.Instance);
            _output.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                _output.WriteLine(validationError.Message);
            }

            _output.WriteLine(k.First());
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void SimpleDtoWithEveryPrimitivesCloningBenchmark()
        {
            var summary = BenchmarkRunner.Run<SimpleDtoWithEveryPrimitivesCloningBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary, NullLogger.Instance);
            _output.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                _output.WriteLine(validationError.Message);
            }

            _output.WriteLine(k.First());
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void CloneBenchmarks()
        {
#if DEBUG
            throw new Exception("Use release mode");
#endif

            var benchmarks = new Dictionary<string, Type>();
            benchmarks.Add("This benchmark clones a fairly large object containing array, lists and dictionaries.  Dictionaries are serialized using the ISerializable interface.", typeof(BenchmarkObjectNormalDictionaryCloningBenchMark));
            benchmarks.Add("This benchmark clones a fairly large object containing array, lists and dictionaries.  Dictionaries are serialized as a normal class.", typeof(BenchmarkObjectNonISerializablerDictionaryCloningBenchMark));
            benchmarks.Add("This benchmark clone a normal sized object that contains all primitives types (30 properties).", typeof(SimpleDtoWithEveryPrimitivesCloningBenchmark));


            var sb = new StringBuilder();

            foreach (var kvp in benchmarks)
            {
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sb.AppendLine();
                        sb.AppendLine(kvp.Key);
                        sb.AppendLine();

                        var logger = new StreamLogger(sw);
                        var summary = BenchmarkRunner.Run(kvp.Value);

                        if (summary.ValidationErrors.Length > 0)
                        {
                            foreach (var validationError in summary.ValidationErrors)
                            {
                                _output.WriteLine(validationError.Message);
                            }
                            Assert.True(false, kvp.Key);
                        }

                        BenchmarkDotNet.Exporters.MarkdownExporter.GitHub.ExportToLog(summary, logger);
                        sw.Flush();


                        sb.AppendLine(System.Text.Encoding.UTF8.GetString(ms.ToArray()));

                    }
                }
            }

            // ### Benchmark

            var file = @"..\..\..\..\Docs\ObjectCloner.md";
            var fileContent = System.IO.File.ReadAllText(file);
            var marker = "### Benchmark";
            var pos = fileContent.IndexOf(marker, StringComparison.InvariantCultureIgnoreCase);
            if (pos == -1)
            {
                Assert.True(false, "Could not find position of " + marker);
            }

            fileContent = fileContent.Substring(0, pos + marker.Length);
            fileContent += "\r\n\r\n" + sb.ToString();

            System.IO.File.WriteAllText(file, fileContent, Encoding.UTF8);
        }
    }



    [Config(typeof(CloneBenchmarkConfiguration))]
    public class CloneBenchmarkBase
    {
        protected class CloneBenchmarkConfiguration : ManualConfig
        {
            public CloneBenchmarkConfiguration()
            {
                Add(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
                //Add(new InliningDiagnoser());
            }
        }
    }

    public class CloneBenchmarkBase<T> : CloneBenchmarkBase
        where T : class
    {
        private readonly object _objectToTest;


        public CloneBenchmarkBase()
        {
            DesertOctopus();
#if NET46
            GeorgeCloney();
            NClone();
#endif
            DeepCloner();
        }


        public CloneBenchmarkBase(object objectToTest)
        {
            _objectToTest = objectToTest;
        }

        [Benchmark]
        public object DesertOctopus()
        {
            return ObjectCloner.Clone(_objectToTest);
        }

#if NET46
        [Benchmark]
        public object GeorgeCloney()
        {
            return CloneExtension.DeepClone(_objectToTest);
        }

        [Benchmark]
        public object NClone()
        {
            return Clone.ObjectGraph(_objectToTest);
        }
#endif

        [Benchmark]
        public object DeepCloner()
        {
            return DeepClonerExtensions.DeepClone(_objectToTest);
        }
    }

    public class BenchmarkObjectNonISerializablerDictionaryCloningBenchMark : CloneBenchmarkBase<BenchmarkObjectNonISerializablerDictionary>
    {
        public BenchmarkObjectNonISerializablerDictionaryCloningBenchMark()
            : base(BenchmarkObjectNonISerializablerDictionary.GetNewInitialized())
        {
        }
    }

    public class BenchmarkObjectNormalDictionaryCloningBenchMark : CloneBenchmarkBase<BenchmarkObjectNormalDictionary>
    {
        public BenchmarkObjectNormalDictionaryCloningBenchMark()
            : base(BenchmarkObjectNormalDictionary.GetNewInitialized())
        {
        }
    }

    public class SimpleDtoWithEveryPrimitivesCloningBenchmark : CloneBenchmarkBase<ClassWithAllPrimitiveTypes>
    {
        public SimpleDtoWithEveryPrimitivesCloningBenchmark()
            : base(new ClassWithAllPrimitiveTypes())
        {
        }
    }
}
