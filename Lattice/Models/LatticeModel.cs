using Lattice.Models.Plate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lattice.Models
{
    class LatticeModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 画面上に表示する板
        /// </summary>
        public MassPlate Plate
        {
            get => this._plate;
            private set
            {
                if (this._plate != value)
                {
                    this._plate = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private MassPlate _plate;

        public LatticeModel()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            int dx = 50;
            int dy = 50;

            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(@"C:\Users\KMY\Pictures\_bg\2.jpg");
            img.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheOnly);
            img.EndInit();

            this.Plate = new MassPlate(dx + 1, dy + 1)
            {
                TextureImage = img,
            };

            this.Plate.StartSimuration();

            this.Plate.CalcAction = () =>
            {
                this.Plate.TayunAddAcc(25, 25, 6, 2, 2);
            };
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
