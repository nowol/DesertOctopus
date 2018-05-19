using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.Serialization;
using DesertOctopus.Utilities;
using Xunit;
using Xunit.Abstractions;

#if  NET46
using BenchmarkDotNet.Diagnostics.Windows;
using NetSerializer;
using Serializer = NetSerializer.Serializer;
#endif

#pragma warning disable CS0414, SA1401, SA1025, SA1308, SA1400, SA1129, SA1001, SA1028, SA1307, CS0162, SA1028, CS1052, CA1052

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedVariable


namespace DesertOctopus.Benchmark
{
    public class AllowNonOptimized : ManualConfig
    {
        public AllowNonOptimized()
        {
            //Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

            Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
            Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
        }
    }

    public class SerializationBenchmarkTest
    {
        private readonly ITestOutputHelper _output;

        public SerializationBenchmarkTest(ITestOutputHelper output)
        {
            _output = output;
//#if DEBUG
//            throw new Exception("Use release mode");
//#endif
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ForOptimization()
        {
            //ParameterExpression outputStream = Expression.Parameter(typeof(Stream)), objTracking = Expression.Parameter(typeof(SerializerObjectTracker));
            //var notTrackedExpressions = new List<Expression>();
            //var variables = new List<ParameterExpression>();

            //var b = Expression.Parameter(typeof(byte));

            //variables.Add(b);

            //notTrackedExpressions.Add(Expression.Assign(b, Expression.Constant((byte)0)));
            ////notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, b, objTracking));

            //notTrackedExpressions.Add(Expression.Call(outputStream, StreamMih.WriteByte(), b ));


            //var block = Expression.Block(variables, notTrackedExpressions);

            //new IntArraySerializationBenchmark();

        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ProfileSerialization()
        {
            var root = BenchmarkObjectNormalDictionary.GetNewInitialized();
            var krakenBytes = DesertOctopus.KrakenSerializer.Serialize(root);
            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(root);
            System.IO.File.WriteAllBytes(@"d:\z.bin", krakenBytes);

            DesertOctopus.KrakenSerializer.Deserialize<BenchmarkObjectNormalDictionary>(krakenBytes);
            DesertOctopus.ObjectCloner.Clone(root);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                //krakenBytes = DesertOctopus.KrakenSerializer.Serialize(root);
                DesertOctopus.KrakenSerializer.Deserialize<BenchmarkObjectNormalDictionary>(krakenBytes);
                //DesertOctopus.ObjectCloner.Clone(root);
            }

            sw.Stop();
            _output.WriteLine(sw.Elapsed.ToString());
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ProfileSerialization2()
        {
            var instance = Enumerable.Range(0, 100).ToArray();
            var krakenBytes = DesertOctopus.KrakenSerializer.Serialize(instance);
            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(instance);
            System.IO.File.WriteAllBytes(@"d:\z.bin", krakenBytes);

            //DesertOctopus.KrakenSerializer.Deserialize<int[]>(krakenBytes);
            //DesertOctopus.ObjectCloner.Clone(instance);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                krakenBytes = DesertOctopus.KrakenSerializer.Serialize(instance);
                //DesertOctopus.KrakenSerializer.Deserialize<int[]>(krakenBytes);
                //DesertOctopus.ObjectCloner.Clone(instance);
            }

            sw.Stop();
            _output.WriteLine(sw.Elapsed.ToString());
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void PrintComplexObjectSerializationSizes()
        {
            var b = new ComplexObjectSerializationBenchmark();
            _output.WriteLine("JsonSerialization: {0}", b.JsonSerialization().Length);
#if NET46
            _output.WriteLine("OmniSerialization: {0}", b.OmniSerialization().Length);
            _output.WriteLine("NetSerializerSerialization: {0}", b.NetSerializerSerialization().Length);
#endif
            _output.WriteLine("KrakenSerialization: {0}", b.KrakenSerialization().Length);
            _output.WriteLine("BinaryFormatterSerialization: {0}", b.BinaryFormatterSerialization().Length);
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ComplexObjectSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<ComplexObjectSerializationBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary, NullLogger.Instance);
            _output.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                _output.WriteLine(validationError.Message);
            }

            PrintComplexObjectSerializationSizes();

            _output.WriteLine(k.First());
        }



