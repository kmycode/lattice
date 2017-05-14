using Lattice.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Models.Plate
{
    /// <summary>
    /// 質量をもった節点
    /// </summary>
    class MassNode
    {
        /// <summary>
        /// 元々の位置
        /// </summary>
        public Point3D OriginalPosition { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// 右下の位置（自身、右、右下、下のNodeの平均）
        /// </summary>
        public Point3D LowerRightPosition { get; set; }

        /// <summary>
        /// 振幅
        /// </summary>
        public Size3D Amplitude { get; set; }

        /// <summary>
        /// 振動をはじめてから経過した時間
        /// </summary>
        public double VibrationTime { get; set; }

        /// <summary>
        /// 減衰振動の三角関数で利用する位相
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// 加速度（実際には、元々の位置との差になる）
        /// </summary>
        public Point3D Acceleration { get; set; }

        /// <summary>
        /// 拘束条件
        /// </summary>
        public bool ConstraintX { get; set; }

        /// <summary>
        /// 拘束条件
        /// </summary>
        public bool ConstraintY { get; set; }

        /// <summary>
        /// 質量
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// バネ定数
        /// </summary>
        public double Spring { get; set; }

        /// <summary>
        /// 減衰定数
        /// </summary>
        public double Decay { get; set; }

        /// <summary>
        /// 物理演算で利用するオメガ
        /// </summary>
        public double Omega => Math.Sqrt(this.Spring / this.Mass);

        /// <summary>
        /// 物理演算で利用する、オメガを微分したもの
        /// </summary>
        public double OmegaDash
        {
            get
            {
                var omega = this.Omega;
                var roe = this.Roe;
                return Math.Sqrt(omega * omega - roe * roe);
            }
        }

        /// <summary>
        /// 物理演算で利用するロー
        /// </summary>
        public double Roe => this.Decay / (2 * this.Mass);
    }
}
