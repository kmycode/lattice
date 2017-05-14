using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Lattice.Models.Utils
{
    static class ImageUtil
    {
        public static BitmapSource[,] DivideImage(BitmapImage img, int dx, int dy)
        {
            var wb = new WriteableBitmap(img);
            var divs = new BitmapSource[dx, dy];

            var blockWidth = (int)(img.PixelWidth / dx);
            var blockHeight = (int)(img.PixelHeight / dy);

            var stride = (int)((img.PixelWidth * img.Format.BitsPerPixel) / 8);
            var pixels = new byte[(int)img.PixelWidth * (int)img.PixelHeight * 4];
            img.CopyPixels(pixels, stride, 0);

            for (int x = 0; x < dx; x++)
            {
                for (int y = 0; y < dy; y++)
                {
                    var px = blockWidth * x;
                    var py = blockHeight * y;
                    var pw = blockWidth;
                    var ph = blockHeight;
                    if (x == dx - 1)
                    {
                        pw = (int)img.PixelWidth - blockWidth * x;
                    }
                    if (y == dy - 1)
                    {
                        ph = (int)img.PixelHeight - blockHeight * y;
                    }
                    var partialPixels = new byte[pw * ph * 4];
                    GetPartialPixels(pixels, partialPixels, px, py, pw, ph, (int)img.PixelWidth);

                    var bitmap = new WriteableBitmap(pw, ph, img.DpiX, img.DpiY, img.Format, img.Palette);
                    bitmap.WritePixels(new System.Windows.Int32Rect
                    {
                        X = 0,
                        Y = 0,
                        Width = pw,
                        Height = ph,
                    }, partialPixels, (pw * img.Format.BitsPerPixel) / 8, 0);
                    divs[x, y] = bitmap;
                }
            }

            return divs;
        }

        private static void GetPartialPixels(byte[] pixels, byte[] array, int x, int y, int w, int h, int imgWidth)
        {
            //GetPartialPixelsForDebug(array, w, h);
            //return;

            int index = 0;
            for (int yy = y; yy < y + h; yy++)
            {
                for (int xx = x; xx < x + w; xx++)
                {
                    int i = (yy * imgWidth + xx) * 4;
                    for (int v = 0; v < 4; v++)
                    {
                        array[index++] = pixels[i++];
                    }
                }
            }
        }

        private static void GetPartialPixelsForDebug(byte[] array, int w, int h)
        {
            int index = 0;
            for (int yy = 0; yy < h; yy++)
            {
                for (int xx = 0; xx < w; xx++)
                {
                    int i = (yy * w + xx) * 4;
                    bool isBorder = yy <= 1 || yy >= h - 2 || xx <= 3 || xx >= w - 4;
                    for (int v = 0; v < 4; v++)
                    {
                        array[index++] = isBorder ? (byte)255 : (byte)0;
                    }
                }
            }
        }
    }
}
