using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace GraphMutatorNS
{
    using GraphRepresentation;

    public interface IGraphMutator<T>
        where T : IMutableGraph
    {
        void MutateGraph(ref T graph);
    }

    public abstract class GraphMutatorBase<T> :
        IGraphMutator<T>
        where T : IMutableGraph
    {
        public GraphMutatorBase() { }
        public abstract void MutateGraph(ref T graph);
    }


    public abstract class EdgeFlipGraphMutatorBase<T> :
        GraphMutatorBase<T>
        where T : IMutableGraph
    {
        public EdgeFlipGraphMutatorBase() { }
    }

    public class EdgeFlipOrientedGraphMutator<T> :
        EdgeFlipGraphMutatorBase<T>
        where T : IGraphBase, IMutableGraph, IOrientedGraph
    {
        public EdgeFlipOrientedGraphMutator() { }

        protected virtual void FlipEdgesCross(int edge1Vertex1, int edge1Vertex2,
            int edge2Vertex1, int edge2Vertex2,
            ref T graph)
        {
            graph.RemoveEdge(edge1Vertex1, edge1Vertex2);
            graph.RemoveEdge(edge2Vertex1, edge2Vertex2);

            graph.AddEdge(edge1Vertex1, edge2Vertex2);
            graph.AddEdge(edge2Vertex1, edge1Vertex2);
        }

        protected virtual void MutateVertexRange(int from, int to, ref T graph)
        {
            int to1 = to - 1;

            for (int i = from; i < to1; ++i)
            {
                for (int j = i + 1; j < to; ++j)
                {
                    FlipEdges(i, j, ref graph);
                }
            }
        }

        public override void MutateGraph(ref T graph)
        {
            MutateVertexRange(0, graph.VertexCount - 1, ref graph);
        }

        protected virtual bool findCrossingEdges(int vert1, int vert2, ref T graph,
            out int vert1partner, out int vert2partner)
        {
            vert1partner = vert2partner = -1;

            if (vert1 <= vert2)
            {
                if (!findHighestEdge(vert1, ref graph, out vert1partner) ||
                    !findLowestEdge(vert2, ref graph, out vert2partner))
                {
                    return false;
                }

                return vert1partner > vert2partner;
            } else
            {
                if (!findHighestEdge(vert2, ref graph, out vert2partner) ||
                   !findLowestEdge(vert1, ref graph, out vert1partner))
                {
                    return false;
                }

                return vert1partner < vert2partner;
            }
        }


        protected virtual bool findLowestEdge(int vert, ref T graph, out int edgeVert)
        {
            return findEdge(vert, 0, graph.VertexCount - 1, 1, ref graph, out edgeVert);
        }

        protected virtual bool findHighestEdge(int vert, ref T graph, out int edgeVert)
        {
            return findEdge(vert, graph.VertexCount - 1, 0, -1, ref graph, out edgeVert);
        }

        protected virtual bool findEdge(int vert, int from, int to, int step,
            ref T graph,
            out int edgeVert)
        {
            edgeVert = -1;

            for (int i = from; i != to; i += step)
            {
                if (graph.IsEdge(vert, i))
                {
                    edgeVert = i;
                    return true;
                }
            }

            return false;
        }

        protected virtual void FlipEdges(int vert1, int vert2, ref T graph)
        {
            int vert1partner, vert2partner;

            while (findCrossingEdges(vert1, vert2, ref graph, out vert1partner, out vert2partner))
            {
                FlipEdgesCross(vert1, vert1partner, vert2, vert2partner, ref graph);
            }
        }
    }

    public class EdgeFlipNonOrientedGraphMutator<T> :
        EdgeFlipOrientedGraphMutator<T>
        where T : IGraphBase, IMutableGraph, INonOrientedGraph
    {
        public EdgeFlipNonOrientedGraphMutator() { }
    }

    public class EdgeFlipBipartiteOrientedGraphMutator<T> :
        EdgeFlipOrientedGraphMutator<T>
        where T : IGraphBase, IMutableGraph, IOrientedGraph, IBipartiteGraph
    {
        public EdgeFlipBipartiteOrientedGraphMutator() { }

        protected override bool findLowestEdge(int vert, ref T graph, out int edgeVert)
        {
            int vertexCount = graph.VertexCount;
            int firstPartitySize = graph.FirstPartitySize;

            if (vert < firstPartitySize)
            {
                return findEdge(vert, firstPartitySize, vertexCount - 1, 1, ref graph, out edgeVert);
            } else
            {
                return findEdge(vert, 0, firstPartitySize - 1, 1, ref graph, out edgeVert);
            }
        }

        protected override bool findHighestEdge(int vert, ref T graph, out int edgeVert)
        {
            int vertexCount = graph.VertexCount;
            int firstPartitySize = graph.FirstPartitySize;

            if (vert < firstPartitySize)
            {
                return findEdge(vert, vertexCount - 1, firstPartitySize, -1, ref graph, out edgeVert);
            } else
            {
                return findEdge(vert, firstPartitySize - 1, 0, -1, ref graph, out edgeVert);
            }
        }

        public override void MutateGraph(ref T graph)
        {
            int firstPartitySize = graph.FirstPartitySize;

            MutateVertexRange(0, firstPartitySize, ref graph);
            MutateVertexRange(firstPartitySize, graph.VertexCount, ref graph);
        }
    }

    public class EdgeFlipBipartiteNonOrientedGraphMutator<T> :
        EdgeFlipBipartiteOrientedGraphMutator<T>
        where T : IGraphBase, IMutableGraph, INonOrientedGraph, IBipartiteGraph
    {
        public EdgeFlipBipartiteNonOrientedGraphMutator() { }

        public override void MutateGraph(ref T graph)
        {
            MutateVertexRange(0, graph.FirstPartitySize, ref graph);
        }
    }
}
