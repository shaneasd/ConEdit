using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Graph
    {
        private class Vertex<T>
        {
            public Vertex(T payload)
            {
                Payload = payload;
            }
            public T Payload;
            public List<Vertex<T>> Parents = new List<Vertex<T>>();
            public List<Vertex<T>> Children = new List<Vertex<T>>();
        }

        private static Either<List<T>, string> OrderAllConnected<T>(IEnumerable<T> vertices, Func<T, IEnumerable<T>> getParents, Func<T, IEnumerable<T>> getChildren)
        {
            Dictionary<T, Vertex<T>> vertexMap = vertices.ToDictionary(v => v, v => new Vertex<T>(v));
            Vertex<T> convert(T vertex) => vertexMap[vertex];
            foreach (var vertex in vertexMap.Values)
            {
                vertex.Parents = getParents(vertex.Payload).Select(convert).ToList();
                vertex.Children = getChildren(vertex.Payload).Select(convert).ToList();
            }

            //https://en.wikipedia.org/wiki/Topological_sorting
            //Kahn's algorithm. Modified to return unconnected graphs in separate lists.

            //L ← Empty list that will contain the sorted elements
            List<T> L = new List<T>();
            //S ← Set of all nodes with no incoming edge
            HashSet<Vertex<T>> S = new HashSet<Vertex<T>>(vertexMap.Values.Where(v => !v.Parents.Any()));

            while (S.Any())
            {
                //while S is non - empty do
                while (S.Any())
                {
                    //remove a node n from S
                    var n = S.First();
                    S.Remove(n);
                    //add n to tail of L
                    L.Add(n.Payload);

                    //for each node m with an edge e from n to m do
                    while (n.Children.Any())
                    {
                        var m = n.Children[n.Children.Count - 1];
                        //remove edge e from the graph
                        n.Children.RemoveAt(n.Children.Count - 1);
                        m.Parents.Remove(n);

                        //if m has no other incoming edges then
                        if (!m.Parents.Any())
                        {
                            //insert m into S
                            S.Add(m);
                        }
                    }
                }
            }
            //if graph has edges then
            if (vertexMap.Values.Any(v => v.Children.Any()))
                //return error(graph has at least one cycle)
                return "graph has at least one cycle";
            else
            {
                //return L(a topologically sorted order)
                return L;
            }
        }

        private static List<HashSet<T>> Split<T>(IEnumerable<T> vertices, Func<T, IEnumerable<T>> getParents, Func<T, IEnumerable<T>> getChildren)
        {
            Dictionary<T, Vertex<T>> vertexMap = vertices.ToDictionary(v => v, v => new Vertex<T>(v));
            Vertex<T> convert(T vertex) => vertexMap[vertex];
            foreach (var vertex in vertexMap.Values)
            {
                vertex.Parents = getParents(vertex.Payload).Select(convert).ToList();
                vertex.Children = getChildren(vertex.Payload).Select(convert).ToList();
            }

            List<HashSet<T>> result = new List<HashSet<T>>();
            HashSet<T> V = vertices.ToHashSet();
            while (V.Any())
            {
                var x = V.First();
                HashSet<T> K = new HashSet<T>();
                void explore(Vertex<T> n)
                {
                    if ( V.Remove(n.Payload))
                    {
                        K.Add(n.Payload);
                        foreach (var v in n.Parents.Concat(n.Children))
                        {
                            explore(v);
                        }
                    }
                }
                explore(vertexMap[x]);
                result.Add(K);
            }

            return result;
        }

        public static Either<List<List<T>>, string> Order<T>(IEnumerable<T> vertices, Func<T, IEnumerable<T>> getParents, Func<T, IEnumerable<T>> getChildren)
        {
            var split = Split(vertices, getParents, getChildren);
            List<List<T>> result = new List<List<T>>();
            foreach (var x in split.Select(set => OrderAllConnected(set, getParents, getChildren)))
            {
                string error = null;
                x.Do(list => result.Add(list), e => error = e);
                if (error != null)
                    return error;
            }
            return result;
        }
    }
}
