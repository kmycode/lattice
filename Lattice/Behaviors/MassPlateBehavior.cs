using Lattice.Models.Plate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lattice.Behaviors
{
    class MassPlateBehavior : Behavior<GeometryModel3D>
    {
        public MassPlate Plate
        {
            get { return (MassPlate)GetValue(MassPlateProperty); }
            set { SetValue(MassPlateProperty, value); }
        }
        public static readonly DependencyProperty MassPlateProperty =
            DependencyProperty.Register(nameof(Plate),
                                        typeof(MassPlate),
                                        typeof(MassPlateBehavior),
                                        new FrameworkPropertyMetadata(null, (sender, e) =>
                                        {
                                            ((MassPlateBehavior)sender).OnPlateChanged(e.OldValue as MassPlate);
                                        }));

        /// <summary>
        /// Plate変更時に呼び出される。板の初期化処理を行う
        /// </summary>
        /// <param name="oldValue"></param>
        private void OnPlateChanged(MassPlate oldValue = null)
        {
            if (oldValue != null)
            {
                oldValue.PositionUpdated -= this.Plate_PositionUpdated;
            }

            if (this.Plate == null) return;

            var dx = this.Plate.DivisionX - 1;
            var dy = this.Plate.DivisionY - 1;

            var points = new Point3DCollection { };
            var indices = new Int32Collection { };
            var coordinates = new PointCollection { };

            Brush brush = new VisualBrush
            {
                Visual = new Image
                {
                    Source = this.Plate.TextureImage,
                },
            };
            RenderOptions.SetCachingHint(brush, CachingHint.Cache);

            this.AssociatedObject.Geometry = new MeshGeometry3D
            {
                Positions = points,
                TriangleIndices = indices,
                TextureCoordinates = coordinates,
            };
            this.AssociatedObject.Material = new DiffuseMaterial
            {
                Brush = brush,
            };

            // Coordinateの配列を作成
            for (int y = 0; y < dy + 1; y++)
            {
                for (int x = 0; x < dx + 1; x++)
                {
                    coordinates.Add(new Point((double)x / dx, 1 - (double)y / dy));
                }
            }
            // 矩形の真ん中に置く頂点を作成
            for (int y = 0; y < dy; y++)
            {
                for (int x = 0; x < dx; x++)
                {
                    coordinates.Add(new Point((x + 0.5) / dx,
                                              1 - (y + 0.5) / dy));
                }
            }

            // Indicesの配列を作成
            for (int y = 0; y < dy; y++)
            {
                for (int x = 0; x < dx; x++)
                {
                    var leftTop = y * (dx + 1) + x;
                    var rightTop = leftTop + 1;
                    var leftBottom = leftTop + (dx + 1);
                    var rightBottom = leftBottom + 1;
                    var center = (dy + 1) * (dx + 1) + y * dx + x;
                    indices.Add(leftTop);
                    indices.Add(rightTop);
                    indices.Add(center);
                    indices.Add(rightTop);
                    indices.Add(rightBottom);
                    indices.Add(center);
                    indices.Add(rightBottom);
                    indices.Add(leftBottom);
                    indices.Add(center);
                    indices.Add(leftBottom);
                    indices.Add(leftTop);
                    indices.Add(center);
                }
            }
            // （頂点座標はアニメーションフレーム更新ごとに設定されるので今は設定不要）

            this.Plate.PositionUpdated += this.Plate_PositionUpdated;
        }

        private async void Plate_PositionUpdated(object sender, EventArgs e)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.UpdatePositions();
                });
            }
            catch
            {
            }
        }

        private void UpdatePositions()
        {
            var dx = this.Plate.DivisionX - 1;
            var dy = this.Plate.DivisionY - 1;

            var mesh = this.AssociatedObject;
            var points = ((MeshGeometry3D)mesh.Geometry).Positions;
            points.Clear();

            for (int y = 0; y < dy + 1; y++)
            {
                for (int x = 0; x < dx + 1; x++)
                {
                    var item = this.Plate.Items[x, y];
                    points.Add(new Point3D(item.Position.X, item.Position.Y, item.Position.Z));
                }
            }

            for (int y = 0; y < dy; y++)
            {
                for (int x = 0; x < dx; x++)
                {
                    var dividedPos = this.Plate.Items[x, y].LowerRightPosition;
                    points.Add(new Point3D(dividedPos.X, dividedPos.Y, dividedPos.Z));
                }
            }
        }
    }
}
