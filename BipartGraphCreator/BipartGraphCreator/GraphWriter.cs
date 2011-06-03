using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GraphRepresentation;

namespace GraphWriterNS
{
    public interface IGraphWriter
    {
        void WriteGraph(IGraphBase graph);
        void WriteGraphs(List<IGraphBase> graphs);
        void Open(Stream stream);
        void Close();
    }

    public class GraphWriter : IGraphWriter, IDisposable
    {
        StreamWriter sw;

        public GraphWriter(Stream stream)
        {
            Open(stream);
        }

        public void WriteGraph(IGraphBase graph)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};",
                new object[8] {
                    graph.Id,
                    (int)graph.Representation,
                    (int)graph.Orientation,
                    (int)graph.Mutability,
                    (int)graph.EdgeInfoType,
                    (int)graph.Type,
                    graph.VertexCount,
                    graph.EdgeCount
                });

            switch (graph.Type)
            {
                case EGraphType.Bipartite:
                    sb.AppendFormat("{0};", (graph as IBipartiteGraph).FirstPartitySize);
                    break;
            }

            switch (graph.Representation)
            {
                case EGraphRepresentation.IncidenceMatrix:
                
                    IncidenceMatrixGraph iGraph = graph as IncidenceMatrixGraph;
                    sb.AppendFormat("{0}", iGraph.IncidenceMatrixToString());
                    break;
            }

            sw.WriteLine(sb.ToString());
        }

        public void WriteGraphs(List<IGraphBase> graphs)
        {
            foreach (IGraphBase graph in graphs)
            {
                WriteGraph(graph);
            }
        }

        public void Close()
        {
            sw.Dispose();
            sw.Close();
        }

        public void Dispose()
        {
            Close();
        }

        #region IGraphWriter Members


        public void Open(Stream stream)
        {
            if (stream != null &&
                stream.CanWrite)
            {
                if (sw != null)
                    Close();

                sw = new StreamWriter(stream);
            }
        }

        #endregion
    }
}