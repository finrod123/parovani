using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// 1) pouzivat matice pro generovani grafu
//    - metoda GenerujGraf pro naplneni matice sousednosti;
// 2) na matici je mozne pouzit ruzne dalsi operace:
//    a) prehazet nahodne hrany
// 3) metoda Create Graphs vrati retezcovou reprezentaci grafu
// 4) metoda CreateGraphs muze vracet odkazem primo i matici sousednosti (-> alternativni vystupni format
//    jak pro koncoveho uzivatele knihovny, tak pro vzajemnou spolupraci generatoru grafu
// 5) casem se mozna zavede metoda pro generovani matice sousednosti z retezcove reprezentace samostatne
//    a site se budou konstruovat az z teto reprezentace


namespace BipartGraphCreator
{
    using GraphRepresentation;

    public interface IGraphMutator<T>
        where T : class
    {
        void MutateGraph(ref IMatrix<T> graphMatrix);
    }

    public class SimpleBipartGraphCreator<T> :
        IBipartGraphCreator<T>
        where T : class
    {

        public void GetNext(out IMatrix<T> graphMatrix)
        {
            throw new NotImplementedException();
        }

        public void WriteNext(StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        public List<IMatrix<T>> GetNextRange(int count)
        {
            throw new NotImplementedException();
        }

        public void WriteNextRange(StreamWriter sw, int count)
        {
            throw new NotImplementedException();
        }
    }

    public class FlippingBipartGraphCreator<T> :
        IBipartGraphCreator<T>
        where T : class
    {

        EdgeFlipGraphMutator<T> flipMutator;

        #region IBipartGraphCreator<T> Members

        public void GetNext(out IMatrix<T> graphMatrix)
        {

            flipMutator.MutateGraph(graphMatrix);
        }

        public void WriteNext(StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        public List<IMatrix<T>> GetNextRange(int count)
        {
            throw new NotImplementedException();
        }

        public void WriteNextRange(StreamWriter sw, int count)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class EdgeFlipGraphMutator<T> :
        IGraphMutator<T>
        where T : class
    {
        public void MutateGraph(ref IMatrix<T> matrix)
        {
            int rows = graphMatrix.Rows - 1,
                cols = graphMatrix.Cols;

            for (int i = 0; i < rows; ++i)
            {
                for (int j = i + 1; j < cols; ++j)
                {
                    flipEdges(i, j, matrix);
                }
            }
        }

        bool findCrossingEdges(int vert1, int vert2, IMatrix<T> matrix, out int vert1partner, out int vert2partner)
        {
            vert1partner = vert2partner = -1;

            if (!findHighestEdge(vert1, matrix, out vert1partner) ||
                !findLowestEdge(vert2, matrix, out vert2partner))
            {
                return false;
            }

            return vert1partner > vert2partner;
        }


        bool findLowestEdge(int vert, IMatrix<T> matrix, out int edgeVert)
        {
            return findEdge(vert, vert, matrix.Cols, 1, matrix, out edgeVert);
        }

        bool findHighestEdge(int vert, IMatrix<T> matrix, out int edgeVert)
        {
            return findEdge(vert, matrix.Cols, vert, -1, matrix, out edgeVert);
        }

        bool findEdge(int vert, int from, int to, int step, IMatrix<T> matrix, out int edgeVert)
        {
            edgeVert = -1;

            for (int i = from; i != to ; i+=step)
            {
                if (matrix[vert, i] != null)
                {
                    edgeVert = i;
                    return true;
                }
            }

            return false;
        }

        void flipEdges(int vert1, int vert2, IMatrix<T> matrix)
        {
            int vert1partner, vert2partner;

            while (findCrossingEdges(vert1, vert2, matrix, out vert1partner, out vert2partner))
            {
                matrix[vert1, vert2partner] = matrix[vert1, vert1partner];
                matrix[vert1, vert1partner] = null;

                matrix[vert2, vert1partner] = matrix[vert2, vert2partner];
                matrix[vert2, vert2partner] = null;
            }
        }
    }
}


