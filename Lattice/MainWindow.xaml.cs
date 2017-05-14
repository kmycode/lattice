using Lattice.Models.Plate;
using Lattice.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lattice
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int dx = 50;
            int dy = 50;
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(@"C:\Users\KMY\Pictures\_bg\2.jpg");
            img.UriCachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheOnly);
            img.EndInit();
            var imgds = ImageUtil.DivideImage(img, dx, dy);
            var plate = new MassPlate(dx + 1, dy + 1);

            this.ViewportModels.Children.Clear();
            var bitmap = img;
            Brush brush = new VisualBrush
            {
                Visual = new Image
                {
                    Source = bitmap,
                },
            };
            RenderOptions.SetCachingHint(brush, CachingHint.Cache);

            var points = new Point3DCollection { };
            var indices = new Int32Collection { };
            var coordinates = new PointCollection { };
            var model = new GeometryModel3D
            {
                Material = new DiffuseMaterial
                {
                    Brush = brush,
                },
                Geometry = new MeshGeometry3D
                {
                    Positions = points,
                    TriangleIndices = indices,
                    TextureCoordinates = coordinates,
                },
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

            this.ViewportModels.Children.Add(model);

            plate.SimurationUpdated += (sender, e) =>
            {
                var meshes = this.ViewportModels.Children;
                var mesh = (GeometryModel3D)meshes[0];
                points.Clear();

                for (int y = 0; y < dy + 1; y++)
                {
                    for (int x = 0; x < dx + 1; x++)
                    {
                        var item = plate.Items[x, y];
                        points.Add(new Point3D(item.Position.X, item.Position.Y, item.Position.Z));
                    }
                }

                for (int y = 0; y < dy; y++)
                {
                    for (int x = 0; x < dx; x++)
                    {
                        var dividedPos = plate.Items[x, y].LowerRightPosition;
                        points.Add(new Point3D(dividedPos.X, dividedPos.Y, dividedPos.Z));
                    }
                }
            };
            plate.StartSimuration();
            plate.CalcAction = () =>
            {
                plate.TayunAddAcc(dx / 2, dy / 2, 4, 0.5, 0);
            };
        }
    }
}
