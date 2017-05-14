using Lattice.Models.Plate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Models.Helpers
{
    class PlateClickedEventHelper
    {
        public event EventHandler<PlateClickedEventArgs> Clicked;

        public void OnClicked(double x, double y, double z)
        {
            this.Clicked?.Invoke(this, new PlateClickedEventArgs(new Common.Point3D
            {
                X = x,
                Y = y,
                Z = z,
            }));
        }
    }
}
