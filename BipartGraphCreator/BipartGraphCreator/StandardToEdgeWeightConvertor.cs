using System;
using System.Diagnostics;
using GraphRepresentation;

namespace StandardToEdgeWeightConvertorNS
{
    public static class StandardToEdgeWeightConvertor
    {
        public static R OrientedToEdgeWeight<R, S, T>(S oGraph)
            where R : class, IOrientedGraph, IEdgeWeightMutableGraph<T>
            where S : class, IOrientedGraph, IGraphBase
            where T : struct
        {
            int vertexCount = oGraph.VertexCount;

            OrientedEdgeWeightEnabledIncidenceMatrixGraph<T> graph =
                new OrientedEdgeWeightEnabledIncidenceMatrixGraph<T>(
                    vertexCount,
                    oGraph.Type);

            for (int i = 0; i < vertexCount; ++i)
            {
                for (int j = 0; j < vertexCount; ++j)
                {
                    if (oGraph.IsEdge(i, j))
                    {
                        T value = (T)(object)1.0d;
                        graph.AddEdge(i, j, value);
                    }
                }
            }

            return graph as R;
        }
    }
}