using System;
using System.IO;
using System.Linq;

using ApiCatalog;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ConsoleApp1
{
    [MemoryDiagnoser]
    public class LookupBenchmark
    {
        public const string NamePath = @"C:\Users\immo\Downloads\Indexing\names.txt";
        public const string LookupsPath = @"C:\Users\immo\Downloads\Indexing\lookups.txt";

        private readonly string[] _lookups;
        private readonly SuffixTree _suffixTree;
        private readonly SuffixTreeBaseline _suffixTreeBaseline;

        public LookupBenchmark()
        {
            _lookups = File.ReadAllLines(LookupsPath);

            var names = File.ReadAllLines(NamePath);

            var suffixTreeBuilder = new SuffixTreeBuilder();
            for (var i = 0; i < names.Length; i++)
                suffixTreeBuilder.Add(names[i], i);

            using var ms = new MemoryStream();
            suffixTreeBuilder.WriteSuffixTree(ms);
            var buffer = ms.ToArray();

            _suffixTree = SuffixTree.Load(buffer);
            _suffixTreeBaseline = SuffixTreeBaseline.Load(buffer);
        }

        [Benchmark(Baseline = true)]
        public void SuffixTreeBaseline_Lookup()
        {
            foreach (var lookup in _lookups)
                _suffixTreeBaseline.Lookup(lookup);
        }

        [Benchmark]
        public void SuffixTree_Lookup()
        {
            foreach (var lookup in _lookups)
                _suffixTree.Lookup(lookup);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<LookupBenchmark>();
        }

        private static void GenerateLookups()
        {
            var fullNames = File.ReadAllLines(LookupBenchmark.NamePath).ToList();

            var random = new Random();
            var lookups = new string[50];

            for (var i = 0; i < lookups.Length; i++)
            {
                var index = random.Next(0, fullNames.Count - 1);
                var fullName = fullNames[index];
                fullNames.RemoveAt(index);
                var tokens = Tokenizer.Tokenize(fullName).ToList();
            TryAnotherStart:
                var start = random.Next(0, tokens.Count - 1);
                if (tokens[start] == ".")
                    goto TryAnotherStart;

            TryAnotherEnd:
                var end = random.Next(start, tokens.Count - 1);
                if (tokens[end] == ".")
                    goto TryAnotherEnd;

                lookups[i] = string.Concat(tokens.Skip(start).Take(end - start + 1));
            }

            File.WriteAllLines(LookupBenchmark.LookupsPath, lookups);
        }
    }
}