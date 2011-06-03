using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace BipartiteGraphCreator
{
    using GraphRepresentation;
    using DistributionComputerNS;
    using GraphMutatorNS;

    public interface IRandomGraphGenerator
    {
        IGraphBase GetNext();
        List<IGraphBase> GetNextRange(int count);
        int VertexCount { get; set; }
    }

    public interface IEdgeProbabilityGraphGenerator:
        IRandomGraphGenerator
    {
        double EdgeProbability { get; set; }
    }

    public interface IBipartGraphGenerator:
        IRandomGraphGenerator
    {
        int MinFirstPartitySize { get; }
        int MaxFirstPartitySize { get; }
    }

    public abstract class RandomGraphGeneratorBase : IRandomGraphGenerator
    {
        static int nextGraphId = 1;
        protected int vertexCount;

        public RandomGraphGeneratorBase(int vertexCount)
        {
            VertexCount = vertexCount;
        }

        public virtual int VertexCount {
            get { return vertexCount; }
            set
            {
                Debug.Assert(value > 0);
                vertexCount = value;
            }
        }


        public static int NextGraphId()
        {
            return nextGraphId++;
        }

        public abstract IGraphBase GetNext();

        public virtual List<IGraphBase> GetNextRange(int count)
        {
            List<IGraphBase> graphs = new List<IGraphBase>();

            for (int i = 0; i < count; ++i)
            {
                graphs.Add(GetNext());
            }

            return graphs;
        }
    }

    public abstract class EdgeProbabilityGraphGeneratorBase :
        RandomGraphGeneratorBase,
        IEdgeProbabilityGraphGenerator
    {
        protected double edgeProbability;
        protected UniformDistributionComputer edgeComputer;

        public EdgeProbabilityGraphGeneratorBase(int vertexCount, double edgeProbability) :
            base(vertexCount)
        {
            EdgeProbability = edgeProbability;
            edgeComputer = new UniformDistributionComputer();
        }

        public double EdgeProbability
        {
            get { return edgeProbability; }
            set
            {
                Debug.Assert(0.0d <= value &&
                             value <= 1.0d);

                edgeProbability = value;
            }
        }
    }

    public class OrientedEdgeProbabilityGraphGenerator :
        EdgeProbabilityGraphGeneratorBase
    {
        public OrientedEdgeProbabilityGraphGenerator(int vertexCount, double edgeProbability) :
            base(vertexCount, edgeProbability)
        { }

        public override IGraphBase GetNext()
        {
            IMutableGraph graph = new OrientedIncidenceMatrixGraph(
                RandomGraphGeneratorBase.NextGraphId(),
                vertexCount, EGraphType.General, EEdgeInfoType.Standard);

            for (int i = 0; i < vertexCount; ++i)
            {
                for (int j = 0; j < vertexCount; ++j)
                {
                    if (i != j &&
                        edgeComputer.NextDouble(0.0d, 1.0d) <= edgeProbability)
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }

            return graph as IGraphBase;
        }
    }

    public class NonOrientedEdgeProbabilityGraphGenerator :
        OrientedEdgeProbabilityGraphGenerator
    {
        public NonOrientedEdgeProbabilityGraphGenerator(int vertexCount,
            double edgeProbability) :
            base(vertexCount, edgeProbability)
        { }

        public override IGraphBase GetNext()
        {
            IMutableGraph graph = new NonOrientedIncidenceMatrixGraph(
                RandomGraphGeneratorBase.NextGraphId(),
                vertexCount, EGraphType.General, EEdgeInfoType.Standard);

            for (int i = 0; i < (vertexCount - 1); ++i)
            {
                for (int j = i + 1; j < vertexCount; ++j)
                {
                    if (edgeComputer.NextDouble(0.0d, 1.0d) <= edgeProbability)
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }

            return graph as IGraphBase;
        }
    }

    public class NonOrientedEdgeProbabilityBipartGraphGenerator :
        NonOrientedEdgeProbabilityGraphGenerator,
        IBipartGraphGenerator
    {
        protected int minFirstPartitySize,
                      maxFirstPartitySize;
        // generator rozmisteni vrcholu do partit
        protected DistributionComputer partitionComputer;

        public NonOrientedEdgeProbabilityBipartGraphGenerator(
            int vertexCount,
            double edgeProbability,
            int minFirstPartitySize, int maxFirstPartitySize):
            base(vertexCount, edgeProbability)
        {
            Debug.Assert(0 < minFirstPartitySize &&
                         minFirstPartitySize <= maxFirstPartitySize &&
                         maxFirstPartitySize < vertexCount);

            this.minFirstPartitySize = minFirstPartitySize;
            this.maxFirstPartitySize = maxFirstPartitySize;
            
            partitionComputer = new UniformDistributionComputer();
        }

        public override int VertexCount
        {
            get { return vertexCount; }
            set
            {
                Debug.Assert(maxFirstPartitySize < value);
                base.VertexCount = value;
            }
        }

        public int MinFirstPartitySize
        {
            get { return minFirstPartitySize; }
            set
            {
                Debug.Assert(0 < value &&
                             value <= maxFirstPartitySize);

                minFirstPartitySize = value;
            }
        }

        public int MaxFirstPartitySize
        {
            get { return maxFirstPartitySize; }
            set
            {
                Debug.Assert(minFirstPartitySize <= value &&
                             value < vertexCount);

                maxFirstPartitySize = value;
            }
        }

        public override IGraphBase GetNext()
        {
            int firstPartitySize = partitionComputer.Next(minFirstPartitySize, maxFirstPartitySize);

            IMutableGraph graph = new BipartiteNonOrientedIncidenceMatrixGraph(
                RandomGraphGeneratorBase.NextGraphId(),
                vertexCount, firstPartitySize);

            for (int i = 0; i < firstPartitySize; ++i)
            {
                for (int j = firstPartitySize; j < vertexCount; ++j)
                {
                    if (edgeComputer.NextDouble(0.0d, 1.0d) <= edgeProbability)
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }

            return graph as IGraphBase;
        }
    }
}


