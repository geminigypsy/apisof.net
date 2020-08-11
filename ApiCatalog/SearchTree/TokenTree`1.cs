using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ApiCatalog.SearchTree
{
    public sealed partial class TokenTree<T>
    {
        public static readonly TokenTree<T> Empty = new TokenTree<T>(TokenNode<T>.Empty);

        internal TokenTree(TokenNode<T> root)
        {
            Root = root;
        }

        public TokenNode<T> Root { get; }

        public IEnumerable<SearchResult<T>> Search(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            var remainingNodes = new Queue<Match>();
            remainingNodes.Enqueue(new Match(null, Root, Token.Empty));

            var results = new Queue<Result>();

            while (remainingNodes.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var remaining = remainingNodes.Dequeue();

                if (remaining.Term.End >= text.Length)
                {
                    var matches = remaining.AncestorsAndSelf().Reverse().Skip(1).ToArray();
                    var result = new Result(remaining.Node, matches);
                    results.Enqueue(result);
                    continue;
                }

                var prefix = text.Subsegment(remaining.Term.End);
                var node = remaining.Node;

                var i = node.BinarySearch(prefix.Subsegment(0, 1));

                if (i < 0)
                    i = ~i;

                int prefixLength;

                while (i < node.Children.Count &&
                      (prefixLength = Token.CommonPrefixLength(node.Children[i].Text, prefix)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    for (var length = 1; length <= prefixLength; length++)
                    {
                        var term = prefix.Subsegment(0, length);
                        var match = new Match(remaining, node.Children[i], term);
                        remainingNodes.Enqueue(match);
                    }

                    i++;
                }

                // This allows skipping, e.g.
                //
                //        'SqlBuil' can match 'SqlConnectionStringBuilder'
                //
                // However, it significantly slows the search.
                //
                // foreach (var candidate in candidates)
                // {
                //     cancellationToken.ThrowIfCancellationRequested();
                // 
                //     remainingNodes.Enqueue(new Remaining(remaining.Previous, candidate, remaining.Offset, remaining.Length));
                // }
            }

            while (results.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var current = results.Dequeue();
                var node = current.Node;

                foreach (var value in node.Values)
                {
                    var matches = current.Matches;
                    var searchMatches = matches.Select(t => new SearchMatch(value.Offset + t.Node.Offset, t.Term.Length));
                    yield return new SearchResult<T>(value.Item, searchMatches);
                }

                // We don't want to return hits for the entirety of our transitive children.
                // Logically, we want to return direct children only. However, there are a
                // a few special cases to consider.
                //
                // Let's assume our tree looks as follows:
                //
                //
                //   +--------+  +---+  +-----------+  +-------+  +---+  +-------------+
                //   | System +--+ . +--+ Component +--+ Model +--+ . +--+ Composition |
                //   +--------+  +---+  +-----------+  +-------+  +---+  +-------------+
                //                                                  |    +--------+
                //                                                  +----+ Design |
                //                                                       +--------+
                //
                // Consider an input like:
                //
                //   'System.Com'
                //
                // Let's say there are no values associated with Component or ComponentModel
                // but that all values are in the suffixes Composition and Design. To address
                // this case, we'll follow nodes until we have a node that has values.
                //
                // Another case to consider is partial word matches. Let's look at this tree:
                //
                //   +------+  +---+  +-----------+  +------+
                //   + Task +--+ . +--+ Completed +--+ Task |
                //   +------+  +---+  +-----------+  +------+
                //
                // Now let's say that we indexed the inputs:
                //
                //     Task.CompletedTask
                //     PrintTask.Completed
                //
                // Consider an input like:
                //
                //    'task.com'
                //
                // The node 'Completed' will have one child (PrintTask.Completed), so naively
                // our algorithm would stop walking the children. However, this would mean that
                // we only return 'PrintTask.Completed' which would be quite unexpected. Thus,
                // we generally want to follow nodes as long as they are still part of a
                // compound word.

                foreach (var child in node.Children)
                {
                    if (!node.Values.Any() || char.IsLetter(child.Text[0]))
                        results.Enqueue(new Result(child, current.Matches));
                }
            }
        }
    }
}
