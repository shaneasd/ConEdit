using Conversation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace ConversationEditor
{
    class NodeLayout
    {
        public static void LayoutNodes<TNode>(IConversationEditorControlData<TNode, TransitionNoduleUIInfo> currentFile)
            where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
        {
            LayoutNodes1(currentFile);
        }

        public static void LayoutNodes2<TNode>(IConversationEditorControlData<TNode, TransitionNoduleUIInfo> currentFile)
            where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
        {
            IEnumerable<Output> TopConnectors(TNode n) => n.Data.Connectors.Where(c => c.Definition.Position.ForPosition(top: () => true, () => false, () => false, () => false));
            IEnumerable<Output> BottomConnectors(TNode n) => n.Data.Connectors.Where(c => c.Definition.Position.ForPosition(top: () => false, bottom: () => true, () => false, () => false));
            IEnumerable<TNode> TopConnections(TNode n) => TopConnectors(n).SelectMany(c => c.Connections).Select(c => currentFile.GetNode(c.Parent.NodeId));
            IEnumerable<TNode> BottomConnections(TNode n) => BottomConnectors(n).SelectMany(c => c.Connections).Select(c => currentFile.GetNode(c.Parent.NodeId));

            var ordered = Graph.Order(currentFile.Nodes, TopConnections, BottomConnections);

            ordered.Do(list =>
            {
                List<TNode> nodes = list[1];
                PointF centroid = new PointF(nodes.Average(n => n.Renderer.Area.Center().X), nodes.Average(n => n.Renderer.Area.Center().Y));
                float repulsion = 1e3f;

                List<ValueTuple<TNode, PointF>> movement = new List<ValueTuple<TNode, PointF>>();

                foreach (var node in nodes.Skip(1))
                {
                    PointF origin = node.Renderer.Area.Center();
                    PointF force = PointF.Empty;

                    //Attraction
                    PointF N = PointF.Empty;
                    int count = 0;
                    foreach (var n in TopConnections(node).Concat(BottomConnections(node)))
                    {
                        PointF pos = n.Renderer.Area.Center();
                        //var offset = pos.Take(origin).ScaleBy(attraction);
                        //offset.X = offset.X * Math.Abs(offset.X);
                        //offset.Y = offset.Y * Math.Abs(offset.Y);
                        //force = force.Plus(offset);
                        N = N.Plus(pos);
                        count++;
                    }
                    N = N.ScaleBy(1.0f / count);
                    force = N.Take(origin).ScaleBy(0.5f);

                    //Repulsion
                    foreach (var n in nodes)
                    {
                        if (n != node)
                        {
                            PointF asd = n.Renderer.Area.Center().Take(origin);
                            float length2 = (asd.X * asd.X + asd.Y * asd.Y);
                            asd = asd.ScaleBy((float)Math.Pow(length2, -1)).ScaleBy(repulsion);
                            //asd.X = asd.X * Math.Abs(asd.X);
                            //asd.Y = asd.Y * Math.Abs(asd.Y);
                            force = force.Take(asd);
                        }
                    }

                    //Gravity
                    foreach (var n in nodes)
                    {
                        foreach (var top in TopConnections(n))
                        {
                            var toppos = top.Renderer.Area.Center();
                            var npos = n.Renderer.Area.Center();
                            var dif = npos.Take(toppos);
                            var diflength = (float)Math.Sqrt(dif.X * dif.X + dif.Y * dif.Y);
                            var dot = dif.Y;
                            float angle = (float)Math.Acos(dot / diflength);

                            force.Y = force.Y + 2.0e-0f * angle;
                        }

                        movement.Add((node, force.Plus(origin)));
                    }
                }

                currentFile.Move(movement);

            }, error => { });
        }

        public static void LayoutNodes1<TNode>(IConversationEditorControlData<TNode, TransitionNoduleUIInfo> currentFile)
            where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
        {
            IEnumerable<Output> TopConnectors(TNode n) => n.Data.Connectors.Where(c => c.Definition.Position.ForPosition(top: () => true, () => false, () => false, () => false));
            IEnumerable<Output> BottomConnectors(TNode n) => n.Data.Connectors.Where(c => c.Definition.Position.ForPosition(top: () => false, bottom: () => true, () => false, () => false));
            IEnumerable<TNode> TopConnections(TNode n) => TopConnectors(n).SelectMany(c => c.Connections).Select(c => currentFile.GetNode(c.Parent.NodeId));
            IEnumerable<TNode> BottomConnections(TNode n) => BottomConnectors(n).SelectMany(c => c.Connections).Select(c => currentFile.GetNode(c.Parent.NodeId));

            var ordered = Graph.Order(currentFile.Nodes, TopConnections, BottomConnections);

            ordered.Do
            (
                lists =>
                {
                    List<ValueTuple<TNode, PointF>> allmovement = new List<ValueTuple<TNode, PointF>>();
                    float xEdge = 100;
                    for (int j = 0; j < lists.Count; j++)
                    {
                        var list = lists[j];
                        Dictionary<TNode, int> heights = new Dictionary<TNode, int>();
                        Dictionary<int, List<TNode>> nodesAtHeight = new Dictionary<int, List<TNode>>();
                        int maxHeight = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var node = list[i];
                            int height = (TopConnections(node).Select(n => heights[n]).Max(x => (int?)x) ?? -1) + 1;
                            heights[node] = height;
                            if (!nodesAtHeight.ContainsKey(height))
                                nodesAtHeight[height] = new List<TNode>();
                            nodesAtHeight[height].Add(node);
                            maxHeight = Math.Max(maxHeight, height);
                        }

                        float[] widths = new float[maxHeight + 1];
                        float maxWidth = 0;
                        for (int h = 0; h <= maxHeight; h++)
                        {
                            var nodes = nodesAtHeight[h];
                            int count = 0;
                            foreach (var node in nodes)
                            {
                                var width = node.Renderer.Area.Width;
                                widths[h] += width;
                                count++;
                            }
                            widths[h] += (40 + h * 20) * (count - 1);
                            maxWidth = Math.Max(maxWidth, widths[h]);
                        }

                        int bestScore = int.MaxValue;
                        List<ValueTuple<TNode, PointF>> bestMovement = new List<ValueTuple<TNode, PointF>>();
                        Random r = new Random(0);

                        for (int attempts = 0; attempts < 10000; attempts++)
                        {
                            List<ValueTuple<TNode, PointF>> movement = new List<ValueTuple<TNode, PointF>>();
                            for (int h = 0; h <= maxHeight; h++)
                            {
                                var nodes = nodesAtHeight[h];
                                bool randomized = nodes.Randomise(r);
                                float x = xEdge;

                                foreach (var node in nodes)
                                {
                                    var width = node.Renderer.Area.Width;
                                    PointF to = new PointF(x + width / 2 + h * 20 + (maxWidth - widths[h]) / 2, h * 100 + 50); //stagger slightly so vertical lines don't overlap
                                    movement.Add((node, to));
                                    x += width + 40;
                                }
                            }

                            int score = Score(movement, currentFile);
                            if (score < bestScore)
                            {
                                bestScore = score;
                                bestMovement = movement.ToList();
                            }
                        }

                        xEdge += maxWidth + 40;
                        allmovement.AddRange(bestMovement);
                    }
                    currentFile.Move(allmovement);
                },
                error => MessageBox.Show("Cannot perform layout as there are cycles in the graph")
            );
        }

        private static float Cross(PointF a, PointF b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private static bool LinesIntersect(ValueTuple<PointF, PointF> a, ValueTuple<PointF, PointF> b)
        {
            PointF p = a.Item1;
            PointF q = b.Item1;
            PointF r = a.Item2.Take(a.Item1);
            PointF s = b.Item2.Take(b.Item1);
            float t = Cross(q.Take(p), s) / Cross(r, s);
            float u = Cross(q.Take(p), r) / Cross(r, s);
            return 0 < t && t < 1 && 0 < u && u < 1;
        }

        public static int Score<TNode>(List<ValueTuple<TNode, PointF>> movement, IConversationEditorControlData<TNode, TransitionNoduleUIInfo> currentFile)
            where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
        {
            Dictionary<TNode, PointF> lookup = movement.ToDictionary(m => m.Item1, m => m.Item2);
            IEnumerable<Output> BottomConnectors(TNode n) => n.Data.Connectors.Where(c => c.Definition.Position.ForPosition(top: () => false, bottom: () => true, () => false, () => false));
            IEnumerable<TNode> BottomConnections(TNode n) => BottomConnectors(n).SelectMany(c => c.Connections).Select(c => currentFile.GetNode(c.Parent.NodeId));

            int score = 0;
            var edges = movement.SelectMany(A => BottomConnections(A.Item1).Select(B => new { A = A.Item2, B = lookup[B] })).ToList();
            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                {
                    var AiPos = edges[i].A;
                    var BiPos = edges[i].B;
                    var AjPos = edges[j].A;
                    var BjPos = edges[j].B;
                    if (LinesIntersect((AiPos, BiPos), (AjPos, BjPos)))
                        score++;
                }
            }
            return score;
        }
    }
}
