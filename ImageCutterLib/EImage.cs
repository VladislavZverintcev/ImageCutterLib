using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace ImageCutterLib
{
    public class EImage : System.Windows.Controls.Image
    {

        #region Fields
        Bitmap currentBitmap;
        Bitmap cuttedBitmap;
        #endregion Fields

        #region Properties
        public Bitmap CurrentBitmap
        {
            get { return currentBitmap; }
            set { currentBitmap = value; }
        }
        public Bitmap CuttedBitmap
        {
            get { return cuttedBitmap; }
        }
        #endregion Properties

        #region Events
        public event Action CutBitmapChanged;
        public event Action SourceBitmapChanged;
        #endregion Events

        #region Constructors

        #endregion Constructors

        #region Methods

        #region Privates
        public void SetBitmapSource(string pathToBitmap)
        {
            if (string.IsNullOrWhiteSpace(pathToBitmap))
            {
                throw new ArgumentException("pathToBitmap is null or empty");
            }
            if (!File.Exists(pathToBitmap))
            {
                throw new FileNotFoundException("Bitmap from path not found");
            }
            if (!ImageCutter.IsFileImage(pathToBitmap))
            {
                throw new ArgumentException("Bitmap is not valid");
            }
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pathToBitmap);
            bitmap.EndInit();
            Source = bitmap;
            CurrentBitmap = new Bitmap(pathToBitmap);
            DelCuttedBitmap();
            SourceBitmapChanged?.Invoke();
        }
        public void DelBitmapSource()
        {
            Source = null;
            currentBitmap = null;
            DelCuttedBitmap();
            SourceBitmapChanged?.Invoke();
        }
        #endregion Privates	

        #region Public
        public void CutBitmap(PointCollection contour)
        {
            cuttedBitmap = ImageCutter.GetCutBitmap(CurrentBitmap, contour);
            CutBitmapChanged?.Invoke();
        }
        public void SaveCuttedBitmap(string pathToSave)
        {
            if (cuttedBitmap == null)
            {
                throw new NullReferenceException("Cutted bitmap is a null, can't be saves");
            }
            ImageCutter.SaveBitmap(cuttedBitmap, pathToSave);
        }
        public void DelCuttedBitmap()
        {
            if (currentBitmap != null)
            {
                cuttedBitmap = null;
                CutBitmapChanged?.Invoke();
            }
        }
        #endregion Public

        #endregion Methods
    }
}
