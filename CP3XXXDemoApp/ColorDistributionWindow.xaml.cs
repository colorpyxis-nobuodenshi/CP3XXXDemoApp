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
using System.Windows.Shapes;

namespace CP3XXXDemoApp
{
    /// <summary>
    /// ColorDistributionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorDistributionWindow : Window
    {
        public ColorDistributionWindow()
        {
            InitializeComponent();
            listSample.Items.Clear();

            var pl = AppStore.Instance.PixelsList;
            XYZPixels currentsampleXYZPixels = null;

            for (var i = 0; i < pl.Count; i++)
            {
                listSample.Items.Add(pl[i].Serialnumber);
            }

            listSample.SelectionChanged += delegate
            {
                var i = listSample.SelectedIndex;
                var cp = pl[i];
                currentsampleXYZPixels = cp.ToXYZPixels();

                imageSample.Source = CreateWritableBitmap(cp);
            };

            Point? roiStartPoint = null;
            draw1.MouseLeftButtonDown += (s1, e1) =>
            {
                selectionROIRect1.Rect = Rect.Empty;
                roiStartPoint = e1.GetPosition(draw1);
                draw1.CaptureMouse();

            };
            draw1.MouseLeftButtonUp += (s1, e1) =>
            {
                draw1.ReleaseMouseCapture();
                roiStartPoint = null;

                var f = 5.12;
                var x = (int)(selectionROIRect1.Rect.X * f);
                var y = (int)(selectionROIRect1.Rect.Y * f);
                var w = (int)(selectionROIRect1.Rect.Width * f);
                var h = (int)(selectionROIRect1.Rect.Height * f);

                if (currentsampleXYZPixels is null)
                    return;

                if (w <= 0 || h <= 0)
                    return;

                var roi = new ROI(x, y, w, h);
                //System.Diagnostics.Debug.Print(roi.ToString());
                var cs = new ColorDistribution();
                var res = cs.Execute(currentsampleXYZPixels, roi, AppStore.Instance.Whitepoint);
                imageColorDistribution.Source = CreateColordistributionBitmap(res.ABHistgram, (res.ARange.Min, res.ARange.Max, res.BRange.Min, res.ARange.Max));
                imageLHistgram.Source = CreateLHistgramBitmap(res.LHistgram);
                colorDistributionArea.Visibility = Visibility.Visible;
                LHistgramArea.Visibility = Visibility.Visible;
                Result.Text = $"L* {res.LAB.L:F0}, a* {res.LAB.A:F2}, b* {res.LAB.B:F2}, C* {res.LCH.C:F2}, h* {res.LCH.H:F0}";
            };
            draw1.MouseMove += (s1, e1) =>
            {
                if (roiStartPoint.HasValue)
                {
                    selectionROIRect1.Rect = new Rect(roiStartPoint.Value, e1.GetPosition(draw1));
                }
            };
            colorDistributionArea.Visibility = Visibility.Hidden;
            LHistgramArea.Visibility = Visibility.Hidden;
        }

        public WriteableBitmap CreateWritableBitmap(CapturedPixels pixels)
        {
            var w = pixels.Width;
            var h = pixels.Height;
            var pic = pixels.Pixels;
            var picmax = 255;// (double)pic.Max();
            var writeableBitmap = new WriteableBitmap(
                    (int)w,
                    (int)h,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

            try
            {
                writeableBitmap.Lock();

                unsafe
                {
                    var pBackBuffer = (int*)writeableBitmap.BackBuffer;
                    var frameSize = w * h;

                    for (var y = 0; y < h; y++)
                    {
                        for (var x = 0; x < w; x++)
                        {
                            var s1 = pic[x + y * w];
                            var s2 = pic[x + y * w + frameSize];
                            var s3 = pic[x + y * w + 2 * frameSize];
                            var color_data = (byte)(s1 * 255 / picmax) << 16; // R
                            color_data |= (byte)(s2 * 255 / picmax) << 8;   // G
                            color_data |= (byte)(s3 * 255 / picmax) << 0;   // B
                            *(pBackBuffer) = color_data;
                            pBackBuffer++;

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)writeableBitmap.Width, (int)writeableBitmap.Height));

                writeableBitmap.Unlock();
            }

            return writeableBitmap;
        }
        public WriteableBitmap CreateColordistributionBitmap(int[,] p, (int xmin, int xmax, int ymin, int ymax) range)
        {
            var w = p.GetLength(0);
            var h = p.GetLength(1);
            var pic = p;

            var wb = BitmapFactory.New(300, 300);
            //var wb = new WriteableBitmap(
            //        (int)330,
            //        (int)330,
            //        96,
            //        96,
            //        PixelFormats.Bgr32,
            //        null);

            var max = 0;
            var xx = 0;
            var yy = 0;
            for (var b = 0; b < h; b++)
            {
                for (var a = 0; a < w;a++)
                {
                    var q = pic[a, b];
                    if (q > 0)
                    {
                        if (pic[a, b] > max)
                        {
                            max = pic[a, b];
                            xx = b;
                            yy = a;
                        }
                    }
                }
            }
            
            try
            {
                //wb.DrawLine(0, 150, 300, 150, Colors.DarkGray);
                //wb.DrawLine(150, 0, 150, 300, Colors.DarkGray);

                ////wb.DrawLineDotted(0, 60 - lo, 120, 60 - lo, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(0, 60 - lo * 2, 120, 60 - lo * 2, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(0, 60 - lo * 3, 120, 60 - lo * 3, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(0, 60 + lo, 256, 127 + lo, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(0, 60 + lo * 2, 256, 127 + lo * 2, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(0, 60 + lo * 3, 256, 127 + lo * 3, 3, 3, Colors.DarkGray);

                ////wb.DrawLineDotted(127 - lo, 0, 127 - lo, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 - lo * 2, 0, 127 - lo * 2, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 - lo * 3, 0, 127 - lo * 3, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 - lo * 4, 0, 127 - lo * 4, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 + lo, 0, 127 + lo, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 + lo * 2, 0, 127 + lo * 2, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 + lo * 3, 0, 127 + lo * 3, 256, 3, 3, Colors.DarkGray);
                ////wb.DrawLineDotted(127 + lo * 4, 0, 127 + lo * 4, 256, 3, 3, Colors.DarkGray);

                //wb.DrawEllipseCentered(165, 165, 60, 60, Colors.DarkGray);
                //wb.DrawEllipseCentered(165, 165, 120, 120, Colors.DarkGray);
                //wb.DrawEllipseCentered(165, 165, 150, 150, Colors.DarkGray);

                var scale = 300.0 / 200.0;
                var lut = PseudoColorLUT.RainbowLUT;
                for (var b = 0; b < h; b++)
                {
                    for (var a = 0; a < w; a++)
                    {
                        var v = pic[a, b];
                        if(v == max)
                        {
                            var e = max;
                        }
                        v = (int)(v / (double)max * 255.0);
                        if (v > 0)
                        {
                            v = v < 0 ? 0 : v > 255 ? 255 : v;
                            var rgb = lut[v];
                            var px = (int)((a - 100) * scale) + 150;
                            var py = (int)((b - 100) * scale) + 150;
                            wb.FillEllipseCentered(px, 300 - py, 1, 1, Color.FromRgb((byte)rgb.r, (byte)rgb.g, (byte)rgb.b));
                            //wb.FillRectangle(px - 1, 300 - py - 1, px + 2, 300 - py + 2, Color.FromRgb((byte)rgb.r, (byte)rgb.g, (byte)rgb.b));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {

            }

            return wb;
        }
        public WriteableBitmap CreateLHistgramBitmap(int[] p)
        {
            var wb = BitmapFactory.New(300, 60);
            var maxi = p.Max();
            var scale = 60.0 / maxi;
            for(var i=0;i<100;i++)
            {
                var v = p[i] * scale;
                wb.DrawLine(i * 300 / 100, (int)(60 - v), i * 300 / 100, 60, Colors.Gray);
            }
            wb.DrawLine(0, 60-1, 300, 60-1, Colors.DarkGray);
            //wp.Invalidate();
            return wb;
        }
    }
}
