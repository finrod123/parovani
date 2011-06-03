using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GraphRepresentation;
using System.Diagnostics;

namespace GraphReaderNS
{
    public interface IGraphReader
    {
        bool ReadGraph(out IGraphBase graph);
        void Open(Stream stream);
        void Close();
    }

    public class GraphReader : IGraphReader, IDisposable
    {
        StreamReader sr;

        public GraphReader(Stream stream)
        {
            sr = new StreamReader(stream);
        }

        /*
         graph.Id,
                    (int)graph.Representation,
                    (int)graph.Orientation,
                    (int)graph.Mutability,
                    (int)graph.EdgeInfoType,
                    (int)graph.Type,
                    graph.VertexCount,
                    graph.EdgeCount
         */
        public bool ReadGraph(out IGraphBase graph)
        {
            int id, vertexCount, edgeCount;
            EGraphType type = default(EGraphType);
            EGraphRepresentation representation = default(EGraphRepresentation);
            EGraphOrientation orientation = default(EGraphOrientation);
            EMutability mutability = default(EMutability);
            EEdgeInfoType edgeInfoType = default(EEdgeInfoType);

            graph = null;

            Type typeT = Type.GetTypeFromHandle(Type.GetTypeHandle(type)),
                 representationT = Type.GetTypeFromHandle(Type.GetTypeHandle(representation)),
                 orientationT = Type.GetTypeFromHandle(Type.GetTypeHandle(orientation)),
                 mutabilityT = Type.GetTypeFromHandle(Type.GetTypeHandle(mutability)),
                 edgeInfoT = Type.GetTypeFromHandle(Type.GetTypeHandle(edgeInfoType));

            string[] parts = sr.ReadLine().Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (!int.TryParse(parts[0], out id) ||
                 id <= 0 ||
                !Enum.IsDefined(representationT, representation = (EGraphRepresentation)Enum.Parse(representationT, parts[1])) ||
                !Enum.IsDefined(orientationT, orientation = (EGraphOrientation)Enum.Parse(orientationT, parts[2])) ||
                !Enum.IsDefined(mutabilityT, mutability = (EMutability)Enum.Parse(mutabilityT, parts[3])) ||
                !Enum.IsDefined(edgeInfoT, edgeInfoType = (EEdgeInfoType)Enum.Parse(edgeInfoT, parts[4])) ||
                !Enum.IsDefined(typeT, type = (EGraphType)Enum.Parse(typeT, parts[5])) ||
                !int.TryParse(parts[6], out vertexCount) ||
                 vertexCount <= 0 ||
                !int.TryParse(parts[7], out edgeCount) ||
                 edgeCount < 0)
                return false;

            switch (type)
            {
                case EGraphType.Bipartite:
                    
                    int firstPartitySize;

                    if (!int.TryParse(parts[8], out firstPartitySize))
                        return false;

                    switch (representation)
                    {
                        case EGraphRepresentation.IncidenceMatrix:

                        IncidenceMatrixGraph iGraph = null;

                            switch (orientation)
                            {
                                case EGraphOrientation.NonOriented:

                                    switch (edgeInfoType)
                                    {
                                        case EEdgeInfoType.Standard:

                                        iGraph = (graph = new BipartiteNonOrientedIncidenceMatrixGraph(
                                            id, vertexCount, firstPartitySize)) as IncidenceMatrixGraph;

                                            break;
                                    }
                                    break;
                            }
                            // nacti incidence matrix
                            if (!iGraph.LoadIncidenceMatrix(parts[9]))
                                return false;

                            break;
                    }
                    break;
            }

            return true;
        }

        public void Close()
        {
            sr.Dispose();
            sr.Close();
        }

        public void Dispose()
        {
            Close();
        }

        #region IGraphReader Members


        public void Open(Stream stream)
        {
            if (stream != null &&
                stream.CanRead)
            {
                if (sr != null)
                    Close();

                sr = new StreamReader(stream);
            }
        }

        #endregion
    }
}