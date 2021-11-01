using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageCutterLib
{
    public class ImageCutter
    {
        public static void GetCutBitmap(string pathToImg, string pathToExp, System.Windows.Media.PointCollection contour)
        {
            var bmp = new Bitmap(pathToImg);
            if (bmp == null) { throw new ArgumentException("No valid bitmap"); }
            if (contour == null || contour.Count < 3) { throw new ArgumentException("No valid contour"); }
            var contourConverted = PointsConverter(contour);
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics G = Graphics.FromImage(result))
            {
                G.Clear(System.Drawing.Color.Transparent);
                G.SmoothingMode = SmoothingMode.AntiAlias;
                G.CompositingQuality = CompositingQuality.HighQuality;
                G.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (System.Drawing.Brush Brush = new TextureBrush(bmp))
                {
                    using (GraphicsPath GP = new GraphicsPath())
                    {
                        GP.AddLines(contourConverted.ToArray());
                        G.FillPath(Brush, GP);
                        G.DrawImage(result, GetBoxFromContour(contourConverted),
                        GetBoxFromContour(contourConverted), GraphicsUnit.Pixel);
                    }
                }
            }
            SaveBitmap(result.Clone(GetBoxFromContour(contourConverted), result.PixelFormat), pathToExp);
        }
        public static void GetCutBitmap(Bitmap bitmapSource, string pathToExp, System.Windows.Media.PointCollection contour)
        {
            if (bitmapSource == null) { throw new ArgumentException("No valid bitmap"); }
            if (contour == null || contour.Count < 3) { throw new ArgumentException("No valid contour"); }
            var contourConverted = PointsConverter(contour);
            Bitmap result = new Bitmap(bitmapSource.Width, bitmapSource.Height);
            using (Graphics G = Graphics.FromImage(result))
            {
                G.Clear(System.Drawing.Color.Transparent);
                G.SmoothingMode = SmoothingMode.AntiAlias;
                G.CompositingQuality = CompositingQuality.HighQuality;
                G.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (System.Drawing.Brush Brush = new TextureBrush(bitmapSource))
                {
                    using (GraphicsPath GP = new GraphicsPath())
                    {
                        GP.AddLines(contourConverted.ToArray());
                        G.FillPath(Brush, GP);
                        G.DrawImage(result, GetBoxFromContour(contourConverted),
                        GetBoxFromContour(contourConverted), GraphicsUnit.Pixel);
                    }
                }
            }
            SaveBitmap(result.Clone(GetBoxFromContour(contourConverted), result.PixelFormat), pathToExp);
        }
        public static Bitmap GetCutBitmap(Bitmap bitmapSource, System.Windows.Media.PointCollection contour)
        {
            if (bitmapSource == null) { throw new ArgumentException("No valid bitmap"); }
            if (contour == null || contour.Count < 3) { throw new ArgumentException("No valid contour"); }
            var contourConverted = PointsConverter(contour);
            Bitmap result = new Bitmap(bitmapSource.Width, bitmapSource.Height);
            using (Graphics G = Graphics.FromImage(result))
            {
                G.Clear(System.Drawing.Color.Transparent);
                G.SmoothingMode = SmoothingMode.AntiAlias;
                G.CompositingQuality = CompositingQuality.HighQuality;
                G.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (System.Drawing.Brush Brush = new TextureBrush(bitmapSource))
                {
                    using (GraphicsPath GP = new GraphicsPath())
                    {
                        GP.AddLines(contourConverted.ToArray());
                        G.FillPath(Brush, GP);
                        G.DrawImage(result, GetBoxFromContour(contourConverted),
                        GetBoxFromContour(contourConverted), GraphicsUnit.Pixel);
                    }
                }
            }
            return result.Clone(GetBoxFromContour(contourConverted), result.PixelFormat);
        }
        static Rectangle GetBoxFromContour(List<Point> contour)
        {
            if (contour == null || contour.Count < 3)
            {
                throw new ArgumentException("No valid contour");
            }
            int minX = contour.Min(a => a.X);
            int maxX = contour.Max(a => a.X);
            int minY = contour.Min(a => a.Y);
            int maxY = contour.Max(a => a.Y);
            Point topLeft = new Point { X = minX, Y = minY };
            Point bottomRight = new Point { X = maxX, Y = maxY };

            var rect = new Rectangle
            {
                Location = topLeft,
                Width = maxX - minX,
                Height = maxY - minY
            };

            return rect;

        }
        static List<Point> PointsConverter(System.Windows.Media.PointCollection points)
        {
            var result = new List<Point>();
            foreach (var point in points)
            {
                result.Add(new Point { X = (int)point.X, Y = (int)point.Y });
            }
            return result;
        }
        public static bool IsFileImage(string pathToImg)
        {
            if (!File.Exists(pathToImg))
            {
                throw new FileNotFoundException();
            }
            else
            {
                try
                {
                    Bitmap test = (Bitmap)Image.FromFile(pathToImg);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static int[] GetImageSizePixels(string pathToImg)
        {
            if (!File.Exists(pathToImg))
            {
                throw new FileNotFoundException("Image not found in a path");
            }
            if (!IsFileImage(pathToImg))
            {
                throw new ArgumentNullException("Image is not valid");
            }
            try
            {
                Bitmap bitmap = (Bitmap)Image.FromFile(pathToImg);
                int[] result = { bitmap.Width, bitmap.Height };
                return result;
            }
            catch
            {
                return null;
            }
        }
        public static int[] GetImageSizePixels(Bitmap bitmapSource)
        {
            try
            {
                int[] result = { bitmapSource.Width, bitmapSource.Height };
                return result;
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("Bitmap source is a null");
            }
            catch (ArgumentException)
            {

                throw new ArgumentException("Bitmap source is not valid");
            }
        }
        public static void SaveBitmap(Bitmap bitmapToSave, string pathToSave)
        {
            if (bitmapToSave == null)
            {
                throw new ArgumentNullException("Bitmap to save is a null");
            }
            if (string.IsNullOrWhiteSpace(pathToSave))
            {
                throw new ArgumentNullException("PathToSave is a null or Empty");
            }
            try
            {
                bitmapToSave.Save(pathToSave, ImageFormat.Png);
            }
            catch
            {
                throw new Exception("Error on save bitmap, may be path to save is not valid");
            }
        }
    }
}
