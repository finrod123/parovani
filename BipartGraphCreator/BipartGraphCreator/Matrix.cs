using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MatrixNS
{
    public interface IMatrix<T>
    where T : struct
    {
        T this[int x, int y] { get; set; }
        void Clear();
        int Rows { get; }
        int Cols { get; }
        int AddRow();
        void InsertRow(int at);
        void RemoveRow(int row);
        int AddCol();
        void InsertCol(int at);
        void RemoveCol(int col);
    }

    public class Matrix<T> : IMatrix<T>
            where T : struct
    {
        List<T> elements;
        int rows, cols;

        public Matrix(int m, int n)
        {

            Debug.Assert(0 < m &&
                         0 < n);
            rows = m;
            cols = n;
            elements = new List<T>();

            Clear();
        }

        public T this[int i, int j]
        {
            get
            {
                Debug.Assert(0 <= i && i < rows &&
                             0 <= j && j < cols);

                return elements[i * cols + j];
            }
            set
            {
                Debug.Assert(0 <= i && i < rows &&
                             0 <= j && j < cols);

                elements[i * cols + j] = value;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    elements.Add(default(T));
        }

        public int Rows
        {
            get { return rows; }
        }

        public int Cols
        {
            get { return cols; }
        }

        public void RemoveRow(int i)
        {
            elements.RemoveRange(i * cols, cols);
            --rows;
        }

        public int AddRow()
        {
            InsertRow(rows);

            return rows;
        }

        public void InsertRow(int at)
        {
            Debug.Assert(0 <= at &&
                         at <= cols);

            int elementsBeforeRow = at * cols;

            for (int i = 0; i < cols; ++i)
            {
                elements.Insert(elementsBeforeRow, default(T));
            }

            ++rows;
        }

        public void InsertCol(int at)
        {
            Debug.Assert(0 <= at &&
                         at <= cols);

            int insertAt = at;
            ++cols;

            for (int i = 0; i < rows; ++i)
            {
                elements.Insert(insertAt, default(T));
                insertAt += cols;
            }
        }

        public int AddCol()
        {
            InsertCol(cols);

            return cols;
        }

        public void RemoveCol(int at)
        {
            int removeAt = (rows - 1) * cols + at;
            
            for (int i = 0; i < rows; ++i)
            {
                elements.RemoveAt(removeAt);
                removeAt -= cols;
            }

            --cols;
        }
    }
}