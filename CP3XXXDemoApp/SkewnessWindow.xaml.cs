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
    /// SkewnessWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SkewnessWindow : Window
    {
        public SkewnessWindow()
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
                var sk = new Skewness();
                var res = sk.Execute(currentsampleXYZPixels, roi, AppStore.Instance.Whitepoint);
                imageLHistgram.Source = CreateLHistgramBitmap(res.Lhist);
                LHistgramArea.Visibility = Visibility.Visible;
                Result.Text = $"{res.Skewness:F3}";
            };
            draw1.MouseMove += (s1, e1) =>
            {
                if (roiStartPoint.HasValue)
                {
                    selectionROIRect1.Rect = new Rect(roiStartPoint.Value, e1.GetPosition(draw1));
                }
            };
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
                            var r = s1 * 255.0 / picmax;
                            var g = s2 * 255.0 / picmax;
                            var b = s3 * 255.0 / picmax;
                            var color_data = (byte)r << 16; // R
                            color_data |= (byte)g << 8;   // G
                            color_data |= (byte)b << 0;   // B
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

        public WriteableBitmap CreateLHistgramBitmap(int[] p)
        {
            var wb = BitmapFactory.New(300, 60);
            var maxi = p.Max();
            var scale = 60.0 / maxi;
            for (var i = 0; i < 100; i++)
            {
                var v = p[i] * scale;
                wb.DrawLine(i * 300 / 100, (int)(60 - v), i * 300 / 100, 60, Colors.Gray);
            }
            wb.DrawLine(0, 60 - 1, 300, 60 - 1, Colors.DarkGray);
            
            return wb;
        }
    }
}
