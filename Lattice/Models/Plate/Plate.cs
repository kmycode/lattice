using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Models.Plate
{
    interface IPlate<out T>
    {
        int DivisionX { get; }
        int DivisionY { get; }
        T[,] Items { get; }
    }

    /// <summary>
    /// 分割がなされた板
    /// </summary>
    class Plate<T> : IPlate<T>
    {
        /// <summary>
        /// 横方向の分割数
        /// </summary>
        public int DivisionX { get; }

        /// <summary>
        /// 縦方向の分割数
        /// </summary>
        public int DivisionY { get; }

        /// <summary>
        /// 各節点に格納するオブジェクト
        /// </summary>
        public T[,] Items { get; }

        public Plate(int dx, int dy)
        {
            this.DivisionX = dx;
            this.DivisionY = dy;
            this.Items = new T[dx, dy];
        }
    }
}
