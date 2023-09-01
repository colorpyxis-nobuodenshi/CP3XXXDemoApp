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
    /// ColorMatchWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorMatchWindow : Window
    {
        public ColorMatchWindow()
        {
            InitializeComponent();

            listKijyun.Items.Clear();
            listHikaku.Items.Clear();

            var pl = AppStore.Instance.PixelsList;
            XYZPixels currentKijyunXYZPixels = null;
            XYZPixels currentHikakuXYZPixels = null;

            for (var i=0;i<pl.Count;i++)
            {
                listKijyun.Items.Add(pl[i].Serialnumber);
                listHikaku.Items.Add(pl[i].Serialnumber);
            }
            listKijyun.SelectionChanged += delegate
            {
                var i = listKijyun.SelectedIndex;
                var cp = pl[i];
                currentKijyunXYZPixels = cp.ToXYZPixels();

                imageKijyun.Source = CreateDisplayBitmap(cp);
            };
            listHikaku.SelectionChanged += delegate
            {
                var i = listHikaku.SelectedIndex;
                var cp = pl[i];
                currentHikakuXYZPixels = cp.ToXYZPixels();

                imageHikaku.Source = CreateDisplayBitmap(cp);

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

                selectionROIRect2.Rect = selectionROIRect1.Rect;

                if (currentKijyunXYZPixels is null)
                    return;
                
                if (currentHikakuXYZPixels is null)
                    return;

                if (w <= 0 || h <= 0)
                    return;

                var roi = new ROI(x, y, w, h);
                System.Diagnostics.Debug.Print(roi.ToString());
                var cm = new ColorMatching();
                var res = cm.Execute(currentKijyunXYZPixels, currentHikakuXYZPixels, roi, AppStore.Instance.Whitepoint);
                System.Diagnostics.Debug.Print(res.DEVec.ToString());
                if(res.DEVec.Scalar < 0.5)
                {
                    Result.Text = "よく一致";
                }
                else if(res.DEVec.Scalar >= 0.5 && res.DEVec.Scalar < 1.0)
                {
                    Result.Text = "ほぼ一致";
                }
                else if (res.DEVec.Scalar >= 1.0 && res.DEVec.Scalar < 1.5)
                {
                    Result.Text = "一致";
                }
                else
                {
                    Result.Text = "***";
                }

                void plotDeltaAB(double da, double db)
                {
                    var scale = 200.0 / 20.0;
                    da = da > 10.0 ? 10.0 : da < -10.0 ? -10.0 : da;
                    db = db > 10.0 ? 10.0 : db < -10.0 ? -10.0 : db;
                    var px = (da + 10) * scale - 2;
                    var py = (db + 10) * scale + 2;

                    ABPoint.Rect = new Rect(px, 200 - py, 4, 4);
                }
                void plotDeltaL(double l)
                {
                    var scale = 200.0 / 20.0;
                    l = l > 10.0 ? 10.0 : l < -10.0 ? -10.0 : l;
                    var px = 25 - 2;
                    var py = (l + 10 )* scale + 2;
                    
                    LPoint.Rect = new Rect(px, 200 - py, 4, 4);
                }
                //Vec.Text = $"(ΔL* {res.DEVec.V0:F2}, Δa* {res.DEVec.V1:F2}, Δb* {res.DEVec.V2:F2} )";
                plotDeltaL(res.DEVec.V0);
                plotDeltaAB(res.DEVec.V1, res.DEVec.V2);
                Scalar.Text = $"ΔE {res.DEVec.Scalar:F2} (ΔL* {res.DEVec.V0:F2},Δa* {res.DEVec.V1:F2},Δb* {res.DEVec.V2:F2})";
                KijyunColorValue.Text = $"L* {res.RefColorvalue.lab.L:F0}, a* {res.RefColorvalue.lab.A:F2}, b* {res.RefColorvalue.lab.B:F2}, C* {res.RefColorvalue.lch.L:F2}, h* {res.RefColorvalue.lch.H:F0}";
                HikakuColorValue.Text = $"L* {res.SampleColorvalue.lab.L:F0}, a* {res.SampleColorvalue.lab.A:F2}, b* {res.SampleColorvalue.lab.B:F2}, C* {res.SampleColorvalue.lch.L:F2}, h* {res.SampleColorvalue.lch.H:F0}";
            };
            draw1.MouseMove += (s1, e1) =>
            {
                if(roiStartPoint.HasValue)
                {
                    selectionROIRect1.Rect = new Rect(roiStartPoint.Value, e1.GetPosition(draw1));
                }
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
