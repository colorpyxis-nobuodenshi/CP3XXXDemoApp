using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace RTC21
{
    public interface IRTC21
    {
        int EffectiveSizeX { get; }
        int EffectiveSizeY { get; }

        void ChangeExposureTime(int value);
        ushort[] CorrectShading(ushort[] src, float[] shd = null);
        void Deinitialize();
        void Initialize();
        void Reset();
        void ResetRegister();
        byte[] SendRecvCommand(byte[] command);
        ushort[] TakeCapture(int integration, int timeout = 30000);
    }

    public class RTC212 : IRTC21
    {
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_PIXCIopen(string c_driverparms, string c_formatname, string c_formatfile);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_PIXCIclose();

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_mesgFault(int c_unitmap);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_doSnap(int c_unitmap, int c_buffer, int c_timeout);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goSnap(int c_unitmap, int c_buffer);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goLive(int c_unitmap, int c_buffer);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goUnLive(int c_unitmap);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_imageXdim();

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_imageYdim();

        [DllImport("XCLIBW64.dll")]
        private static extern double pxd_imageAspectRatio();

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_capturedFieldCount(int c_unitmap);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_renderStretchDIBits(int c_unitmap, int c_buf, int c_ulx, int c_uly, int c_lrx, int c_lry, int c_options, IntPtr c_hDC, int c_nX, int c_nY, int c_nWidth, int c_nHeight, int c_winoptions);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_saveBmp(int c_unitmap, string c_name, int c_buf, int c_ulx, int c_uly, int c_lrx, int c_lry, int c_savemode, int c_options);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_readuchar(int c_unitmap, int c_framebuf, int c_ulx, int c_uly, int c_lrx, int c_lry, byte[] c_membuf, int c_cnt, string c_colorspace);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_writeuchar(int c_unitmap, int c_framebuf, int c_ulx, int c_uly, int c_lrx, int c_lry, byte[] c_membuf, int c_cnt, string c_colorspace);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_defineImage(int c_unitmap, int c_framebuf, int c_ulx, int c_uly, int c_lrx, int c_lry, string c_colorspace);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_readushort(int c_unitmap, int c_framebuf, int c_ulx, int c_uly, int c_lrx, int c_lry, ushort[] c_membuf, int c_cnt, string c_colorspace);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_serialConfigure(int unitmap, int rsvd0, double baud, int bits, int parity, int stopbits, int rsvd1, int rsvd2, int rsvd3);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_serialRead(int unitmap, int rsvd0, byte[] data, int cnt);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_serialWrite(int unitmap, int rsvd0, byte[] data, int cnt);

        const int EFFECTIVE_IMAGE_WIDTH = 2048;
        const int EFFECTIVE_IMAGE_HEIGHT = 1536;
        public int EffectiveSizeX => EFFECTIVE_IMAGE_WIDTH / 2;

        public int EffectiveSizeY => EFFECTIVE_IMAGE_HEIGHT / 2;

        public void ChangeExposureTime(int value)
        {
            switch (value)
            {
                //case 0:
                //    SendRecvCommand(new byte[] { 0x30,0x34, 0x30, 0x30, 0x30});
                //    break;
                //case 1:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 });
                //    break;
                //case 2:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x32 });
                //    break;
                //case 3:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x33 });
                //    break;
                //case 4:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x34 });
                //    break;
                //case 5:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x35 });
                //    break;
                //case 6:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x36 });
                //    break;
                //case 7:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x37 });
                //    break;
                case 0:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });
                    break;
                case 1:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x31 });
                    break;
                case 2:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x32 });
                    break;
                case 3:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x33 });
                    break;
                case 4:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x34 });
                    break;
            }
        }

        public ushort[] CorrectShading(ushort[] src, float[] shd = null)
        {
            if (shd is null)
                return src;

            var dst = new ushort[src.Length];

            for (var i = 0; i < src.Length; i++)
            {
                dst[i] = (ushort)(src[i] / shd[i]);
            }

            return dst;
        }

        public static float[] MakeShadingData(ushort[] src)
        {
            //var pixels = _camera.TakePicture(condition.ExposureTime, condition.Integration);

            var w = 1024;
            var h = 768;
            var tmp = new float[w * h * 3];
            var framesize = w * h;

            //20x20 median
            var filterSize = 20;

            unsafe
            {
                fixed (ushort* srcBase = src)
                fixed (float* tmpBase = tmp)
                {
                    var srcPtr1 = srcBase;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            var n = 0;
                            var sum1 = 0.0f;
                            var sum2 = 0.0f;
                            var sum3 = 0.0f;
                            for (var k = j - filterSize / 2; k < j + filterSize / 2; k++)
                            {
                                for (var l = i - filterSize / 2; l < i + filterSize / 2; l++)
                                {
                                    var x = (l < 0) ? 0 : ((l >= w) ? w - 1 : l);
                                    var y = (k < 0) ? 0 : ((k >= h) ? h - 1 : k);
                                    sum1 += srcPtr1[x + y * w];
                                    sum2 += srcPtr2[x + y * w];
                                    sum3 += srcPtr3[x + y * w];
                                    n++;
                                }
                            }
                            tmpPtr1[i + j * w] = sum1 / n;
                            tmpPtr2[i + j * w] = sum2 / n;
                            tmpPtr3[i + j * w] = sum3 / n;
                        }
                    }
                }
            }

            //average
            var x1 = 200;
            var x2 = w - x1;
            var y1 = 150;
            var y2 = h - y1;
            var x3 = x2 - x1;
            var y3 = y2 - y1;

            var ave1 = 0.0f;
            var ave2 = 0.0f;
            var ave3 = 0.0f;

            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = y1; j < y2; j++)
                    {
                        for (var i = x1; i < x2; i++)
                        {

                            ave1 += tmpPtr1[i + j * w];
                            ave2 += tmpPtr2[i + j * w];
                            ave3 += tmpPtr3[i + j * w];
                        }
                    }
                }

                ave1 /= x3 * y3;
                ave2 /= x3 * y3;
                ave3 /= x3 * y3;
            }


            //
            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            //tmpPtr1[i + j * w] /= ave1;
                            //tmpPtr2[i + j * w] /= ave2;
                            //tmpPtr3[i + j * w] /= ave3;
                            *tmpPtr1 /= ave1; tmpPtr1++;
                            *tmpPtr2 /= ave2; tmpPtr2++;
                            *tmpPtr3 /= ave3; tmpPtr3++;
                        }
                    }
                }
            }

            return tmp;
        }

        public void Deinitialize()
        {
            pxd_PIXCIclose();
        }

        public void Initialize()
        {
            pxd_PIXCIclose();
            if (pxd_PIXCIopen("", "", "RTC-21-EPX-QXGA.fmt") < 0)
            {
                pxd_mesgFault(1);
                throw new ApplicationException("camera initializing error.");
            }

            if(pxd_serialConfigure(1, 0, 9600, 8, 0, 1, 0, 0, 0) < 0)
            {
                throw new ApplicationException("camera initializing error.");
            }

            Reset();
        }

        public void Reset()
        {
            SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x33 });
            SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });
        }

        public void ResetRegister()
        {
            throw new NotImplementedException();
        }

        public byte[] SendRecvCommand(byte[] command)
        {
            var req = new byte[command.Length + 2];

            if (command.Length == 5)
            {
                req[0] = 0x2;
                req[1] = (byte)command[0];
                req[2] = (byte)command[1];
                req[3] = (byte)command[2];
                req[4] = (byte)command[3];
                req[5] = (byte)command[4];
                req[6] = 0x3;
            }
            else if (command.Length == 3)
            {
                req[0] = 0x2;
                req[1] = (byte)command[0];
                req[2] = (byte)command[1];
                req[3] = (byte)command[2];
                req[4] = 0x3;
            }

            var retray = 0;

            //do
            //{
                while (pxd_serialWrite(1, 0, null, 0) < req.Length)
                {
                    Thread.Sleep(100);
                }

                var result = pxd_serialWrite(1, 0, req, req.Length);
                if (result < 0)
                    throw new ApplicationException("camera communication error. send error.");

            //var resCount = 0;
            //var r = 0;
            //var res = new byte[8];
            //var b = new byte[1];
            //var timeout = 0;

            //    while (true)
            //    {

            //        r = pxd_serialRead(1, 0, b, 1);
            //        if (r < 0)
            //            throw new ApplicationException("camera communication error.recive error.");
            //        //if (r == 0)
            //        //{
            //        //    //if (timeout > 5000)
            //        //    //{
            //        //    //    throw new ApplicationException("communication error. timeout.");
            //        //    //}
            //        //    Thread.Sleep(100);
            //        //    //timeout += 100;
            //        //    continue;
            //        //}
            //        res[resCount] = b[0];
            //        resCount++;


            //        if (b[0] == 0x06)
            //            return res;// new byte[] { 0x6 };

            //        if (b[0] == 0x15)
            //            return new byte[] { 0x15 };
            //    };

            //    retray++;

            //} while (retray < 1);

            return null;

        }

        unsafe public ushort[] TakeCapture(int integration, int timeout = 30000)
        {
            var x = pxd_imageXdim();
            var y = pxd_imageYdim();
            var len = x * y * 3;
            var srcPixels = new ushort[len];
            var dstPixels = new ushort[len];
            var count = 1;

            while (true)
            {
                pxd_doSnap(1, 1, 0);
                pxd_readushort(1, 1, 0, 0, -1, -1, srcPixels, len, "RGB");

                fixed (ushort* dst = dstPixels)
                {
                    fixed (ushort* src = srcPixels)
                    {
                        ushort* srcPtr = src;
                        ushort* dstPtr = dst;

                        for (int i = 0; i < len; i++)
                        {
                            *dstPtr += (ushort)((*srcPtr) / integration);

                            if (*dstPtr > 1023)
                                *dstPtr = 1023;

                            srcPtr++;
                            dstPtr++;
                        }
                    }
                }
                count++;
                if (count > integration)
                    break;

            }

            var dstPixels2 = new ushort[len];
            var framesize = x * y;
            fixed (ushort* srcPtr = dstPixels)
            fixed (ushort* dstPtr = dstPixels2)
            {
                var srcPtr1 = srcPtr;
                var dstPtr1 = dstPtr;
                var dstPtr2 = dstPtr + framesize;
                var dstPtr3 = dstPtr + framesize * 2;
                for (var i = 0; i < framesize; i++)
                {
                    *(dstPtr1) = *(srcPtr1 + 0);
                    *(dstPtr2) = *(srcPtr1 + 1);
                    *(dstPtr3) = *(srcPtr1 + 2);
                    dstPtr1++;
                    dstPtr2++;
                    dstPtr3++;
                    srcPtr1 += 3;
                }
            }

            var p = CorrectBinning2x2(dstPixels2);

            return p;
        }
        private ushort[] CorrectBinning2x2(ushort[] value)
        {
            var src = value;
            var dst = new ushort[EFFECTIVE_IMAGE_WIDTH / 2 * EFFECTIVE_IMAGE_HEIGHT / 2 * 3];
            //
            unsafe
            {
                var w = EFFECTIVE_IMAGE_WIDTH;
                var h = EFFECTIVE_IMAGE_HEIGHT;
                var w2 = EFFECTIVE_IMAGE_WIDTH / 2;
                var h2 = EFFECTIVE_IMAGE_HEIGHT / 2;
                var threshold = (ushort)1023;

                fixed (ushort* srcBase = src)
                fixed (ushort* dstBase = dst)
                {
                    var srcPtr1 = srcBase + w * h * 0;
                    var srcPtr2 = srcBase + w * h * 1;
                    var srcPtr3 = srcBase + w * h * 2;
                    var dstPtr1 = dstBase + w2 * h2 * 0;
                    var dstPtr2 = dstBase + w2 * h2 * 1;
                    var dstPtr3 = dstBase + w2 * h2 * 2;

                    for (var y = 0; y < h - 1; y += 2)
                    {
                        for (var x = 0; x < w - 1; x += 2)
                        {
                            var p1 = *(srcPtr1 + x + y * w);
                            var p2 = *(srcPtr1 + (x + 1) + y * w);
                            var p3 = *(srcPtr1 + x + (y + 1) * w);
                            var p4 = *(srcPtr1 + (x + 1) + (y + 1) * w);
                            var p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr1 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr2 + x + y * w);
                            p2 = *(srcPtr2 + (x + 1) + y * w);
                            p3 = *(srcPtr2 + x + (y + 1) * w);
                            p4 = *(srcPtr2 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr2 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr3 + x + y * w);
                            p2 = *(srcPtr3 + (x + 1) + y * w);
                            p3 = *(srcPtr3 + x + (y + 1) * w);
                            p4 = *(srcPtr3 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr3 + (x / 2) + (y / 2) * w2) = p;


                        }
                    }
                }
            }

            return dst;
        }
    }

    public class RTC21 : IRTC21
    {
        [Flags]
        enum RTCError
        {
            RTC_ERROR_NONE = 0,
            RTC_ERROR_GRABBER_NOT_FOUND,
            RTC_ERROR_GRABBER_NOT_AVAILABLE,
            RTC_ERROR_ILLEGAL_CAMERA_CONFIG,
            RTC_ERROR_FAILED_CAMERA_CONFIG,
            RTC_ERROR_FAILED_ALLOCATE_MEMORY,
            RTC_ERROR_FAILED_ALOCATE_MEMORY,
            RTC_ERROR_MISC,
            RTC_ERROR_FAILED
        };

        [Flags]
        enum GrabberBoard
        {
            MATROX = 0,
            EPIX,
            PLEORA,
            RTCUSB,
            RTC_NUM_OF_GRABBER_BOARD
        };
        public delegate void CaptureCallback(IntPtr args);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_InitializeGrabberBoard", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern RTCError RTC_InitializeGrabberBoard(int boardNumber, int cameraNumber, string cameraFileName, ref int width, ref int height, GrabberBoard grabberboard);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_DeinitializeGrabberBoard", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_DeinitializeGrabberBoard();
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_StartCapture", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern RTCError RTC_StartCapture(CaptureCallback callback, IntPtr parameter);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_CaptureFrame", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_CaptureFrame(IntPtr frameBuffer);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_StopCapture", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_StopCapture();
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_AllocateFrameBufferN", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_AllocateFrameBufferN(ref IntPtr frameBuffer, int width, int height, int frames);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_AllocateFrameBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_AllocateFrameBuffer(ref IntPtr frameBuffer, int width, int height);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_FreeFrameBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_FreeFrameBuffer(IntPtr frameBuffer);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_ClserInitializeSerial", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern IntPtr RTC_ClserInitializeSerial(string filename, ulong serialIndex, string errmsg);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_ClserCloseSerial", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_ClserCloseSerial(IntPtr handle);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_SendCommand", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_SendCommand(byte[] command, ulong msecTimeout, IntPtr clserRef);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_RecvCommand", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_RecvCommand(byte[] command, ulong msecTimeout, IntPtr clserRef);


        const int EFFECTIVE_IMAGE_WIDTH = 2048;
        const int EFFECTIVE_IMAGE_HEIGHT = 1536;
        const int FRAME_SIZE = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT * 3;

        IntPtr _ser = IntPtr.Zero;
        string _serError = string.Empty;
        IntPtr _frameBuffer = IntPtr.Zero;
        public RTC21()
        {

        }

        public int EffectiveSizeX
        {
            get
            {
                return EFFECTIVE_IMAGE_WIDTH / 2;
            }
        }

        public int EffectiveSizeY
        {
            get
            {
                return EFFECTIVE_IMAGE_HEIGHT / 2;
            }
        }

        public void Reset()
        {
            //SendRecvCommand(new byte[] { 0x31, 0x31, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x31, 0x32, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x31, 0x33, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x31, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x32, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x33, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x31, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x32, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x33, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x30 });

            SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x33 });
            SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });

            //SendRecvCommand(new byte[] { 0x43, 0x31, 0x30, 0x30, 0x30 });
        }

        public void ResetRegister()
        {
            //SendRecvCommand(new byte[] { 0x31, 0x31, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x31, 0x32, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x31, 0x33, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x31, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x32, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x32, 0x33, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x31, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x32, 0x30, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x33, 0x33, 0x32, 0x30, 0x30 });
            //SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 });

            //SendRecvCommand(new byte[] { 0x43, 0x31, 0x30, 0x30, 0x30 });
        }
        public void Initialize()
        {
            int width = 0;
            int height = 0;

            if (RTC_InitializeGrabberBoard(0, 0, "", ref width, ref height, GrabberBoard.RTCUSB) != RTCError.RTC_ERROR_NONE)
            //var res = RTC_InitializeGrabberBoard(0, 0, "RTC-21_EPX_QX_30bit.fmt", ref width, ref height, GrabberBoard.EPIX);
            //if (res != RTCError.RTC_ERROR_NONE)
            {
                throw new ApplicationException($"Camera initialization error.");
            }
            if (width != EFFECTIVE_IMAGE_WIDTH)
            {
                throw new ApplicationException("Camera initialization error.");
            }
            if (height != EFFECTIVE_IMAGE_HEIGHT)
            {
                throw new ApplicationException("Camera initialization error.");
            }
            if (RTC_AllocateFrameBufferN(ref _frameBuffer, EFFECTIVE_IMAGE_WIDTH, EFFECTIVE_IMAGE_HEIGHT, 1) == 0)
            {
                throw new OutOfMemoryException("memory allocate exeption.");
            }
            _ser = RTC_ClserInitializeSerial("ArtCamSdk_CnvU3I.dll", 0, _serError);
            //_ser = RTC_ClserInitializeSerial("clserEPX.dll", 0, _serError);
            if (_ser == IntPtr.Zero)
            {
                throw new ApplicationException("Camera communication port initialization error.");
            }
            //if (RTC_StartCapture(_ => { RTC_StopCapture(); }, IntPtr.Zero) != RTCError.RTC_ERROR_NONE)
            //{
            //    throw new ApplicationException("Camera capture error.");
            //}

            Reset();

            TakeCapture(1);
        }

        public void Deinitialize()
        {
            RTC_FreeFrameBuffer(_frameBuffer);

            RTC_DeinitializeGrabberBoard();

            RTC_ClserCloseSerial(_ser);
        }

        public byte[] SendRecvCommand(byte[] command)
        {
            if (RTC_SendCommand(command, 3000, _ser) < 0)
            {
                throw new ApplicationException("Camera communication error.");
            }
            var recv = new byte[8];
            if (RTC_RecvCommand(recv, 3000, _ser) < 0)
            {
                throw new ApplicationException("Camera communication error.");
            }
            if (recv[0] == 0x15)
            {
                throw new ApplicationException("Camera communication error.");
            }
            return recv;
        }
        public void ChangeExposureTime(int value)
        {
            switch (value)
            {
                //case 0:
                //    SendRecvCommand(new byte[] { 0x30,0x34, 0x30, 0x30, 0x30});
                //    break;
                //case 1:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 });
                //    break;
                //case 2:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x32 });
                //    break;
                //case 3:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x33 });
                //    break;
                //case 4:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x34 });
                //    break;
                //case 5:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x35 });
                //    break;
                //case 6:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x36 });
                //    break;
                //case 7:
                //    SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x37 });
                //    break;
                case 0:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });
                    break;
                case 1:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x31 });
                    break;
                case 2:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x32 });
                    break;
                case 3:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x33 });
                    break;
                case 4:
                    SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x34 });
                    break;
            }
        }
        unsafe public ushort[] TakeCapture(int integration, int timeout = 30000)
        {
            var captured = false;
            var p = new ushort[FRAME_SIZE];
            var count = 0;
            IntPtr tmp = _frameBuffer;

            // if(RTC_AllocateFrameBufferN(ref tmp, EFFECTIVE_IMAGE_WIDTH, EFFECTIVE_IMAGE_HEIGHT, 1) == 0)
            // {
            //     throw new OutOfMemoryException("memory allocate exeption.");
            // }

            var callback = new CaptureCallback((_) =>
            {
                RTC_CaptureFrame(tmp);
                fixed (ushort* dstPtr = p)
                {
                    var srcPtr = (ushort*)tmp.ToPointer();
                    for (var i = 0; i < FRAME_SIZE; i++)
                    {
                        dstPtr[i] += srcPtr[i];
                    }
                }
                count++;
                if (count == integration)
                {
                    RTC_StopCapture();
                    captured = true;
                }
            });

            RTC_StartCapture(callback, IntPtr.Zero);

            var startTime = DateTime.Now.Ticks;
            while (!captured)
            {
                System.Threading.Tasks.Task.Delay(100);
                if (DateTime.Now.Ticks - startTime > timeout * 10000)
                {
                    //RTC_FreeFrameBuffer(tmp);
                    throw new ApplicationException("Camera capture error.");
                }
            };


            fixed (ushort* ptr = p)
            {
                for (var i = 0; i < FRAME_SIZE; i++)
                {
                    ptr[i] /= (ushort)integration;
                }
            }

            // RTC_FreeFrameBuffer(tmp);

            var len = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT;
            var p2 = new ushort[FRAME_SIZE];

            fixed (ushort* srcPtr = p)
            fixed (ushort* dstPtr = p2)
            {
                var srcPtr1 = srcPtr;
                var dstPtr1 = dstPtr;
                var dstPtr2 = dstPtr + len;
                var dstPtr3 = dstPtr + len * 2;
                for (var i = 0; i < len; i++)
                {
                    *(dstPtr1) = *(srcPtr1 + 0);
                    *(dstPtr2) = *(srcPtr1 + 1);
                    *(dstPtr3) = *(srcPtr1 + 2);
                    dstPtr1++;
                    dstPtr2++;
                    dstPtr3++;
                    srcPtr1 += 3;
                }
            }

            return CorrectBinning2x2(p2);
        }


        private ushort[] CorrectBinning2x2(ushort[] value)
        {
            var src = value;
            var dst = new ushort[EFFECTIVE_IMAGE_WIDTH / 2 * EFFECTIVE_IMAGE_HEIGHT / 2 * 3];
            //
            unsafe
            {
                var w = EFFECTIVE_IMAGE_WIDTH;
                var h = EFFECTIVE_IMAGE_HEIGHT;
                var w2 = EFFECTIVE_IMAGE_WIDTH / 2;
                var h2 = EFFECTIVE_IMAGE_HEIGHT / 2;
                var threshold = (ushort)1023;

                fixed (ushort* srcBase = src)
                fixed (ushort* dstBase = dst)
                {
                    var srcPtr1 = srcBase + w * h * 0;
                    var srcPtr2 = srcBase + w * h * 1;
                    var srcPtr3 = srcBase + w * h * 2;
                    var dstPtr1 = dstBase + w2 * h2 * 0;
                    var dstPtr2 = dstBase + w2 * h2 * 1;
                    var dstPtr3 = dstBase + w2 * h2 * 2;

                    for (var y = 0; y < h - 1; y += 2)
                    {
                        for (var x = 0; x < w - 1; x += 2)
                        {
                            var p1 = *(srcPtr1 + x + y * w);
                            var p2 = *(srcPtr1 + (x + 1) + y * w);
                            var p3 = *(srcPtr1 + x + (y + 1) * w);
                            var p4 = *(srcPtr1 + (x + 1) + (y + 1) * w);
                            var p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr1 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr2 + x + y * w);
                            p2 = *(srcPtr2 + (x + 1) + y * w);
                            p3 = *(srcPtr2 + x + (y + 1) * w);
                            p4 = *(srcPtr2 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr2 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr3 + x + y * w);
                            p2 = *(srcPtr3 + (x + 1) + y * w);
                            p3 = *(srcPtr3 + x + (y + 1) * w);
                            p4 = *(srcPtr3 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr3 + (x / 2) + (y / 2) * w2) = p;


                        }
                    }
                }
            }

            return dst;
        }

        public ushort[] CorrectShading(ushort[] src, float[] shd = null)
        {
            if (shd is null)
                return src;

            var dst = new ushort[src.Length];

            for (var i = 0; i < src.Length; i++)
            {
                dst[i] = (ushort)(src[i] / shd[i]);
            }

            return dst;
        }

        public static float[] MakeShadingData(ushort[] src)
        {
            //var pixels = _camera.TakePicture(condition.ExposureTime, condition.Integration);

            var w = 1024;
            var h = 768;
            var tmp = new float[w * h * 3];
            var framesize = w * h;

            //20x20 median
            var filterSize = 20;

            unsafe
            {
                fixed (ushort* srcBase = src)
                fixed (float* tmpBase = tmp)
                {
                    var srcPtr1 = srcBase;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            var n = 0;
                            var sum1 = 0.0f;
                            var sum2 = 0.0f;
                            var sum3 = 0.0f;
                            for (var k = j - filterSize / 2; k < j + filterSize / 2; k++)
                            {
                                for (var l = i - filterSize / 2; l < i + filterSize / 2; l++)
                                {
                                    var x = (l < 0) ? 0 : ((l >= w) ? w - 1 : l);
                                    var y = (k < 0) ? 0 : ((k >= h) ? h - 1 : k);
                                    sum1 += srcPtr1[x + y * w];
                                    sum2 += srcPtr2[x + y * w];
                                    sum3 += srcPtr3[x + y * w];
                                    n++;
                                }
                            }
                            tmpPtr1[i + j * w] = sum1 / n;
                            tmpPtr2[i + j * w] = sum2 / n;
                            tmpPtr3[i + j * w] = sum3 / n;
                        }
                    }
                }
            }

            //average
            var x1 = 200;
            var x2 = w - x1;
            var y1 = 150;
            var y2 = h - y1;
            var x3 = x2 - x1;
            var y3 = y2 - y1;

            var ave1 = 0.0f;
            var ave2 = 0.0f;
            var ave3 = 0.0f;

            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = y1; j < y2; j++)
                    {
                        for (var i = x1; i < x2; i++)
                        {

                            ave1 += tmpPtr1[i + j * w];
                            ave2 += tmpPtr2[i + j * w];
                            ave3 += tmpPtr3[i + j * w];
                        }
                    }
                }

                ave1 /= x3 * y3;
                ave2 /= x3 * y3;
                ave3 /= x3 * y3;
            }


            //
            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            //tmpPtr1[i + j * w] /= ave1;
                            //tmpPtr2[i + j * w] /= ave2;
                            //tmpPtr3[i + j * w] /= ave3;
                            *tmpPtr1 /= ave1; tmpPtr1++;
                            *tmpPtr2 /= ave2; tmpPtr2++;
                            *tmpPtr3 /= ave3; tmpPtr3++;
                        }
                    }
                }
            }

            return tmp;
        }
    }
}
