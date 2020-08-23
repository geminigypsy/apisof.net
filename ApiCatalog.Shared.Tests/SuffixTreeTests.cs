using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xunit;

namespace ApiCatalog.Shared.Tests
{
    public class SuffixTreeTests
    {
        [Fact]
        public void SuffixTree_Empty_ReturnsEmpty()
        {
            Matches(
                keysAndValuesText: @"
                ",
                input: @"
                    nothing
                ",
                expectedValuesText: @"
                "
            );
        }

        [Fact]
        public void SuffixTree_Single_ReturnsElement()
        {
            Matches(
                keysAndValuesText: @"
                    nothing
                ",
                input: @"
                    nothing
                ",
                expectedValuesText: @"
                    1
                "
            );
        }

        [Fact]
        public void SuffixTree_Suffix_ReturnsElement()
        {
            Matches(
                keysAndValuesText: @"
                    System.Text.StringBuilder
                    System.Collections.ImmutableArray.Builder
                ",
                input: @"
                    Builder
                ",
                expectedValuesText: @"
                    1
                    2
                "
            );
        }

        private void Matches(string keysAndValuesText, string input, string expectedValuesText)
        {
            var keysAndValues = GetKeysAndValues(keysAndValuesText);
            var expectedValues = GetValues(expectedValuesText);

            var suffixTree = SuffixTree.Create(keysAndValues);
            var actualValues = suffixTree.Lookup(input.Trim())
                                         .ToArray()
                                         .Select(t => t.Value)
                                         .ToArray();
            Array.Sort(actualValues);

            Assert.Equal(expectedValues, actualValues);

            static IEnumerable<KeyValuePair<string, int>> GetKeysAndValues(string text)
            {
                var index = 1;
                string line;
                using var reader = new StringReader(text);
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        yield return KeyValuePair.Create(line, index);
                        index++;
                    }
                }
            }

