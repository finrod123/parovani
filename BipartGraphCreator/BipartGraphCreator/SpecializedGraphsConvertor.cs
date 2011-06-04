using System;
using System.Diagnostics;
using GraphRepresentation;


namespace SpecializedGraphsConvertorNS
{
    public static class SpecializedGraphsConvertor
    {
        public static INonOrientedGraph NonOrientedBipartiteToNonOriented<T>(T bGraph)
            where T : IBipartiteGraph, INonOrientedGraph, IMutableGraph, IGraphBase
        {
            int vertexCount = bGraph.VertexCount;

            NonOrientedIncidenceMatrixGraph graph =
                new NonOrientedIncidenceMatrixGraph(
                    vertexCount,
                    EGraphType.General,
                    bGraph.EdgeInfoType);

            int vertexCount1 = vertexCount - 1;
            for (int i = 0; i < vertexCount1; ++i)
            {
                for (int j = i + 1; j < vertexCount; ++j)
                {
                    if (bGraph.IsEdge(i, j))
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }

            return graph;
        }

        public static IOrientedGraph NonOrientedToOriented<T>(T nGraph)
            where T : INonOrientedGraph, IMutableGraph, IGraphBase
        {
            int vertexCount = nGraph.VertexCount;

            OrientedIncidenceMatrixGraph graph =
                new OrientedIncidenceMatrixGraph(
                    vertexCount,
                    nGraph.Type,
                    nGraph.EdgeInfoType);

            int vertexCount1 = vertexCount - 1;
            for (int i = 0; i < vertexCount1; ++i)
            {
                for (int j = i + 1; j < vertexCount; ++j)
                {
                    if (nGraph.IsEdge(i, j))
                    {
                        graph.AddEdge(i, j);
                        graph.AddEdge(j, i);
                    }
                }
            }

            return graph;
        }
    }
}