using Lattice.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Models.Plate
{
    class PlateClickedEventArgs : EventArgs
    {
        public Point3D Position { get; }
        public PlateClickedEventArgs(Point3D pos)
        {
            this.Position = pos;
        }
    }
}
