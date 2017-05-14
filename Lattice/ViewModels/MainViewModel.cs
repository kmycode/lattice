using Lattice.Models;
using Lattice.Models.Plate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        LatticeModel model = new LatticeModel();

        public MassPlate Plate => this.model.Plate;
    }
}
