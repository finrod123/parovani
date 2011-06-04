using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using MatrixNS;

namespace GraphRepresentation
{
    public enum EGraphType
    {
        General,
        Bipartite
    };

    public enum EGraphRepresentation
    {
        IncidenceMatrix
    };

    public enum EMutability
    {
        Immutable,
        EdgeMutable,
        VertexMutable,
        Mutable
    };

    public enum EEdgeInfoType
    {
        Standard,
        Number
    };

    public enum EGraphOrientation
    {
        Oriented,
        NonOriented
    };

    public interface IOrientedGraph { }
    public interface INonOrientedGraph : IOrientedGraph{ }

    public interface IEdgeMutableGraph
    {
        void AddEdge(int vertex1, int vertex2);
        void RemoveEdge(int vertex1, int vertex2);
    }

    public interface IVertexMutableGraph
    {
        int AddVertex();
        void RemoveVertex(int vertex);
    }

    public interface IMutableGraph :
        IVertexMutableGraph,
        IEdgeMutableGraph
    { }

    public interface IGraphBase {
        int Id { get; }
        EGraphType Type { get; }
        EGraphOrientation Orientation { get; }
        EGraphRepresentation Representation { get; }
        EMutability Mutability { get; }
        EEdgeInfoType EdgeInfoType { get; }
        int VertexCount { get; }
        int EdgeCount { get; }
        bool IsEdge(int vertex1, int vertex2);
    }

    public interface IEdgeWeightMutableGraph<T> :
        IEdgeMutableGraph
        where T : struct
    {
        void AddEdge(int vertex1, int vertex2, T edgeValue);
        T EdgeWeight(int vertex1, int vertex2);
        void SetEdgeWeight(int vertex1, int vertex2, T newEdgeValue);
    }

    public interface IBipartiteGraph : IPartitionInfo
    {
        int FirstPartitySize { get; }
        int SecondPartitySize { get; }
        ReadOnlyCollection<int> FirstPartityVerticesList { get; }
        ReadOnlyCollection<int> SecondPartityVerticesList { get; }
        ReadOnlyCollection<int> NoPartityVerticesList { get; }
        EPartity GetVertexPartity(int vertex);
    }

    public enum EPartity
    {
        None,
        First,
        Second        
    }

    public abstract class GraphBase : IGraphBase
    {
        readonly int id;
        static int nextId = 1;
        protected int vertexCount;
        protected int edgeCount;
        readonly EGraphType type;
        readonly EGraphOrientation orientation;
        readonly EGraphRepresentation representation;
        readonly EMutability mutability;
        readonly EEdgeInfoType edgeInfoType;

        public GraphBase(
            int id,
            int vertexCount,
            EGraphType type,
            EGraphOrientation orientation,
            EGraphRepresentation representation,
            EMutability mutability,
            EEdgeInfoType edgeInfoType)
        {
            Debug.Assert(0 < id &&
                         0 < vertexCount);

            this.id = id;
            this.vertexCount = vertexCount;

            this.type = type;
            this.orientation = orientation;
            this.representation = representation;
            this.mutability = mutability;
            this.edgeInfoType = edgeInfoType;
        }

        public int Id
        {
            get { return id; }
        }

        public int VertexCount
        {
            get { return vertexCount; }
        }

        public virtual int EdgeCount {
            get { return edgeCount; }
            protected set
            {
                Debug.Assert(0 <= value);
                edgeCount = value;
            }
        }
        
        public EGraphType Type {
            get { return type; }
        }

        public EGraphOrientation Orientation
        {
            get { return orientation; }
        }

        public EGraphRepresentation Representation
        {
            get { return representation; }
        }

        public EMutability Mutability
        {
            get { return mutability; }
        }

        public EEdgeInfoType EdgeInfoType
        {
            get { return edgeInfoType; }
        }

        public abstract int AddVertex();
        public abstract void RemoveVertex(int vertex);
        public abstract void AddEdge(int vertex1, int vertex2);
        public abstract void  RemoveEdge(int vertex1, int vertex2);
        public abstract bool IsEdge(int vertex1, int vertex2);

        public static int GetNextId() { return nextId++; }
    }

