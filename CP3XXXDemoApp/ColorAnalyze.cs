using Codeplex.Data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP3XXXDemoApp
{
    public class ColorAnalyze
    {

    }

    public class CIELAB
    {
        public CIELAB(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        public CIELCH ToLCH()
        {
            if (A == 0 & B == 0 & L == 0)
                return new CIELCH(0, 0, 0);

            var a = A;
            var b = B;

            var h = (Math.Atan2(b, a)) * (180.0 / Math.PI);

            if (h < 0)
            {
                h += 360.0;
            }
            else if (h >= 360)
            {
                h -= 360.0;
            }

            var c = Math.Sqrt(a * a + b * b);

            return new CIELCH(L, c, h);
        }
    }

    public class CIELCH
    {
        public CIELCH(double l, double c, double h)
        {
            L = l;
            C = c;
            H = h;
        }
        public double L { get; set; }
        public double C { get; set; }
        public double H { get; set; }

    }

    public class CIEXYZ
    {
        public CIEXYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public CIELAB ToLab(CIEXYZ whitePoint)
        {
            if (X + Y + Z == 0)
                return new CIELAB(0, 0, 0);

            Func<double, double, double> f = ((v1, v2) =>
            {
                var r = v2 == 0 ? 0 : v1 / v2;
                if (r > 0.008856)
                {
                    r = Math.Pow(r, 1.0 / 3.0);
                }
                else
                {
                    r = 7.787 * r + 16.0 / 116.0;
                }
                return r;
            });


            var fx = f(X, whitePoint.X);
            var fy = f(Y, whitePoint.Y);
            var fz = f(Z, whitePoint.Z);

            var l = fy * 116.0 - 16.0;
            var a = 500.0 * (fx - fy);
            var b = 200.0 * (fy - fz);

            return new CIELAB(l, a, b);
        }

    }

    public class Whitepoint : CIEXYZ
    {
        public Whitepoint(double x, double y, double z)
            :base(x, y, z)
        {

        }
    }

    
    public class ROI
    {
        public ROI(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return $"X={X},Y={Y},Width={Width},Height={Height}";
        }
    }

    public class CapturedPixels
    {
        public CapturedPixels(int width, int height, ushort[] pixels, string serialnumber = "NOSERIALNUMBER")
        {
            Width = width;
            Height = height;
            Pixels = pixels;
            Serialnumber = serialnumber;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public ushort[] Pixels { get; set; }
        public string Serialnumber { get; set; }

        public XYZPixels ToXYZPixels()
        {
            if (Pixels.Length != Width * Height * 3)
                throw new Exception("error");

            var l = new List<CIEXYZ>();
            var len = Width * Height;
            for(int i=0;i<len;i++)
            {
                var x = (double)Pixels[i];
                var y = (double)Pixels[i + len];
                var z = (double)Pixels[i + len * 2];

                l.Add(new CIEXYZ(x, y, z));
            }

            return new XYZPixels(Width, Height, l.ToArray());
        }

        public ushort[] ToDisplayPixels()
        {
            var mat = new double[][] { new double[] { 2.3655, -0.8971, -0.4683 }, new double[] { -0.5151, 1.4264, 0.0887 }, new double[] { 0.0052, -0.0144, 1.0089 } };
            //var mat = new double[][] { new double[] { 3.2410, -1.5374, -0.4986 }, new double[] { -0.9692, 1.8760, 0.0416 }, new double[] { 0.0556, -0.2040, 1.0507 } };
            var p1 = Pixels;
            var p2 = new ushort[p1.Length];
            var w = Width;
            var h = Height;
            var framesize = w * h;
            for(var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    var x = p1[j + i * w];
                    var y = p1[j + i * w + framesize];
                    var z = p1[j + i * w + framesize * 2];
                    var r = x * mat[0][0] + y * mat[0][1] + z * mat[0][2];
                    var g = x * mat[1][0] + y * mat[1][1] + z * mat[1][2];
                    var b = x * mat[2][0] + y * mat[2][1] + z * mat[2][2];
                    if (r > 255)
                        r = 255;
                    if (g > 255)
                        g = 255;
                    if (b > 255)
                        b = 255;
                    if (r < 0)
                        r = 0;
                    if (g < 0)
                        g = 0;
                    if (b < 0)
                        b = 0;
                    p2[j + i * w] = (ushort)r;
                    p2[j + i * w + framesize] = (ushort)g;
                    p2[j + i * w + framesize * 2] = (ushort)b;
                }
            }
            return p2;
        }
    }
    public class ShadingPixels
    {
        public ShadingPixels(int width, int height, float[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
        public int Width { get; }
        public int Height { get; }
        public float[] Pixels { get; }

    }
    public class XYZPixels
    {
        public XYZPixels(int witdh, int height, CIEXYZ[] pixels)
        {
            Width = witdh;
            Height = height;
            Pixels = pixels;
        }

        public XYZPixels Subpixels(ROI roi)
        {

            var w = roi.Width;
            var h = roi.Height;
            var l = new List<CIEXYZ>();
            for (int y = roi.Y; y < h + roi.Y; y++)
            {
                for (int x = roi.X; x < w + roi.X; x++)
                {
                    l.Add(Pixels[x + y * Width]);
                }
            }
            return new XYZPixels(w, h, l.ToArray());
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public CIEXYZ[] Pixels { get; set; }

    }

    public class LABPixels
    {
        public LABPixels(int width, int height, CIELAB[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public CIELAB[] Pixels { get; set; }

        public static LABPixels From(XYZPixels xyzPixels, Whitepoint white)
        {
            var w = xyzPixels.Width;
            var h = xyzPixels.Height;
            var p = xyzPixels.Pixels.Select(x => x.ToLab(white)).ToArray();

            return new LABPixels(w, h, p);
        }

        public CIELAB Average()
        {
            return new CIELAB(
                Pixels.Select(x => x.L).Average(),
                Pixels.Select(x => x.A).Average(),
                Pixels.Select(x => x.B).Average());
        }
    }
    
    public class ColorMatching
    {
        public class Vec3
        {
            public Vec3(double v0, double v1, double v2)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
            }

            public double V0 { get; set; }
            public double V1 { get; set; }
            public double V2 { get; set; }

            public double Scalar
            {
                get
                {
                    return Math.Sqrt(V0 * V0 + V1 * V1 + V2 * V2);
                }
            }

            public override string ToString()
            {
                return $"VEC ({V0:F2}, {V1:F2}, {V2:F2}) , Scalar {Scalar:F2}";
            }
        }
        public class Result
        {
            public Result(Vec3 deVec, (CIELAB lab, CIELCH lch) refColoevalue, (CIELAB lab, CIELCH lch) sampleColoevalue)
            {
                DEVec = deVec;
                RefColorvalue = refColoevalue;
                SampleColorvalue = sampleColoevalue;
            }

            public Vec3 DEVec { get; set; }
            public (CIELAB lab, CIELCH lch) RefColorvalue { get; }
            public (CIELAB lab, CIELCH lch) SampleColorvalue { get; }
        }
        public ColorMatching()
        {

        }

        public Result Execute(XYZPixels reference, XYZPixels sample, ROI roi, Whitepoint white)
        {
            var r = reference.Subpixels(roi);
            var s = sample.Subpixels(roi);
            var rois = new ROI[3,3];
            var roiw3 = roi.Width / 3;
            var roih3 = roi.Height / 3;
            for (int i=0;i<3;i++)
            {
                for(int j=0;j<3;j++)
                {
                    rois[i, j] = new ROI(i * roiw3, j * roih3, roiw3, roih3);
                }
            }
            var de = new List<Vec3>();
            var rlab1 = new List<CIELAB>();
            var slab1 = new List<CIELAB>();
            var r2 = LABPixels.From(r.Subpixels(rois[0, 0]), white).Average();
            var s2 = LABPixels.From(s.Subpixels(rois[0, 0]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[0, 1]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[0, 1]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[0, 2]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[0, 2]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[1, 0]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[1, 0]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[1, 1]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[1, 1]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[1, 2]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[1, 2]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            r2 = LABPixels.From(r.Subpixels(rois[2, 0]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[2, 0]), white).Average();
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            r2 = LABPixels.From(r.Subpixels(rois[2, 1]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[2, 1]), white).Average();
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            r2 = LABPixels.From(r.Subpixels(rois[2, 2]), white).Average();
            s2 = LABPixels.From(s.Subpixels(rois[2, 2]), white).Average();
            de.Add(new Vec3(s2.L - r2.L, s2.A - r2.A, s2.B - r2.B));
            rlab1.Add(new CIELAB(r2.L, r2.A, r2.B));
            slab1.Add(new CIELAB(s2.L, s2.A, s2.B));

            var l1 = 0.0;
            var a1 = 0.0;
            var b1 = 0.0;
            var l2 = 0.0;
            var a2 = 0.0;
            var b2 = 0.0;
            var v0 = 0.0;
            var v1 = 0.0;
            var v2 = 0.0;
            for (var i=0;i<9;i++)
            {
                v0 += de[i].V0;
                v1 += de[i].V1;
                v2 += de[i].V2;
                l1 += rlab1[i].L;
                a1 += rlab1[i].A;
                b1 += rlab1[i].B;
                l2 += slab1[i].L;
                a2 += slab1[i].A;
                b2 += slab1[i].B;
            }
            v0 /= 9;
            v1 /= 9;
            v2 /= 9;
            l1 /= 9;
            a1 /= 9;
            b1 /= 9;
            l2 /= 9;
            a2 /= 9;
            b2 /= 9;

            var lab2 = new CIELAB(l1, a1, b1);
            var lab3 = new CIELAB(l2, a2, b2);

            return new Result(new Vec3(v0, v1, v2), (lab2, lab2.ToLCH()), (lab3, lab3.ToLCH()));
        }
    }
    
    public class ColorShading
    {
        public class Result
        {
            public Result(int width, int height, double[] pixels, CIELAB lab, CIELCH lch)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
                LAB = lab;
                LCH = lch;
            }
            public int Width { get; set; }
            public int Height { get; set; }
            public double[] Pixels { get; set; }
            public CIELAB LAB { get; }
            public CIELCH LCH { get; }
            public double Max
            {
                get
                {
                    return Pixels.Max();
                }
            }

            public double Min
            {
                get
                {
                    return Pixels.Min();
                }
            }
            public double Average
            {
                get
                {
                    return Pixels.Average();
                }
            }
        }

        public ColorShading()
        {

        }

        public Result Execute(XYZPixels sample, ROI roi, Whitepoint white)
        {
            
            var values = LABPixels.From(sample.Subpixels(roi), white);
            var w = values.Width;
            var h = values.Height;
            var lave = values.Pixels.Select(x => x.L).Average();
            var aave = values.Pixels.Select(x => x.A).Average();
            var bave = values.Pixels.Select(x => x.B).Average();

            var res = values.Pixels.Select(x => Math.Sqrt((x.L - lave) * (x.L - lave) + (x.A - aave) * (x.A - aave) + (x.B - bave) * (x.B - bave))).ToArray();

            var lab = new CIELAB(lave, aave, bave);
            var lch = lab.ToLCH();

            return new Result(w, h, res, lab, lch);
        }
    }

    public class ColorDistribution
    {
        public class Result
        {
            public Result(CIELAB lab, CIELCH lch, int[] lhist, int[] ahist, int[] bhist, int[,] abhist, (int Min, int Max) aRange, (int Min, int Max) bRange)
            {
                LAB = lab;
                LCH = lch;
                LHistgram = lhist;
                AHistgram = ahist;
                BHistgram = bhist;
                ABHistgram = abhist;
                ARange = aRange;
                BRange = bRange;
            }

            public CIELAB LAB { get; }
            public CIELCH LCH { get; }
            public int[] LHistgram { get; }
            public int[] AHistgram { get; }
            public int[] BHistgram { get; }
            public int[,] ABHistgram { get; }
            public (int Min, int Max) ARange { get; }
            public (int Min, int Max) BRange { get; }

        }
        public ColorDistribution()
        {

        }

        public Result Execute(XYZPixels sample, ROI roi, Whitepoint white)
        {
            var values = sample.Subpixels(roi);
            var res = LABPixels.From(values, white);
            var lave = res.Pixels.Select(x => x.L).Average();
            var aave = res.Pixels.Select(x => x.A).Average();
            var bave = res.Pixels.Select(x => x.B).Average();
            var amin = res.Pixels.Select(x => x.A).Min();
            var bmin = res.Pixels.Select(x => x.B).Min();
            var amax = res.Pixels.Select(x => x.A).Max();
            var bmax = res.Pixels.Select(x => x.B).Max();
            var lab = new CIELAB(lave, aave, bave);
            var lch = lab.ToLCH();
            var p = res.Pixels;
            var lhist = new int[101];
            var ahist = new int[201];
            var bhist = new int[201];
            var abhist = new int[201, 201];
            var abrangemin = -100;
            var abrangemax = 100;
            foreach (var pp in p)
            {
                var l = (int)pp.L;
                l = l > 100 ? 100 : l;
                lhist[l]++;
                var a = (int)pp.A;
                a = a < abrangemin ? abrangemin : a > abrangemax ? abrangemax : a;
                ahist[a + abrangemax]++;
                var b = (int)pp.B;
                b = b < abrangemin ? abrangemin : b > abrangemax ? abrangemax : b;
                bhist[b + abrangemax]++;
                abhist[a + abrangemax, b + abrangemax]++;
            }

            return new Result(lab, lch, lhist, ahist, bhist, abhist,((int)amin, (int)amax), ((int)bmin, (int)bmax));
        }
    }

    public class StatisticalDistribution
    {
        public class Result
        {
            public Result(int[] hist, double skewness, double kurtosis, double min, double max, double average)
            {
                Hist = hist;
                Skewness = skewness;
                Kurtosis = kurtosis;
                Min = min;
                Max = max;
                Average = average;
            }

            public int[] Hist { get; }
            public double Skewness { get; }
            public double Kurtosis { get; }
            public double Min { get; }
            public double Max { get; }
            public double Average { get; }

        }
        public StatisticalDistribution()
        {

        }

        public Result Execute(double[] value)
        {
            var h = new int[101];
            var n = value.Length;
            for (var i = 0; i < n; i++)
            {
                h[value[i] > 100.0 ? 100 : value[i] < 0.0 ? 0 : (int)value[i]]++;
            }
            var sk = CalcSkewness(value);
            var ku = CalcKurtosis(value);

            return new Result(h, sk, ku, value.Min(), value.Max(), value.Average());
        }

        double CalcSkewness(double[] value)
        {
            var n = value.Length;

            var ave = value.Average();
            var a = 0.0;
            var b = 0.0;
            for (var i = 0; i < n; i++)
            {
                a += (value[i] - ave) * (value[i] - ave) * (value[i] - ave);
                b += (value[i] - ave) * (value[i] - ave);
            }
            a /= n;
            b /= n;
            b = Math.Sqrt(b);
            b = b * b * b;
            var sk = a / b;
            

            return sk;
        }

        double CalcKurtosis(double[] value)
        {
            var n = value.Length;

            var ave = value.Average();
            var a = 0.0;
            var b = 0.0;
            for (var i = 0; i < n; i++)
            {
                a += (value[i] - ave) * (value[i] - ave) * (value[i] - ave) * (value[i] - ave);
                b += (value[i] - ave) * (value[i] - ave);
            }
            a /= n;
            b /= n;
            b = Math.Sqrt(b);
            b = b * b * b * b;
            var ku = a / b;

            return ku;
        }

    }
    public class Skewness
    {
        public class Result
        {
            public Result(double skewness, int[] lhist)
            {
                Skewness = skewness;
                Lhist = lhist;
            }
            public double Skewness { get; }
            public int[] Lhist { get; }
        }

        public Skewness()
        {

        }

        public Result Execute(XYZPixels sample, ROI roi, Whitepoint white)
        {
            var lab = LABPixels.From(sample.Subpixels(roi), white);
            var l = lab.Pixels.Select(x => x.L).ToArray();
            var st = new StatisticalDistribution();
            var r = st.Execute(l);

            return new Result(r.Skewness, r.Hist);

        }
    }
    public class PixelsReader
    {
        public PixelsReader()
        {

        }

        public CapturedPixels Execute(string filename, double[] cmat)
        {
            //var len = 1024 * 768 * 3;
            //var p = new ushort[len];
            //using (var br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            //{
            //    for(var i=0;i<len;i++)
            //        p[i] = br.ReadUInt16();
            //}
            var pix = Cv2.ImRead(filename, ImreadModes.Color);
            var len = pix.Width * pix.Height;
            var p = new ushort[len * 3];
            
            for (var y = 0; y < pix.Height; y++)
            {
                for (var x = 0; x < pix.Width; x++)
                {
                    var vec = pix.At<Vec3b>(y, x);
                    var vec2 = new Vec3d(0, 0, 0);
                    vec2[0] = vec[2] * cmat[0] + vec[1] * cmat[1] + vec[0] * cmat[2];
                    vec2[1] = vec[2] * cmat[3] + vec[1] * cmat[4] + vec[0] * cmat[5];
                    vec2[2] = vec[2] * cmat[6] + vec[1] * cmat[7] + vec[0] * cmat[8];
                    p[x + y * pix.Width] = (ushort)vec2[0];
                    p[x + y * pix.Width + len] = (ushort)vec2[1];
                    p[x + y * pix.Width + len * 2] = (ushort)vec2[2];
                }
            }
            return new CapturedPixels(pix.Width, pix.Height, p, Path.GetFileName(filename));
        }
    }

    public class ShadingPixelsLoader
    {
        public ShadingPixelsLoader()
        {

        }

        public ShadingPixels Execute(string filename = "shd.raw")
        {
            var len = 1024 * 768 * 3;
            var p = new float[len].Select(x => (float)1.0).ToArray();
            try
            {
                using (var br = new BinaryReader(new FileStream(filename, FileMode.Open)))
                {
                    for (var i = 0; i < len; i++)
                        p[i] = br.ReadSingle();
                }
            }
            catch
            {

            }
            return new ShadingPixels(1024, 768, p);
        }
    }
    public class PixelsWriter
    {
        public PixelsWriter()
        {

        }

        public void Execute(string filename, CapturedPixels pixels)
        {
            using (var bw = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                var p = pixels.Pixels;
                foreach (var v in p)
                {
                    bw.Write(v);
                }
            }
        }
    }

    public class ShadingPixelsWriter
    {
        public ShadingPixelsWriter()
        {

        }

        public void Execute(ShadingPixels pixels, string filename = "shd.raw")
        {
            using (var bw = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                var p = pixels.Pixels;
                foreach (var v in p)
                {
                    bw.Write(v);
                }
            }
        }
    }

    public class PixelsList
    {
        public PixelsList()
        {

        }

        List<CapturedPixels> _list = new List<CapturedPixels>();

        public void Add(CapturedPixels pixels)
        {
            _list.Add(pixels);
        }

        public void Remove(CapturedPixels pixels)
        {
            _list.Remove(pixels);
        }

        public void RemoveAll()
        {
            
        }
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public CapturedPixels this[int index]
        {
            get
            {
                return _list[index];
            }
        }
    }

    public class PixelsListCreator
    {
        public PixelsList Execute()
        {
            var pl = new PixelsList();
            var pr = new PixelsReader();

            //pl.Add(pr.Execute(@"D:\TESTIMG\L0.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L1.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L2.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L3.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L4.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L5.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L6.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L7.raw"));
            //pl.Add(pr.Execute(@"D:\TESTIMG\L8.raw"));

            if(Directory.Exists("PIX"))
            {
                var files = Directory.GetFiles("PIX", "*.jpg");
                foreach(var f in files)
                {
                    pl.Add(pr.Execute(f, AppStore.Instance.ColorCorrectionMatrix));
                }
            }

            return pl;
        }

    }

    public class AppStore
    {
        public AppStore()
        {
            
            Whitepoint = new Whitepoint(255, 255, 255);
        }

        
        public PixelsList PixelsList { get; set; }
        public Whitepoint Whitepoint { get; set; }
        public ShadingPixels ShadingPixels { get; set; }

        public double[] ColorCorrectionMatrix { get; set; }
        public static AppStore Instance { get; } = new AppStore();

        public void Load()
        {
            try
            {
                var s = File.ReadAllText("app.config.txt");
                var wp = s.Split(',');
                Whitepoint = new Whitepoint(int.Parse(wp[0]), int.Parse(wp[1]), int.Parse(wp[2]));
                ColorCorrectionMatrix = new double[] { double.Parse(wp[3]), double.Parse(wp[4]), double.Parse(wp[5]), double.Parse(wp[6]), double.Parse(wp[7]), double.Parse(wp[8]), double.Parse(wp[9]), double.Parse(wp[10]), double.Parse(wp[11]) };
                //ShadingPixels = new ShadingPixelsLoader().Execute();
                //dynamic json = DynamicJson.Parse(@"appsettings.json");
                //ColorCorrectionMatrix = json.ColorCorrectionMatrix;
                //Whitepoint = json.Whitepoint;

                PixelsList = new PixelsListCreator().Execute();
            }
            catch
            {

            }
        }

        public void Store()
        {
            var s = $"{Whitepoint.X:F0},{Whitepoint.Y:F0},{Whitepoint.Z:F0}," +
                $"{ColorCorrectionMatrix[0]:F4},{ColorCorrectionMatrix[1]:F4},{ColorCorrectionMatrix[2]:F4}," +
                $"{ColorCorrectionMatrix[3]:F4},{ColorCorrectionMatrix[4]:F4},{ColorCorrectionMatrix[5]:F4}," +
                $"{ColorCorrectionMatrix[6]:F4},{ColorCorrectionMatrix[7]:F4},{ColorCorrectionMatrix[8]:F4}";
            File.WriteAllText("app.config.txt",s);
        }
    }

    public class SamplePixels
    {
        public SamplePixels(string name, CapturedPixels capturedPixels)
        {
            Name = name;
            CapturedPixels = capturedPixels;
            DisplayPixels = new byte[capturedPixels.Width * capturedPixels.Height * 3];
        }

        public CapturedPixels CapturedPixels { get; }
        public string Name { get; }
        public byte[] DisplayPixels { get; }
    }
    
    public static class PseudoColorLUT
    {
        public static List<(int r, int g, int b)> RainbowLUT
        {
            get
            {
                var r = 0;
                var g = 0;
                var b = 0;
                var l = new List<(int r, int g, int b)>();
                for (int i = 0; i < 52; i++)
                {
                    r = 0;
                    g = (int)((Math.Cos(Math.PI * (i * 900 / 255.0 - 180) / 180.0) + 1) * 110.0);
                    b = 255;
                    l.Add((r, g, b));
                }
                for (int i = 52; i < 102; i++)
                {
                    r = 0;
                    g = 220;
                    b = (int)((Math.Cos(Math.PI * (i * 900 / 255.0 - 180) / 180.0) + 1) * 127.5);
                    l.Add((r, g, b));
                }
                for (int i = 102; i < 154; i++)
                {
                    r = (int)((Math.Cos(Math.PI * (i * 900 / 255.0 - 180) / 180.0) + 1) * 127.5);
                    g = 220;
                    b = 0;
                    l.Add((r, g, b));
                }
                for (int i = 154; i < 204; i++)
                {
                    r = 255;
                    g = (int)((Math.Cos(Math.PI * (i * 900 / 255.0 - 180) / 180.0) + 1) * 110.0);
                    b = 0;
                    l.Add((r, g, b));
                }
                for (int i = 204; i < 256; i++)
                {
                    r = 255;
                    g = 0;
                    b = (int)((Math.Cos(Math.PI * (i * 900 / 255.0 - 180) / 180.0) + 1) * 127.5);
                    l.Add((r, g, b));
                }

                return l;
            }
        }
    }
}