        [Fact]
        [Trait("Category", "Benchmark")]
        public void KrakenBenchmarks()
        {
#if DEBUG
            throw new Exception("Use release mode");
#endif
//#if NET46
//            return; // we only benchmark .Net core
//#endif

            var benchmarks = new Dictionary<string, Type>();
            benchmarks.Add("This benchmark serialize and deserialize a fairly large object containing array, lists and dictionaries.", typeof(ComplexObjectSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize a normal sized object that contains all primitives types.", typeof(SimpleDtoWithEveryPrimitivesSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an array of 100000 ints.", typeof(IntArraySerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an array of 100000 doubles.", typeof(DoubleArraySerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an array of 100000 decimals.", typeof(DecimalArraySerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an array of 100000 DateTimes.", typeof(DateTimeArraySerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.", typeof(DictionaryIntIntSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.", typeof(DictionaryStringIntSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize a string of 1000 characters.", typeof(StringSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize a large struct.", typeof(LargeStructSerializationBenchmark));
            benchmarks.Add("This benchmark serialize and deserialize a small class used by the Wire project.", typeof(WireSmallObjectSerializationBenchmark));


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
#if NET46
            var file = @"..\..\..\..\..\Docs\KrakenSerializer.md";
#else
            var file = @"..\..\..\..\..\Docs\KrakenSerializerCore.md";
#endif
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

        [Fact]
        [Trait("Category", "Benchmark")]
        public void SimpleDtoWithEveryPrimitivesSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<SimpleDtoWithEveryPrimitivesSerializationBenchmark>();

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
        public void IntArraySerializationBenchmark()
        {
            var ii = new IntArraySerializationBenchmark();

            var summary = BenchmarkRunner.Run<IntArraySerializationBenchmark>();

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
        public void DoubleArraySerializationBenchmark()
        {
            var ii = new DoubleArraySerializationBenchmark();


            var summary = BenchmarkRunner.Run<DoubleArraySerializationBenchmark>();

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
        public void DecimalArraySerializationBenchmark()
        {
            var ii = new DecimalArraySerializationBenchmark();

            var summary = BenchmarkRunner.Run<DecimalArraySerializationBenchmark>(new AllowNonOptimized());

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
        public void DateTimeArraySerializationBenchmark()
        {
            var ii = new DateTimeArraySerializationBenchmark();

            var summary = BenchmarkRunner.Run<DateTimeArraySerializationBenchmark>(new AllowNonOptimized());

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
        public void DictionaryStringIntSerializationBenchmark()
        {
            var ii = new DictionaryStringIntSerializationBenchmark();

            var summary = BenchmarkRunner.Run<DictionaryStringIntSerializationBenchmark>();

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
        public void DictionaryIntIntSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<DictionaryIntIntSerializationBenchmark>();

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
        public void StringSerializationBenchmark()
        {
            var ii = new StringSerializationBenchmark();

            var summary = BenchmarkRunner.Run<StringSerializationBenchmark>();

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
        public void LargeStructSerializationBenchmark()
        {
            var ii = new LargeStructSerializationBenchmark();

            var summary = BenchmarkRunner.Run<LargeStructSerializationBenchmark>();

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
        public void WireSmallObjectSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<WireSmallObjectSerializationBenchmark>();

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
        public void TwoDimIntArraySerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<TwoDimIntArraySerializationBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary, NullLogger.Instance);
            _output.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                _output.WriteLine(validationError.Message);
            }

            _output.WriteLine(k.First());
        }
    }

    public class ComplexObjectSerializationBenchmark : SerializationBenchmarkBase<BenchmarkObjectNormalDictionary>
    {
        public ComplexObjectSerializationBenchmark()
            : base(BenchmarkObjectNormalDictionary.GetNewInitialized())
        {
        }
    }

    public class SimpleDtoWithEveryPrimitivesSerializationBenchmark : SerializationBenchmarkBase<ClassWithAllPrimitiveTypes>
    {
        public SimpleDtoWithEveryPrimitivesSerializationBenchmark()
            : base(new ClassWithAllPrimitiveTypes())
        {
        }
    }

    public class IntArraySerializationBenchmark : SerializationBenchmarkBase<int[]>
    {
        public IntArraySerializationBenchmark()
            : base(IntArraySerializationBenchmark.Array)
        {
        }

        public static int[] Array = Enumerable.Range(0, 100000).Select(x => 55555).ToArray();
        //public static int[] Array = Enumerable.Range(0, 100000).Select(x => int.MaxValue).ToArray();
    }

    public class DoubleArraySerializationBenchmark : SerializationBenchmarkBase<double[]>
    {
        public DoubleArraySerializationBenchmark()
            : base(DoubleArraySerializationBenchmark.Array)
        {
        }

        public static double[] Array = Enumerable.Range(0, 100000).Select(x => 5456465.564D).ToArray();
    }

    public class DecimalArraySerializationBenchmark : SerializationBenchmarkBase<decimal[]>
    {
        public DecimalArraySerializationBenchmark()
            : base(DecimalArraySerializationBenchmark.Array)
        {
        }

        //public static decimal[] Array = Enumerable.Range(0, 100000).Select(x => Decimal.MaxValue).ToArray();
        public static decimal[] Array = Enumerable.Range(0, 100000).Select(x => 5456465.564M).ToArray();
    }

    public class DateTimeArraySerializationBenchmark : SerializationBenchmarkBase<DateTime[]>
    {
        public DateTimeArraySerializationBenchmark()
            : base(DateTimeArraySerializationBenchmark.Array)
        {
        }

        public static DateTime[] Array = Enumerable.Range(0, 100000).Select(x => DateTime.UtcNow).ToArray();
    }

    public class DictionaryStringIntSerializationBenchmark : SerializationBenchmarkBase<Dictionary<string, int>>
    {
        public DictionaryStringIntSerializationBenchmark()
            : base(DictionaryStringIntSerializationBenchmark.Dict)
        {

        }

        static DictionaryStringIntSerializationBenchmark()
        {
            for (int i = 0; i < 100000; i++)
            {
                Dict.Add(i.ToString(), i);
            }
        }

        public static Dictionary<string, int> Dict = new Dictionary<string, int>();
    }

    public class DictionaryIntIntSerializationBenchmark : SerializationBenchmarkBase<Dictionary<int, int>>
    {
        public DictionaryIntIntSerializationBenchmark()
            : base(DictionaryIntIntSerializationBenchmark.Dict)
        {

        }

        static DictionaryIntIntSerializationBenchmark()
        {
            for (int i = 0; i < 100000; i++)
            {
                Dict.Add(int.MaxValue - i, int.MaxValue);
            }
        }

        public static Dictionary<int, int> Dict = new Dictionary<int, int>();
    }

    public class StringSerializationBenchmark : SerializationBenchmarkBase<ObjectWrapper<string>>
    {
        public StringSerializationBenchmark()
            : base(StringSerializationBenchmark.Str)
        {
        }

        public static ObjectWrapper<string> Str = new ObjectWrapper<string>() { Value = new string('c', 1000) };
    }

    public class LargeStructSerializationBenchmark : SerializationBenchmarkBase<ObjectWrapper<LargeStruct>>
    {
        public LargeStructSerializationBenchmark()
            : base(LargeStructSerializationBenchmark.Str)
        {

        }

        public static ObjectWrapper<LargeStruct> Str = new ObjectWrapper<LargeStruct> { Value = LargeStruct.Create() };
    }

    public class WireSmallObjectSerializationBenchmark : SerializationBenchmarkBase<WireSmallObject>
    {
        public WireSmallObjectSerializationBenchmark()
            : base(WireSmallObjectSerializationBenchmark.Str)
        {

        }

        public static WireSmallObject Str = new WireSmallObject { StringProp = "hello", IntProp = 123, GuidProp = Guid.NewGuid(), DateProp = DateTime.Now };
    }

    [Serializable]
    public class WireSmallObject
    {
        public string StringProp { get; set; }      //using the text "hello"
        public int IntProp { get; set; }            //123
        public Guid GuidProp { get; set; }          //Guid.NewGuid()
        public DateTime DateProp { get; set; }      //DateTime.Now
    }


    [Serializable]
    public struct LargeStruct
    {
        ulong m_val1;
        ulong m_val2;
        ulong m_val3;
        ulong m_val4;

        public static LargeStruct Create()
        {
            var ls = new LargeStruct();
            ls.m_val1 = ulong.MaxValue;
            ls.m_val2 = ulong.MaxValue;
            ls.m_val3 = ulong.MaxValue;
            ls.m_val4 = ulong.MaxValue;
            return ls;
        }
    }

    public class TwoDimIntArraySerializationBenchmark : SerializationBenchmarkBase<int[,]>
    {
        public TwoDimIntArraySerializationBenchmark()
            : base(TwoDimIntArraySerializationBenchmark.Array, false, false)
        {
        }

        public static int[,] Array = CreateArray();

        private static int[,] CreateArray()
        {
            var arr = new int[10,10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    arr[i, j] = i;
                }
            }

            return arr;
        }
    }

    [Serializable]
    public class ObjectWrapper<T>
    {
        public T Value { get; set; }
    }

    //public class Config : ManualConfig
    //{


    //    public Config()
    //    {
    //        Add(new MemoryDiagnoser());
    //        Add(new InliningDiagnoser());
    //        Add(new TagColumn("Size (bytes)", name => name.Substring(3)));
    //    }
    //}

    public interface ISerializationBenchmark
    {
        int KrakenSizeBytes { get; set; }
        int BinaryFormatterSizeBytes { get; set; }
        int OmniSizeBytes { get; set; }
        int JsonSizeBytes { get; set; }
    }

    [Config(typeof(BenchmarkConfiguration))]
    public class SerializationBenchmarkBase
    {
        protected class BenchmarkConfiguration : ManualConfig
        {
            public BenchmarkConfiguration()
            {
                Add(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
                //Add(new InliningDiagnoser());
                Add(new TagColumn("Size (bytes)", GetSizeFromName));
            }

            private string GetSizeFromName(string name)
            {
                if (System.IO.File.Exists(name))
                {
                    return System.IO.File.ReadAllText(name);
                }
                return String.Empty;
            }
        }
    }

    public class SerializationBenchmarkBase<T> : SerializationBenchmarkBase
        where T : class
    {
        //private class Config : BenchmarkDotNet.Configs.ManualConfig
        //{
        //    public Config()
        //    {
        //        //Add(Job.Dry);
        //        //// You can add custom tags per each method using Columns
        //        //Add(new TagColumn("Foo or Bar", name => name.Substring(0, 3)));
        //        //Add(new TagColumn("Number", name => name.Substring(3)));
        //        BenchmarkDotNet.Configs.DefaultConfig

//        Add(Job.AllJits);
//        Add(Job.LegacyX64, Job.RyuJitX64);
//        Add(Job.Default.With(Mode.SingleRun).WithProcessCount(1).WithWarmupCount(1).WithTargetCount(1));
//        Add(Job.Default.With(Framework.V40).With(Runtime.Mono).With(Platform.X64));

//    }
//}

        private BinaryFormatter bf = new BinaryFormatter();
        public byte[] krakenBytes;
        public byte[] krakenBytesWithOmittedRootType;
        public byte[] bfBytes;
#if NET46
        private Orckestra.OmniSerializer.Serializer omni = new Orckestra.OmniSerializer.Serializer();
        public byte[] omniBytes;
        private NetSerializer.Serializer netSerializer;
        public byte[] netBytes;
#endif
        public byte[] wireBytes;
        protected object obj;
        private string json = String.Empty;
        private Wire.Serializer wireSerializer = new Wire.Serializer();

        private SerializationOptions _serializationOptions;

        public SerializationBenchmarkBase(object objectToTest, bool benchmarkOmni = true, bool benchmarkSS = true)
        {
            BenchmarkOmni = benchmarkOmni;
            BenchmarkSs = benchmarkSS;
            obj = objectToTest;
            if (benchmarkSS)
            {
                json = ServiceStack.Text.JsonSerializer.SerializeToString(obj);
                System.IO.File.WriteAllText("JsonSerialization", json.Length.ToString());
            }

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                bfBytes = ms.ToArray();
            }

#if NET46
            using (var ms = new MemoryStream())
            {
                if (benchmarkOmni)
                {
                    omni.SerializeObject(ms, obj);
                    omniBytes = ms.ToArray();
        }
    }

            netSerializer = CreateNetSerializer();

            using (var ms = new MemoryStream())
            {
                netSerializer.Serialize(ms, objectToTest);
                netBytes = ms.ToArray();
                ms.Position = 0;
                object o;
                netSerializer.Deserialize(ms, out o);
            }
#endif

            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(obj);
            DesertOctopus.KrakenSerializer.Deserialize<T>(krakenBytes);

            _serializationOptions = new SerializationOptions { OmitRootTypeName = true };
            krakenBytesWithOmittedRootType = DesertOctopus.KrakenSerializer.Serialize(obj, _serializationOptions);
            DesertOctopus.KrakenSerializer.Deserialize<T>(krakenBytesWithOmittedRootType, _serializationOptions);

            if (benchmarkSS)
            {
                ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(json);
            }

            System.IO.File.WriteAllText("KrakenSerialization", krakenBytes.Length.ToString());
            System.IO.File.WriteAllText("KrakenSerializationWithOmittedRootType", krakenBytesWithOmittedRootType.Length.ToString());
            System.IO.File.WriteAllText("BinaryFormatterSerialization", bfBytes.Length.ToString());
#if NET46
            System.IO.File.WriteAllText("OmniSerialization", omniBytes.Length.ToString());
            System.IO.File.WriteAllText("NetSerializerSerialization", netBytes.Length.ToString());
#endif

            //using (var ms = new MemoryStream())
            //{
            //    wireSerializer.Serialize(obj, ms);
            //    wireBytes = ms.ToArray();
            //    ms.Position = 0;
            //    wireSerializer.Deserialize<T>(ms);
            //}
        }

        public bool BenchmarkOmni { get; set; }
        public bool BenchmarkSs { get; set; }


        [Benchmark]
        public byte[] JsonSerialization()
        {
            if (!BenchmarkSs)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                ServiceStack.Text.JsonSerializer.SerializeToStream(obj, ms);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public T JsonDeserialization()
        {
            if (!BenchmarkSs)
            {
                return null;
            }

            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(json);
        }

#if NET46

        private Serializer CreateNetSerializer()
        {
            var types = new HashSet<Type>();
            types.Add(typeof(Dictionary<string, string>));

            BuildTypeList(types, typeof(T));

            foreach (var t in types.ToList())
            {
                BuildTypeList(types,
                              typeof(List<>).MakeGenericType(t));
            }

            return new NetSerializer.Serializer(types);
        }

        private void BuildTypeList(HashSet<Type> types, Type type)
        {
            if (types.Contains(type))
            {
                return;
            }

            types.Add(type);

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    BuildTypeList(types, arg);
                }
            }

            if (type.IsArray)
            {
                BuildTypeList(types, type.GetElementType());
            }

            foreach (var prop in type.GetProperties())
            {
                BuildTypeList(types,
                              prop.PropertyType);
            }
        }

        [Benchmark]
        public byte[] OmniSerialization()
        {
            if (!BenchmarkOmni)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                omni.SerializeObject(ms, obj);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public T OmniDeserialization()
        {
            if (!BenchmarkOmni)
            {
                return null;
            }

            using (var ms = new MemoryStream(omniBytes))
            {
                return omni.Deserialize(ms) as T;
            }
        }

        [Benchmark]
        public byte[] NetSerializerSerialization()
        {
            using (var ms = new MemoryStream())
            {
                netSerializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public T NetSerializerDeserialization()
        {
            using (var ms = new MemoryStream(netBytes))
            {
                return netSerializer.Deserialize(ms) as T;
            }
        }
#endif

        [Benchmark]
        public byte[] KrakenSerialization()
        {
            return DesertOctopus.KrakenSerializer.Serialize(obj);
        }

        [Benchmark]
        public T KrakenDeserialization()
        {
            return DesertOctopus.KrakenSerializer.Deserialize<T>(krakenBytes);
        }

        [Benchmark]
        public byte[] KrakenSerializationWithOmittedRootType()
        {
            return DesertOctopus.KrakenSerializer.Serialize(obj, _serializationOptions);
        }

        [Benchmark]
        public T KrakenDeserializationWithOmittedRootType()
        {
            return DesertOctopus.KrakenSerializer.Deserialize<T>(krakenBytesWithOmittedRootType, _serializationOptions);
        }

        [Benchmark]
        public byte[] BinaryFormatterSerialization()
        {
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public T BinaryFormatterDeserialization()
        {
            using (var ms = new MemoryStream(bfBytes))
            {
                return bf.Deserialize(ms) as T;
            }
        }

        //[Benchmark]
        //public byte[] WireSerialization()
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        wireSerializer.Serialize(obj, ms);
        //        return ms.ToArray();
        //    }
        //}

        //[Benchmark]
        //public T WireDeserialization()
        //{
        //    using (var ms = new MemoryStream(wireBytes))
        //    {
        //        return wireSerializer.Deserialize<T>(ms);
        //    }
        //}
    }
}