    public abstract class IncidenceMatrixGraph :
        GraphBase,
        IMutableGraph
    {
        protected IMatrix<bool> matrix;

        public IncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type,
            EGraphOrientation orientation,
            EMutability mutability,
            EEdgeInfoType edgeInfoType) :
            base(id, vertexCount, type, orientation,
            EGraphRepresentation.IncidenceMatrix, mutability, edgeInfoType)
        {
            matrix = new Matrix<bool>(vertexCount, vertexCount);
            matrix.Clear();
        }

        public virtual bool LoadIncidenceMatrix(string matrixString)
        {
            foreach (string row in matrixString.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries))
            {
                StringCollection rowParts = new StringCollection();
                rowParts.AddRange(row.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                int rowVertex;

                if (!int.TryParse(rowParts[0], out rowVertex))
                    return false;

                rowParts.RemoveAt(0);

                foreach (string sVertex in rowParts)
                {
                    int rowNeighbor;

                    if (!int.TryParse(sVertex, out rowNeighbor))
                        return false;

                    AddEdge(rowVertex, rowNeighbor);
                }
            }

            return true;
        }

        public override bool IsEdge(int vertex1, int vertex2)
        {
            return matrix[vertex1, vertex2];
        }

        protected void addEdgeHelper(int vertex1, int vertex2)
        {
            matrix[vertex1, vertex2] = true;
        }

        public override void AddEdge(int vertex1, int vertex2)
        {
            addEdgeHelper(vertex1, vertex2);
            ++edgeCount;
        }

        protected void removeEdgeHelper(int vertex1, int vertex2)
        {
            matrix[vertex1, vertex2] = false;
        }

        public override void RemoveEdge(int vertex1, int vertex2)
        {
            removeEdgeHelper(vertex1, vertex2);
            --edgeCount;
        }

        public override int AddVertex()
        {
            matrix.AddRow();
            matrix.AddCol();

            return vertexCount++;
        }

        public override void RemoveVertex(int vertex)
        {
            RemoveNeighbors(vertex);

            matrix.RemoveRow(vertex);
            matrix.RemoveCol(vertex);

            --vertexCount;
        }

        protected virtual void RemoveNeighbors(int vertex)
        {
            for (int i = 0; i < vertexCount; ++i)
            {
                if (IsEdge(vertex, i))
                {
                    RemoveEdge(vertex, i);
                }
            }
        }

