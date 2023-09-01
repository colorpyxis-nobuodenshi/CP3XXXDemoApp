using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace CP3XXXDemoApp
{
    /// <summary>
    /// CaptureWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CaptureWindow : Window
    {
        public CaptureWindow()
        {
            InitializeComponent();

            Title = "RTC21 Capture";
            //txtCommand.Text = string.Empty;
            //txtResult.Text = string.Empty;

            cmbExposureTime.Items.Add("1/15 sec");
            cmbExposureTime.Items.Add("1/7.5 sec");
            cmbExposureTime.Items.Add("1/4 sec");
            cmbExposureTime.Items.Add("1/2 sec");
            cmbExposureTime.Items.Add("1 sec");
            //cmbExposureTime.Items.Add("1/30 sec");
            //cmbExposureTime.Items.Add("1/60 sec");
            //cmbExposureTime.Items.Add("1/100 sec");
            //cmbExposureTime.Items.Add("1/120 sec");
            //cmbExposureTime.Items.Add("1/250 sec");
            //cmbExposureTime.Items.Add("1/500 sec");
            //cmbExposureTime.Items.Add("1/1000 sec");
            cmbExposureTime.SelectedIndex = 0;

            //var cam = new RTC21.RTC21();
            var cam = new RTC21.RTC212();
            try
            {
                cam.Initialize();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //var capturePixels = new CapturedPixels(1024, 768, null);

            

            var rawImage = new ushort[cam.EffectiveSizeX * cam.EffectiveSizeY * 3];
            var camSizeX = cam.EffectiveSizeX;
            var camSizeY = cam.EffectiveSizeY;
            bool doLive = false;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            btnLive.Content = "ライブ";
            timer.Tick += delegate
            {
                var pic = cam.TakeCapture(4);
                rawImage = pic;
                RenderOptions.SetBitmapScalingMode(imageCapture, BitmapScalingMode.NearestNeighbor);
                RenderOptions.SetEdgeMode(imageCapture, EdgeMode.Aliased);


                var writeableBitmap = new WriteableBitmap(
                    (int)camSizeX,
                    (int)camSizeY,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                imageCapture.Source = writeableBitmap;

                try
                {
                    writeableBitmap.Lock();

                    unsafe
                    {
                        var pBackBuffer = (int*)writeableBitmap.BackBuffer;
                        var frameSize = camSizeX * camSizeY;

                        for (var y = 0; y < camSizeY; y++)
                        {
                            for (var x = 0; x < camSizeX; x++)
                            {
                                var s1 = pic[x + y * camSizeX];
                                var s2 = pic[x + y * camSizeX + frameSize];
                                var s3 = pic[x + y * camSizeX + 2 * frameSize];
                                var color_data = (byte)(s1 * 255 / 1023) << 16; // R
                                color_data |= (byte)(s2 * 255 / 1023) << 8;   // G
                                color_data |= (byte)(s3 * 255 / 1023) << 0;   // B
                                //color_data |= 0 << 24;
                                //byte* p = pBackBuffer + (y * writeableBitmap.BackBufferStride) + (x * 4);
                                *(pBackBuffer) = color_data;
                                //p[2] = (byte)(s1 * 255 / 1023);
                                //p[1] = (byte)(s1 * 255 / 1023);
                                //p[0] = (byte)(s1 * 255 / 1023);
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
            };
            btnLive.Click += delegate
            {
                if (!doLive)
                {
                    timer.Start();
                    btnLive.Content = "ストップ";
                    btnCapture.IsEnabled = false;
                    //btnReset.IsEnabled = false;
                    //btnCommand.IsEnabled = false;
                    doLive = true;
                    return;
                }

                timer.Stop();
                btnLive.Content = "ライブ";
                btnCapture.IsEnabled = true;
                //btnReset.IsEnabled = true;
                //btnCommand.IsEnabled = true;
                doLive = false;
            };
            btnCapture.Click += delegate
            {
                var pic = cam.TakeCapture(4);
                pic = cam.CorrectShading(pic, AppStore.Instance.ShadingPixels.Pixels);

                RenderOptions.SetBitmapScalingMode(imageCapture, BitmapScalingMode.NearestNeighbor);
                RenderOptions.SetEdgeMode(imageCapture, EdgeMode.Aliased);


                var writeableBitmap = new WriteableBitmap(
                    (int)camSizeX,
                    (int)camSizeY,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                imageCapture.Source = writeableBitmap;

                try
                {
                    writeableBitmap.Lock();

                    unsafe
                    {
                        var pBackBuffer = (int*)writeableBitmap.BackBuffer;
                        var frameSize = camSizeX * camSizeY;

                        for (var y = 0; y < camSizeY; y++)
                        {
                            for (var x = 0; x < camSizeX; x++)
                            {
                                var s1 = pic[x + y * camSizeX];
                                var s2 = pic[x + y * camSizeX + frameSize];
                                var s3 = pic[x + y * camSizeX + 2 * frameSize];
                                var color_data = (byte)(s1 * 255 / 1023) << 16; // R
                                color_data |= (byte)(s2 * 255 / 1023) << 8;   // G
                                color_data |= (byte)(s3 * 255 / 1023) << 0;   // B
                                //color_data |= 0 << 24;
                                //byte* p = pBackBuffer + (y * writeableBitmap.BackBufferStride) + (x * 4);
                                *(pBackBuffer) = color_data;
                                //p[2] = (byte)(s1 * 255 / 1023);
                                //p[1] = (byte)(s1 * 255 / 1023);
                                //p[0] = (byte)(s1 * 255 / 1023);
                                pBackBuffer++;

                            }
                        }

                    }
                }
                finally
                {
                    writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)writeableBitmap.Width, (int)writeableBitmap.Height));

                    writeableBitmap.Unlock();
                }



                //if (MessageBox.Show("保存しますか？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                //{
                //    var sfd = new SaveFileDialog();
                //    sfd.Filter = "RAW XYZ Picture(.raw) | *.raw";
                //    sfd.FileName = $"PIX\\IMG{DateTime.Now.ToString("MMddHHmmss")}.raw";
                //    var res = sfd.ShowDialog().Value;
                //    if (res)
                //    {
                //        string fileName = sfd.FileName;
                //        using (var bw = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
                //        {
                //            foreach (var v in pic)
                //            {
                //                bw.Write(v);
                //            }
                //        }
                //    }
                //}
                string fileName = $"PIX\\IMG{DateTime.Now.ToString("MMddHHmmss")}.raw";
                using (var bw = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
                {
                    foreach (var v in pic)
                    {
                        bw.Write(v);
                    }
                }
                AppStore.Instance.PixelsList.Add(new CapturedPixels(1024, 768, pic, System.IO.Path.GetFileName(fileName)));
            };

            //btnReset.Click += delegate
            //{
            //    cam.ResetRegister();
            //};

            //btnCommand.Click += delegate
            //{
            //    txtResult.Text = string.Empty;
            //    var cmd = txtCommand.Text.Trim();
            //    var res = cam.SendRecvCommand(System.Text.Encoding.ASCII.GetBytes(cmd));
            //    txtResult.Text = System.Text.Encoding.ASCII.GetString(res);
            //    txtCommand.Text = string.Empty;
            //};
            void updateXYZValue(Point pos)
            {
                var x = (int)pos.X * 2;
                var y = (int)pos.Y * 2;
                var frameSize = cam.EffectiveSizeX * cam.EffectiveSizeY;
                var s1 = rawImage[x + cam.EffectiveSizeX * y];
                var s2 = rawImage[x + cam.EffectiveSizeX * y + frameSize];
                var s3 = rawImage[x + cam.EffectiveSizeX * y + 2 * frameSize];

                txtXYZValue.Text = $"({s1}, {s2}, {s3})";
            }
            imageCapture.MouseMove += (s, e) =>
            {
                try
                {
                    var pos = e.GetPosition(imageCapture);
                    updateXYZValue(pos);
                }
                catch
                {

                }
            };
            
            cmbExposureTime.SelectionChanged += delegate
            {
                try
                {
                    cam.ChangeExposureTime(cmbExposureTime.SelectedIndex);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            };


            btnMakeShadingpixels.Click += delegate
            {
                var p = cam.TakeCapture(4);
                var p2 = RTC21.RTC21.MakeShadingData(p);
                var shd = new ShadingPixels(cam.EffectiveSizeX, cam.EffectiveSizeY, p2);
                AppStore.Instance.ShadingPixels = shd;
                var writer = new ShadingPixelsWriter();
                writer.Execute(shd);
            };

            Closing += delegate
            {
                cam.Deinitialize();
            };
        }
    }
}
