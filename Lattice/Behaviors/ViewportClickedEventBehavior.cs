using Lattice.Models.Helpers;
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
    /// <summary>
    /// 3D部分のクリックイベントを処理するビヘイビア
    /// </summary>
    class ViewportClickedEventBehavior : Behavior<Viewport3D>
    {
        /// <summary>
        /// ヘルパ
        /// </summary>
        public PlateClickedEventHelper Helper
        {
            get { return (PlateClickedEventHelper)GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }
        public static readonly DependencyProperty HelperProperty =
            DependencyProperty.Register(nameof(Helper),
                                        typeof(PlateClickedEventHelper),
                                        typeof(ViewportClickedEventBehavior),
                                        new FrameworkPropertyMetadata(null, null));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseDown += this.AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseDown -= this.AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VisualTreeHelper.HitTest(this.AssociatedObject, null, (result) =>
            {
                if (result is RayHitTestResult res)
                {
                    if (res is RayMeshGeometry3DHitTestResult meshRes)
                    {
                        var point3D = meshRes.PointHit;
                        this.Helper?.OnClicked(point3D.X, point3D.Y, point3D.Z);
                    }
                }
                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(e.GetPosition(this.AssociatedObject)));
        }
    }
}