        public abstract string IncidenceMatrixToString();
    }

    public class OrientedIncidenceMatrixGraph :
        IncidenceMatrixGraph,
        IOrientedGraph
    {
        protected OrientedIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type,
            EGraphOrientation orientation,
            EEdgeInfoType edgeInfoType) :
            base(id, vertexCount, type, orientation, EMutability.EdgeMutable, edgeInfoType)
        { }

        public OrientedIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type, EEdgeInfoType edgeInfoType) :
            this(id, vertexCount, type, EGraphOrientation.Oriented, edgeInfoType)
        { }

        public OrientedIncidenceMatrixGraph(int vertexCount,
            EGraphType type, EEdgeInfoType edgeInfoType) :
            this(GetNextId(), vertexCount, type, edgeInfoType)
        { }

        public override string IncidenceMatrixToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < vertexCount; ++i)
            {

                bool hasEdge = false;

                for (int j = 0; j < vertexCount; ++j)
                {
                    if (IsEdge(i, j))
                    {
                        if (!hasEdge)
                        {
                            sb.AppendFormat(":{0}", i);
                            hasEdge = true;
                        }

                        sb.AppendFormat(",{0}", j);
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class NonOrientedIncidenceMatrixGraph :
        OrientedIncidenceMatrixGraph,
        INonOrientedGraph
    {
        public NonOrientedIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type, EEdgeInfoType edgeInfoType) :
            base(id, vertexCount, type, EGraphOrientation.NonOriented, edgeInfoType)
        { }

        public NonOrientedIncidenceMatrixGraph(int vertexCount,
            EGraphType type, EEdgeInfoType edgeInfoType) :
            this(GetNextId(), vertexCount, type, edgeInfoType)
        { }

        public override bool  IsEdge(int vertex1, int vertex2)
        {
            return
                base.IsEdge(vertex1, vertex2) ||
                base.IsEdge(vertex2, vertex1);
        }

        public override void AddEdge(int vertex1, int vertex2)
        {
            addEdgeHelper(vertex1, vertex2);
            addEdgeHelper(vertex2, vertex1);
            ++edgeCount;
        }

        public override void RemoveEdge(int vertex1, int vertex2)
        {
            removeEdgeHelper(vertex1, vertex2);
            removeEdgeHelper(vertex2, vertex1);
            --edgeCount;
        }

        public override string IncidenceMatrixToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < (vertexCount - 1); ++i) {

                bool hasEdge = false;

                for (int j = i + 1; j < vertexCount; ++j) {
                    if (IsEdge(i, j)) {
                        if (!hasEdge)
                        {
                            sb.AppendFormat(":{0}", i);
                            hasEdge = true;
                        }

                        sb.AppendFormat(",{0}", j);
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class OrientedEdgeWeightEnabledIncidenceMatrixGraph<T> :
        OrientedIncidenceMatrixGraph,
        IEdgeWeightMutableGraph<T>
        where T : struct
    {
        IMatrix<T> edgeWeights;

        public OrientedEdgeWeightEnabledIncidenceMatrixGraph(int vertexCount,
            EGraphType type) :
            this(GetNextId(), vertexCount, type)
        { }

        public OrientedEdgeWeightEnabledIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type) :
            this(id, vertexCount, type, EGraphOrientation.Oriented)
        { }

        protected OrientedEdgeWeightEnabledIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type, EGraphOrientation orientation):
            base(id, vertexCount, type, orientation, EEdgeInfoType.Number)
        {
            edgeWeights = new Matrix<T>(vertexCount, vertexCount);
            edgeWeights.Clear();
        }

        protected void addEdgeHelper(int vertex1, int vertex2, T edgeValue)
        {
            addEdgeHelper(vertex1, vertex2);
            SetEdgeWeight(vertex1, vertex2, edgeValue);
        }

        public virtual void AddEdge(int vertex1, int vertex2, T edgeValue)
        {
            base.AddEdge(vertex1, vertex2);
            SetEdgeWeight(vertex1, vertex2, edgeValue);
        }

        public virtual T EdgeWeight(int vertex1, int vertex2)
        {
            if (IsEdge(vertex1, vertex2))
            {
                return edgeWeights[vertex1, vertex2];
            } else
            {
                return default(T);
            }
        }

        public virtual void SetEdgeWeight(int vertex1, int vertex2, T newEdgeValue)
        {
            edgeWeights[vertex1, vertex2] = newEdgeValue;
        }

        public override void AddEdge(int vertex1, int vertex2)
        {
 	        AddEdge(vertex1, vertex2, default(T));
        }

        public override int AddVertex()
        {
            edgeWeights.AddRow();
            edgeWeights.AddCol();

            return base.AddVertex();
        }

        public override void RemoveVertex(int vertex)
        {
            edgeWeights.RemoveRow(vertex);
            edgeWeights.RemoveCol(vertex);

            base.RemoveVertex(vertex);
        }

        public override string IncidenceMatrixToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < vertexCount; ++i)
            {

                bool hasEdge = false;

                for (int j = 0; j < vertexCount; ++j)
                {
                    if (IsEdge(i, j))
                    {
                        if (!hasEdge)
                        {
                            sb.AppendFormat(":{0}", i);
                            hasEdge = true;
                        }

                        sb.AppendFormat(",{0},{1}", j, EdgeWeight(i, j));
                    }
                }
            }

            return sb.ToString();
        }

        public override bool LoadIncidenceMatrix(string matrixString)
        {
            foreach (string row in matrixString.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] rowParts = row.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int vertex;

                if (!int.TryParse(rowParts[0], out vertex))
                    return false;

                int rowPartsLen = rowParts.Length;
                for(int i = 1; i < rowPartsLen; i += 2)
                {
                    int neighbor;
                    T edgeValue;

                    if(!int.TryParse(rowParts[i], out neighbor))
                        return false;

                    edgeValue = (T)(object)rowParts[i + 1];

                    AddEdge(vertex, neighbor, edgeValue);
                }

            }

            return true;
        }
    }

    public class NonOrientedEdgeWeightEnabledIncidenceMatrixGraph<T> :
        OrientedEdgeWeightEnabledIncidenceMatrixGraph<T>
        where T : struct
    {
        public NonOrientedEdgeWeightEnabledIncidenceMatrixGraph(int vertexCount,
            EGraphType type) :
            this(GetNextId(), vertexCount, type)
        { }

        public NonOrientedEdgeWeightEnabledIncidenceMatrixGraph(int id, int vertexCount,
            EGraphType type) :
            base(id, vertexCount, type, EGraphOrientation.NonOriented)
        { }

        public override void AddEdge(int vertex1, int vertex2, T edgeValue)
        {
            addEdgeHelper(vertex1, vertex2, edgeValue);
            addEdgeHelper(vertex2, vertex1, edgeValue);
            ++edgeCount;
        }

        public override void AddEdge(int vertex1, int vertex2)
        {
            AddEdge(vertex1, vertex2, default(T));
        }

        public override void RemoveEdge(int vertex1, int vertex2)
        {
            removeEdgeHelper(vertex1, vertex2);
            removeEdgeHelper(vertex2, vertex1);
            --edgeCount;
        }

        public override void SetEdgeWeight(int vertex1, int vertex2, T newEdgeValue)
        {
            base.SetEdgeWeight(vertex1, vertex2, newEdgeValue);
            base.SetEdgeWeight(vertex2, vertex1, newEdgeValue);
        }

        public override string IncidenceMatrixToString()
        {
            StringBuilder sb = new StringBuilder();
            int vertexCount1 = vertexCount - 1;

            for (int i = 0; i < vertexCount1; ++i)
            {

                bool hasEdge = false;

                for (int j = i + 1; j < vertexCount; ++j)
                {
                    if (IsEdge(i, j))
                    {
                        if (!hasEdge)
                        {
                            sb.AppendFormat(":{0}", i);
                            hasEdge = true;
                        }

                        sb.AppendFormat(",{0},{1}", j, EdgeWeight(i, j));
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class BipartiteNonOrientedIncidenceMatrixGraph :
        NonOrientedIncidenceMatrixGraph,
        IBipartiteGraph
    {
        int firstPartitySize,
            secondPartitySize;

        List<int> firstPartity,
                  secondPartity,
                  noPartity;

        Dictionary<int, EPartity> vertexToPartity;

        public BipartiteNonOrientedIncidenceMatrixGraph(
            int vertexCount,
            int firstPartitySize) :
            this(GetNextId(), vertexCount, firstPartitySize)
        { }


        public BipartiteNonOrientedIncidenceMatrixGraph(
            int id,
            int vertexCount,
            int firstPartitySize) :
            base(id, vertexCount, EGraphType.Bipartite, EEdgeInfoType.Standard) {

            this.firstPartitySize = firstPartitySize;
            this.secondPartitySize = vertexCount - firstPartitySize;

            firstPartity = new List<int>(firstPartitySize);
            secondPartity = new List<int>(secondPartitySize);
            noPartity = new List<int>();

            vertexToPartity = new Dictionary<int, EPartity>(vertexCount);
        }

        public int FirstPartitySize
        {
            get { return firstPartitySize; }
        }

        public int SecondPartitySize
        {
            get { return secondPartitySize; }
        }

        public ReadOnlyCollection<int> FirstPartityVerticesList
        {
            get { return firstPartity.AsReadOnly(); }
        }

        public ReadOnlyCollection<int> SecondPartityVerticesList
        {
            get { return secondPartity.AsReadOnly(); }
        }

        public ReadOnlyCollection<int> NoPartityVerticesList
        {
            get { return noPartity.AsReadOnly(); }
        }

        public EPartity GetVertexPartity(int vertex)
        {
            return vertexToPartity[vertex];
        }
    }

    public class BipartiteNonOrientedEdgeWeightEnabledIncidenceMatrixGraph<T> :
        NonOrientedEdgeWeightEnabledIncidenceMatrixGraph<T>,
        IBipartiteGraph
        where T : struct
    {
        

        public BipartiteNonOrientedEdgeWeightEnabledIncidenceMatrixGraph(int vertexCount,
            int firstPartitySize)
            : this(GetNextId(), vertexCount, firstPartitySize)
        { }

        public BipartiteNonOrientedEdgeWeightEnabledIncidenceMatrixGraph(
            int id, int vertexCount,
            int firstPartitySize) :
            base(id, vertexCount, EGraphType.Bipartite)
        {
            Debug.Assert(0 < firstPartitySize &&
                         firstPartitySize < vertexCount);

            this.firstPartitySize = firstPartitySize;
            this.secondPartitySize = vertexCount - firstPartitySize;

            firstPartity = new List<int>(firstPartitySize);
            secondPartity = new List<int>(secondPartitySize);
            noPartity = new List<int>();

            vertexToPartity = new Dictionary<int, EPartity>(vertexCount);
        }

        public int FirstPartitySize
        {
            get { return firstPartitySize; }
        }

        public int SecondPartitySize
        {
            get { return secondPartitySize; }
        }
    }

    public interface IPartitionInfo
    {
        int FirstPartitySize { get; }
        int SecondPartitySize { get; }
        ReadOnlyCollection<int> FirstPartityVerticesList { get; }
        ReadOnlyCollection<int> SecondPartityVerticesList { get; }
        ReadOnlyCollection<int> NoPartityVerticesList { get; }
    }

    class PartitionInfo : IPartitionInfo
    {
        int firstPartitySize, secondPartitySize;
        bool canAddEdge;

        List<int> firstPartity,
                  secondPartity,
                  noPartity;

        Dictionary<int, EPartity> vertexToPartity;

        public PartitionInfo(int firstPartitySize, int secondPartitySize)
        {
            int vertexCount = firstPartitySize + secondPartitySize;

            Debug.Assert(0 < firstPartitySize &&
                         firstPartitySize < vertexCount);

            this.firstPartitySize = firstPartitySize;
            this.secondPartitySize = secondPartitySize;
            canAddEdge = false;

            firstPartity = new List<int>(firstPartitySize);
            secondPartity = new List<int>(secondPartitySize);
            noPartity = new List<int>();

            vertexToPartity = new Dictionary<int, EPartity>(vertexCount);
        }

        public int FirstPartitySize
        {
            get { return firstPartitySize; }
        }

        public int SecondPartitySize
        {
            get { return secondPartitySize; }
        }

        public ReadOnlyCollection<int> FirstPartityVerticesList
        {
            get { return firstPartity.AsReadOnly(); }
        }

        public ReadOnlyCollection<int> SecondPartityVerticesList
        {
            get { return secondPartity.AsReadOnly(); }
        }

        public ReadOnlyCollection<int> NoPartityVerticesList
        {
            get { return noPartity.AsReadOnly(); }
        }

        public void AddVertex(int vertex)
        {
            vertexToPartity.Add(vertex, EPartity.None);
            noPartity.Add(vertex);
        }

        public void RemoveVertex(int vertex)
        {
            vertexToPartity.Remove(vertex);
            noPartity.Remove(vertex);
        }

        public bool CanAddEdge(int vertex1, int vertex2)
        {
            return canAddEdge = vertexToPartity[vertex1] != vertexToPartity[vertex2];
        }

        public bool AddEdge(int vertex1, int vertex2)
        {
            if (!canAddEdge &&
               !CanAddEdge(vertex1, vertex2))
            {
                return false;
            }

            EPartity vertex1Partity = vertexToPartity[vertex1],
                     vertex2Partity = vertexToPartity[vertex2],
                     oppositeVertexPartity = default(EPartity);

            int vertex = 0;

            bool addingNoPartityVertex = false;

            if (vertex1Partity == EPartity.None)
            {
                vertex = vertex1;
                oppositeVertexPartity = vertexToPartity[vertex2];
                addingNoPartityVertex = true;
            } else if (vertex2Partity == EPartity.None)
            {
                vertex = vertex2;
                oppositeVertexPartity = vertexToPartity[vertex1];
                addingNoPartityVertex = true;
            }

            if (addingNoPartityVertex)
            {
                noPartity.Remove(vertex);

                if (oppositeVertexPartity == EPartity.First)
                {
                    vertexToPartity[vertex] = EPartity.Second;
                    secondPartity.Add(vertex);
                } else
                {
                    vertexToPartity[vertex] = EPartity.First;
                    firstPartity.Add(vertex);
                }
            }

            return true;
        }

        public void RemoveEdge(int vertex1, int vertex2, bool hasNeighbors)
        {

        }
    }
}