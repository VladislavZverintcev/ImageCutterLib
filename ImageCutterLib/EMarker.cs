using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageCutterLib
{
    public class EMarker
    {
        #region Fields
        Rectangle rect;
        Point center;
        Point sourceLocation;
        int size;
        int sizeIfSelected;
        Canvas parentCanvas;
        SolidColorBrush brushM;
        bool isSelected = false;
        #endregion Fields

        #region Properties
        public Point Center
        {
            get { return center; }
            set
            {
                center = value;
                MoveRectToCenter();
            }
        }
        public Point SourceLocation
        {
            get { return sourceLocation; }
            set { sourceLocation = value; }
        }
        public int Size
        {
            get { return size; }
            set
            {
                if (value > 0)
                {
                    size = value;
                    rect.Height = size;
                    rect.Width = size;
                    MoveRectToCenter();
                }
                else { throw new ArgumentException(); }
            }
        }
        public int SizeIfSelected
        {
            get { return sizeIfSelected; }
            set
            {
                if (value > 0)
                {
                    sizeIfSelected = value;
                }
                else { throw new ArgumentException(); }
            }
        }
        public SolidColorBrush BrushM
        {
            get { return brushM; }
            set
            {
                brushM = value;
                rect.Fill = brushM;
            }
        }
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    SelectChanged?.Invoke();
                }
            }
        }
        #endregion Properties

        #region Events
        public event Action SelectChanged;
        #endregion Events

        #region Constructors
        public EMarker(Canvas _parentCanvas, Point _center, Point _sourceLocation, int _size, int _sizeIfSelected, SolidColorBrush _brush)
        {
            if (_parentCanvas != null && _brush != null && _size > 0 && _sizeIfSelected > 0)
            {
                parentCanvas = _parentCanvas;
                center = _center;
                sourceLocation = _sourceLocation;
                size = _size;
                sizeIfSelected = _sizeIfSelected;
                brushM = _brush;
                rect = new Rectangle
                {
                    Height = size,
                    Width = size,
                    Fill = brushM,
                };
                parentCanvas.Children.Add(rect);
                MoveRectToCenter();
                SelectChanged += OnSelectedChanged;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        #endregion Constructors

        #region Methods
        #region Public
        public virtual void DelFromCanvas()
        {
            if (rect != null)
            {
                parentCanvas?.Children.Remove(rect);
            }
        }
        #endregion Public
        #region Privates
        void MoveRectToCenter()
        {
            Canvas.SetLeft(rect, center.X - rect.Width / 2);
            Canvas.SetTop(rect, center.Y - rect.Height / 2);
        }
        void OnSelectedChanged()
        {
            if (isSelected)
            {
                rect.Height = sizeIfSelected;
                rect.Width = sizeIfSelected;
                MoveRectToCenter();
            }
            else
            {
                rect.Height = size;
                rect.Width = size;
                MoveRectToCenter();
            }
        }
        #endregion Privates	

        #endregion Methods
    }
}
