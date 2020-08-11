using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ApiCatalog.SearchTree
{
    public static class TokenTreeExtensions
    {
        public static XDocument GetDgmlGraph<T>(this TokenTree<T> tree)
            where T : class
        {
            var nodeId = 1;
            var idByNode = new Dictionary<TokenNode<T>, int>();

            var nodes = tree.Root.DescendantsAndSelf();

            foreach (var node in nodes)
            {
                idByNode.Add(node, nodeId);
                nodeId++;
            }

            var edges = from n in nodes
                        from c in n.Children
                        let nId = idByNode[n]
                        let cId = idByNode[c]
                        select Tuple.Create(nId, cId, c.Text);

            const string dgmlNsp = @"http://schemas.microsoft.com/vs/2009/dgml";
            var xDocument = new XDocument();
            var xRoot = new XElement(XName.Get("DirectedGraph", dgmlNsp));
            xDocument.Add(xRoot);

            var xNodes = new XElement(XName.Get("Nodes", dgmlNsp));
            xRoot.Add(xNodes);

            foreach (var node in nodes)
            {
                var id = idByNode[node];
                var xNode = new XElement(XName.Get("Node", dgmlNsp),
                    new XAttribute("Id", id)
                );
                xNodes.Add(xNode);
            }

            var xLinks = new XElement(XName.Get("Links", dgmlNsp));
            xRoot.Add(xLinks);

            foreach (var edge in edges)
            {
                var source = edge.Item1;
                var target = edge.Item2;
                var label = edge.Item3;
                var xLink = new XElement(XName.Get("Link", dgmlNsp),
                    new XAttribute("Source", source),
                    new XAttribute("Target", target),
                    new XAttribute("Label", label)
                );
                xLinks.Add(xLink);
            }

            return xDocument;
        }
    }
}