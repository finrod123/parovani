using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BipartiteGraphCreator;
using GraphReaderNS;
using GraphWriterNS;
using GraphRepresentation;
using GraphMutatorNS;
using SpecializedGraphsConvertorNS;
using StandardToEdgeWeightConvertorNS;

namespace bitGraphsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            /*NonOrientedEdgeProbabilityBipartGraphGenerator gen = new
                 NonOrientedEdgeProbabilityBipartGraphGenerator(10, 0.5d, 2, 4);

            BipartiteNonOrientedIncidenceMatrixGraph g = gen.GetNext() as BipartiteNonOrientedIncidenceMatrixGraph;

            GraphWriter gw = new GraphWriter(new FileStream("vystup.txt", FileMode.Create));
            gw.WriteGraph(g);
            gw.Close();*/

            GraphReader gr = new GraphReader(new FileStream("vystup.txt", FileMode.Open));
            IGraphBase graph;
            if (!gr.ReadGraph(out graph))
                return;

            BipartiteNonOrientedIncidenceMatrixGraph g = graph as BipartiteNonOrientedIncidenceMatrixGraph;

            gr.Close();

            foreach (int i in g.FirstPartityVerticesList)
                Console.WriteLine("{0}, ", i);
            Console.WriteLine();
            foreach (int i in g.SecondPartityVerticesList)
                Console.WriteLine("{0}, ", i);

            if (!g.AddEdge(3, 7))
                Console.WriteLine("Nejde pridat!");
            else
                Console.WriteLine("Jde pridat!");
            
            g.AddVertex();
            g.AddVertex();



            g.AddEdge(10, 11);
            g.RemoveVertex(0);
            g.RemoveVertex(1);
            g.RemoveVertex(2);

            Console.WriteLine("Prvni partita");
            foreach (int i in g.FirstPartityVerticesList)
                Console.WriteLine("{0}, ", i);
            Console.WriteLine("Druha partita");
            foreach (int i in g.SecondPartityVerticesList)
                Console.WriteLine("{0}, ", i);
            Console.WriteLine("Zadna partita");
            foreach (int i in g.NoPartityVerticesList)
                Console.WriteLine("{0}, ", i);
            Console.ReadKey();
        }
    }
}
