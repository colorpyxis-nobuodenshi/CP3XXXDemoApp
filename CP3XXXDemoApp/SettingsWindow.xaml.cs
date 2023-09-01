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
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            var wp = AppStore.Instance.Whitepoint;
            Whitepoint.Text = $"({wp.X}, {wp.Y}, {wp.Z})";

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

                imageSample.Source = CreateDisplayBitmap(cp);
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
                var subPix = currentsampleXYZPixels.Subpixels(roi);
                var X = (int)subPix.Pixels.Select(x => x.X).Average();
                var Y = (int)subPix.Pixels.Select(x => x.Y).Average();
                var Z = (int)subPix.Pixels.Select(x => x.Z).Average();

                wp = new Whitepoint(X, Y, Z);

                
                Whitepoint.Text = $"({X}, {Y}, {Z})";
            };
            draw1.MouseMove += (s1, e1) =>
            {
                if (roiStartPoint.HasValue)
                {
                    selectionROIRect1.Rect = new Rect(roiStartPoint.Value, e1.GetPosition(draw1));
                }
            };
            btnSetWhitepoint.Click += delegate 
            {
                AppStore.Instance.Whitepoint = wp;
                MessageBox.Show("白色点をセットしました。");
            };
        }

        public WriteableBitmap CreateDisplayBitmap(CapturedPixels pixels)
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
    }
}
