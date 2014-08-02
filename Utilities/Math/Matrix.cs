using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public struct Mat
    {
        double[] m_data;
        public readonly uint Rows;
        public readonly uint Columns;
        public Mat(uint rows, uint columns, IEnumerable<double> terms)
        {
            Rows = rows;
            Columns = columns;
            m_data = terms.ToArray();
            //var it = terms.GetEnumerator();
            //m_data = new double[rows * columns];
            //for (int column = 0; column < columns; column++)
            //{
            //    for (int row = 0; row < rows; row++)
            //    {
            //        it.MoveNext();
            //        m_data[column * rows + row] = it.Current;
            //    }
            //}
        }

        public T[] PreMultiply<T>(T[] vector, Func<double, T, T> mult, Func<T, T, T> add, T zero)
        {
            T[] result = new T[Rows];
            for (int row = 0; row < Rows; row++)
            {
                result[row] = zero;
                for (int column = 0; column < Columns; column++)
                {
                    result[row] = add(result[row], mult(m_data[column * Rows + row], vector[column]));
                }
            }
            return result;
        }
    }
}