            static IEnumerable<int> GetValues(string text)
            {
                string line;
                using var reader = new StringReader(text);
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                        yield return int.Parse(line);
                }
            }
        }
    }

    //public class TokenizationTests
    //{
    //    [Theory]
    //    [InlineData("", new string[0])]
    //    [InlineData("l", new[] { "l" })]
    //    [InlineData("U", new[] { "U" })]
    //    [InlineData("_", new[] { "_" })]
    //    [InlineData(".", new[] { "." })]
    //    [InlineData("<", new[] { "<" })]
    //    [InlineData("0", new[] { "0" })]
    //    [InlineData("42", new[] { "42" })]
    //    [InlineData("lower", new[] { "lower" })]
    //    [InlineData("lower1", new[] { "lower1" })]
    //    [InlineData("lower12", new[] { "lower12" })]
    //    [InlineData("Upper", new[] { "Upper" })]
    //    [InlineData("Upper1", new[] { "Upper1" })]
    //    [InlineData("Upper12", new[] { "Upper12" })]
    //    [InlineData("ALLCAPS", new[] { "ALLCAPS" })]
    //    [InlineData("ALLCAPS1", new[] { "ALLCAPS1" })]
    //    [InlineData("ALLCAPS12", new[] { "ALLCAPS12" })]
    //    [InlineData("ALL_CAPS", new[] { "ALL", "_", "CAPS" })]
    //    [InlineData("PascalCase", new[] { "Pascal", "Case" })]
    //    [InlineData("camelCase", new[] { "camel", "Case" })]
    //    [InlineData("_private", new[] { "_private" })]
    //    [InlineData("_Private", new[] { "_Private" })]
    //    [InlineData("value_", new[] { "value_" })]
    //    [InlineData("value__", new[] { "value__" })]
    //    [InlineData("value__.test", new[] { "value__", ".", "test" })]
    //    [InlineData("System.IO", new[] { "System", ".", "IO" })]
    //    [InlineData("System.IO.Stream", new[] { "System", ".", "IO", ".", "Stream" })]
    //    [InlineData("First.Second(Test)", new[] { "First", ".", "Second" })]
    //    [InlineData("System.Action<T>.Invoke()", new[] { "System", ".", "Action<T>", ".", "Invoke" })]
    //    [InlineData("Action<T>.Invoke()", new[] { "Action<T>", ".", "Invoke" })]
    //    [InlineData("String.Item[int]", new[] { "String", ".", "Item" })]
    //    [InlineData("System.IComparable", new[] { "System", ".", "I", "Comparable" })]
    //    [InlineData("Sql.SQLTicksPerHour", new[] { "Sql", ".", "SQL", "Ticks", "Per", "Hour" })]
    //    public void Tokenization_Works(string text, string[] expectedTokens)
    //    {
    //        var actualTokens = ApiTokenization.Tokenize(text).Select(t => t.Text).ToArray();
    //        Assert.Equal(expectedTokens, actualTokens);
    //    }
    //}
    //
    //public class TokenTreeTests
    //{
    //    [Theory]
    //    [InlineData("System.Text.StringBuilder", "S.T.SB")]
    //    [InlineData("System.Text.StringBuilder", "StringBuilder")]
    //    [InlineData("System.Text.StringBuilder", "Builder")]
    //    [InlineData("System.Collections.IComparable", "IC")]
    //    [InlineData("System.Collections.IComparable", "IComparable")]
    //    [InlineData("System.Collections.IComparable", "Comparable")]
    //    [InlineData("System.IO", "S.IO")]
    //    [InlineData("System.IO", "IO")]
    //    [InlineData("System.IO.Stream", "S.I.S")]
    //    public void TokenTree_Matches(string tree, string term)
    //    {
    //        var wordTree = TokenTree.Create(new[] { tree }, ApiTokenization.Tokenizer);

    //        var result = wordTree.Search(term).Single();
    //        Assert.Equal(result.Item, tree);

    //        var resultLowerCase = wordTree.Search(term.ToLowerInvariant()).Single();
    //        Assert.Equal(resultLowerCase.Item, tree);

    //        var resultUpperCase = wordTree.Search(term.ToUpperInvariant()).Single();
    //        Assert.Equal(resultUpperCase.Item, tree);
    //    }

    //    [Fact]
    //    public void TokenTree_Matches_Intermediaries()
    //    {
    //        var items = new[]
    //        {
    //            "Microsoft.AspNetCore.Bar",
    //            "Microsoft.AspNetCore.Foo"
    //        };

    //        var wordTree = TokenTree.Create(items, ApiTokenization.Tokenizer);
    //        var results = wordTree.Search("Microsoft.Asp").Select(r => r.Item).ToArray();

    //        Assert.Equal(items, results);
    //    }

    //    [Fact]
    //    public void TokenTree_Matches_Intermediaries_ExceptWhenItHasValues()
    //    {
    //        var items = new[]
    //        {
    //            "Microsoft.AspNetCore",
    //            "Microsoft.AspNetCore.Bar",
    //            "Microsoft.AspNetCore.Foo"
    //        };

    //        var wordTree = TokenTree.Create(items, ApiTokenization.Tokenizer);
    //        var result = wordTree.Search("Microsoft.Asp").Select(r => r.Item).Single();

    //        Assert.Equal("Microsoft.AspNetCore", result);
    //    }

    //    [Fact]
    //    public void TokenTree_Matches_IntermediaryWords()
    //    {
    //        var items = new[]
    //        {
    //            "System.Threading.Tasks.Task.CompletedTask",
    //            "Windows.Graphics.Printing.PrintTask.Completed"
    //        };

    //        var wordTree = TokenTree.Create(items, ApiTokenization.Tokenizer);
    //        var results = wordTree.Search("task.com").Select(r => r.Item).ToArray();
    //        Array.Sort(results);

    //        Assert.Equal(items, results);
    //    }
    //}
}
