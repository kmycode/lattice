﻿using Lattice.Models.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;

namespace Lattice.Models.Plate
{
    /// <summary>
    /// 質量情報を保持した板
    /// </summary>
    class MassPlate : Plate<MassNode>
    {
        /// <summary>
        /// 1フレームにかかる時間
        /// </summary>
        private const double TimePerFrame = 1.0 / 30;

        /// <summary>
        /// 配置間隔
        /// </summary>
        private SizeD SzMeshDist;

        /// <summary>
        /// 物理演算中であるか
        /// </summary>
        public bool IsCalcing { get; private set; }

        /// <summary>
        /// 板に表示する画像
        /// </summary>
        public BitmapImage TextureImage { get; set; }

        public Action CalcAction { get; set; }

        public MassPlate(int dx, int dy) : base(dx, dy)
        {
            this.SzMeshDist = new SizeD
            {
                W = 1.0 / (this.DivisionX - 1),
                H = 1.0 / (this.DivisionY - 1),
            };
            this.InitializeMeshes();
        }

        /// <summary>
        /// 物理演算を開始する
        /// </summary>
        public async void StartSimuration()
        {
            if (this.IsCalcing) return;
            this.IsCalcing = true;

            var sw = new Stopwatch();
            bool isWaiting = false;

            while (this.IsCalcing)
            {
                sw.Reset();
                sw.Start();

                if (this.CalcAction != null)
                {
                    this.CalcAction();
                    this.CalcAction = null;
                }

                // 加速度を計算
                this.TayunCalcAcc();

                // 他の節点の動きにつられて動く加速度を計算
                this.TayunCalcForcedAcc();

                // 前回の画面反映の終了を待ち合わせ
                while (isWaiting) { }
                isWaiting = true;

                // 計算を座標に反映
                this.TayunCalcPos();

                // 画面反映。反映処理終了を待たず、次のフレームの計算を開始する
                Task.Run(() =>
                {
                    this.PositionUpdated?.Invoke(this, new EventArgs());
                }).ContinueWith((task) => isWaiting = false);

                sw.Stop();

                // 本来の時間から計算にかかった時間を引いて待機する
                var waitTime = (int)(1000 * TimePerFrame) - (int)sw.ElapsedMilliseconds;
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime);
                }
            }
        }
        // マウス移動によって呼び出されるメソッド
        // TayunAddAcc          // 特定座標に指定した加速度を加算する
        // TayunSetTransfer     // 節点を移動

        /// <summary>
        /// 物理演算を停止する
        /// </summary>
        public void StopSimuration()
        {
            this.IsCalcing = false;
        }

        /// <summary>
        /// 各座標の移動やアニメーションなどをすべてクリアする
        /// </summary>
        public void Reset()
        {
            foreach (var item in this.Items)
            {
                item.Position = item.OriginalPosition;
                item.Acceleration = new Point3D();
                item.Amplitude = new Size3D();
            }
            this.TayunCalcCenterPos();
            this.PositionUpdated?.Invoke(this, new EventArgs());
        }

        public event EventHandler PositionUpdated;

        #region TOY-CAKE! V2.03からの移植部分

        private void InitializeMeshes()
        {
            // 位置の初期化
            for (int x = 0; x < this.DivisionX; x++)
            {
                for (int y = 0; y < this.DivisionY; y++)
                {
                    this.Items[x, y] = new MassNode
                    {
                        Position = new Point3D
                        {
                            X = this.SzMeshDist.W * x,
                            Y = this.SzMeshDist.H * y,
                            Z = 3 / 100.0,
                        },
                        OriginalPosition = new Point3D
                        {
                            X = this.SzMeshDist.W * x,
                            Y = this.SzMeshDist.H * y,
                            Z = 3 / 100.0,
                        },
                        Mass = 10,
                        Spring = 90,
                        Decay = 10,
                        ConstraintX = x == 0 || x == this.DivisionX - 1,
                        ConstraintY = y == 0 || y == this.DivisionY - 1,
                    };
                }
            }
        }

        /// <summary>
        /// 加速度を計算
        /// 
        /// ;設定された条件で、全ての節点の加速度を計算します。
        /// ;メインループ内で１ループにつき１回だけ実行してください。
        /// </summary>
        private void TayunCalcAcc()
        {
            // 外周をのぞく点において計算する
            for (int x = 1; x <= this.DivisionX - 2; x++)
            {
                for (int y = 1; y <= this.DivisionY - 2; y++)
                {
                    var node = this.Items[x, y];

                    // 振動しない場合は無視
                    if (node.Amplitude.W == 0 && node.Amplitude.H == 0) continue;

                    // 減衰振動
                    var omega = node.Omega;
                    var roe = node.Roe;
                    var omegaDash = node.OmegaDash;
                    var amp = node.Amplitude;
                    var t = node.VibrationTime;
                    var phase = node.Phase;

                    // 振幅が十分に小さければ、振動情報を消去して以降計算しない
                    var currentAmpBase = Math.Pow(Math.E, -roe * t);
                    if (amp.W * currentAmpBase * amp.W * currentAmpBase + amp.H * currentAmpBase * amp.H * currentAmpBase < 0.0001)
                    {
                        node.Amplitude = new Size3D();
                        node.VibrationTime = 0;
                        continue;
                    }

                    var pow_sin = currentAmpBase * Math.Sin(omegaDash * t + phase);

                    // 加速度
                    var acc = node.Acceleration;
                    if (!node.ConstraintX)
                    {
                        var fx = amp.W * pow_sin;
                        acc.X = fx / node.Mass;
                    }
                    if (!node.ConstraintY)
                    {
                        var fy = amp.H * pow_sin;
                        acc.Y = fy / node.Mass;
                    }
                    node.Acceleration = acc;

                    node.VibrationTime += TimePerFrame;
                }
            }
        }
        
        /// <summary>
        /// 強制振動を計算します（オリジナルメソッド）
        /// </summary>
        private void TayunCalcForcedAcc()
        {
            // 外周をのぞく点において計算する
            for (int x = 1; x <= this.DivisionX - 2; x++)
            {
                for (int y = 1; y <= this.DivisionY - 2; y++)
                {
                    var node = this.Items[x, y];

                    // 自分で振動する節点は除外
                    //if (node.Amplitude.W != 0 || node.Amplitude.H != 0) continue;

                    // 隣り合う節点
                    var leftNode = this.Items[x - 1, y];
                    var rightNode = this.Items[x + 1, y];
                    var upperNode = this.Items[x, y - 1];
                    var lowerNode = this.Items[x, y + 1];

                    // 強制振動
                    var omega = node.Omega;
                    var roe = node.Roe;
                    var t = 0; // this.time;

                    var omegaZero = Double.NaN;
                    Complex ft = default(Complex);

                    // 加速度
                    var acc = node.Acceleration;
                    if (!node.ConstraintX)
                    {
                        var force = (rightNode.Position.X + leftNode.Position.X) / 2 - node.Position.X;
                        force += (upperNode.Position.X + lowerNode.Position.X) / 2 - node.Position.X;
                        force *= -1;
                        if (force != 0)
                        {
                            var forceZero = force; // / Math.Cos(omega * t); // 実質 1 なので計算不要
                            omegaZero = Math.Sqrt((rightNode.Spring + leftNode.Spring + upperNode.Spring + lowerNode.Spring) /
                                                  (rightNode.Mass + leftNode.Mass + upperNode.Mass + lowerNode.Mass) / 4 / 4);
                            ft = (omegaZero * omegaZero - omega * omega + new Complex(0, roe * omega)) * Complex.Pow(Math.E, new Complex(0, omega * t));

                            var fx = forceZero / ft;
                            acc.X += fx.Real;
                        }
                    }
                    if (!node.ConstraintY)
                    {
                        var force = (rightNode.Position.Y + leftNode.Position.Y) / 2 - node.Position.Y;
                        force += (upperNode.Position.Y + lowerNode.Position.Y) / 2 - node.Position.Y;
                        force *= -1;
                        if (force != 0)
                        {
                            var forceZero = force; // / Math.Cos(omega * t); // 実質 1 なので計算不要
                            if (Double.IsNaN(omegaZero))
                            {
                                omegaZero = Math.Sqrt((rightNode.Spring + leftNode.Spring + upperNode.Spring + lowerNode.Spring) /
                                                      (rightNode.Mass + leftNode.Mass + upperNode.Mass + lowerNode.Mass) / 4 / 4);
                                ft = (omegaZero * omegaZero - omega * omega + new Complex(0, roe * omega)) * Complex.Pow(Math.E, new Complex(0, omega * t));
                            }

                            var fy = forceZero / ft;
                            acc.Y += fy.Real;
                        }
                    }
                    node.Acceleration = acc;
                }
            }
        }

        /// <summary>
        /// 変形後のメッシュの節点座標を計算します。
        /// メインループ内で、TayunCalcAcc命令の後に実行してください。
        /// </summary>
        private void TayunCalcPos()
        {
            for (var x = 0; x < this.DivisionX; x++)
            {
                for (var y = 0; y < this.DivisionY; y++)
                {
                    var node = this.Items[x, y];

                    // 変位
                    {
                        var pos = node.Position;
                        pos.X = node.OriginalPosition.X + node.Acceleration.X;
                        pos.Y = node.OriginalPosition.Y + node.Acceleration.Y;
                        pos.Z = node.OriginalPosition.Z + (node.Acceleration.X * node.Acceleration.X + node.Acceleration.Y * node.Acceleration.Y) / 100.0;
                        node.Position = pos;
                    }
                }
            }

            this.TayunCalcCenterPos();
        }

        private void TayunCalcCenterPos()
        {
            // 中間点の座標を求める
            for (var x = 0; x < this.DivisionX - 1; x++)
            {
                for (var y = 0; y < this.DivisionY - 1; y++)
                {
                    var leftTop = this.Items[x, y].Position;
                    var rightTop = this.Items[x + 1, y].Position;
                    var leftBottom = this.Items[x, y + 1].Position;
                    var rightBottom = this.Items[x + 1, y + 1].Position;

                    var pos = this.Items[x, y].LowerRightPosition;
                    if (leftTop.Z + rightBottom.Z > rightTop.Z + leftBottom.Z)
                    {
                        pos = (leftTop + rightBottom) / 2;
                    }
                    else
                    {
                        pos = (rightTop + leftBottom) / 2;
                    }
                    this.Items[x, y].LowerRightPosition = pos;
                }
            }
        }

        /// <summary>
        /// 指定範囲の節点に任意の加速度を加算
        /// 
        /// ;選択中心（座標(bx,by)、半径r）の領域内にある節点に対して、加速度（accx, accy）を加えます。
        /// ; 拘束されていない節点方向にのみ処理を行います。
        /// ;
        /// ;加速度による節点の移動量は、影響半径に含まれる節点数によって変化します。
        /// ;つまり影響半径やメッシュの細かさのパラメータの影響や、拘束点をどれだけ含んでいるかにも影響を受けます。
        /// ;また、拘束点の分布状態にも影響を受けます。
        /// </summary>
        /// <param name="bx">節点を選択する基準点（２次元配列のインデクス）</param>
        /// <param name="by">節点を選択する基準点（２次元配列のインデクス）</param>
        /// <param name="r">影響半径</param>
        /// <param name="accx">加速度加算量</param>
        /// <param name="accy">加速度加算量</param>
        public void TayunAddAcc(int bx, int by, double r, double accx, double accy)
        {
            var rr = r * r;
            var cntrX = bx;
            var cntrY = by;

            for (int x = 1; x <= this.DivisionX - 2; x++)
            {
                for (int y = 1; y <= this.DivisionY - 2; y++)
                {
                    var node = this.Items[x, y];

                    // 節点から選択中心までの距離
                    var dx = x - cntrX;
                    var dy = y - cntrY;

                    if (rr > dx * dx + dy * dy)
                    {
                        var roe = node.Roe;
                        var amp = node.Amplitude;
                        var t = node.VibrationTime;

                        // 拘束されている方向に対しては、処理を行わない。
                        // X方向加速度
                        if (!node.ConstraintX)
                        {
                            // 現在の振幅
                            var currentA = amp.W == 0 ? 0 : amp.W * Math.Pow(Math.E, -roe * t);

                            amp.W = currentA + accx;
                            node.VibrationTime = 0;
                        }
                        // Y方向加速度
                        if (!node.ConstraintY)
                        {
                            // 現在の振幅
                            var currentA = amp.H == 0 ? 0 : amp.H * Math.Pow(Math.E, -roe * t);

                            amp.H = currentA + accy;
                            node.VibrationTime = 0;
                        }

                        // （厳密に言うと正確ではないが）それっぽく見せるため位相を調整
                        node.Phase += node.OmegaDash + node.VibrationTime;

                        node.Amplitude = amp;
                    }
                }
            }
        }

        #endregion
    }
}
