﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BipartiteGraphCreator;
using GraphReaderNS;
using GraphWriterNS;
using GraphRepresentation;
using GraphMutatorNS;

namespace bitGraphsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            NonOrientedEdgeProbabilityBipartGraphGenerator graphGen =
                new NonOrientedEdgeProbabilityBipartGraphGenerator(10, 0.5d, 2, 5);

            BipartiteNonOrientedIncidenceMatrixGraph graph;

            GraphWriter gw = new GraphWriter(new FileStream("vystup.txt", FileMode.Create));
            gw.WriteGraph(graph = graphGen.GetNext() as BipartiteNonOrientedIncidenceMatrixGraph);

            EdgeFlipBipartiteNonOrientedGraphMutator<BipartiteNonOrientedIncidenceMatrixGraph>
                mutator = new EdgeFlipBipartiteNonOrientedGraphMutator<BipartiteNonOrientedIncidenceMatrixGraph>();

            mutator.MutateGraph(ref graph);

            gw.WriteGraph(graph);

            gw.Close();

            Console.ReadKey();
        }
    }
}