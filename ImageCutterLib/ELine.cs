using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageCutterLib
{
    public class ELine
    {

        #region Fields
        Canvas parentCanvas;
        #endregion Fields

        #region Properties
        public Line line { get; set; }
        public Point SourceLocationPoint1 { get; set; }
        public Point SourceLocationPoint2 { get; set; }
        #endregion Properties

        #region Events

        #endregion Events

        #region Constructors
        public ELine(Canvas _parentCanvas, Line _line, Point _sourceLocationPoint1, Point _sourceLocationPoint2)
        {
            if (_line == null)
            {
                throw new ArgumentNullException("Line is a null");
            }
            if (_parentCanvas == null)
            {
                throw new ArgumentNullException("Parent canvas is a null");
            }
            line = _line;
            parentCanvas = _parentCanvas;
            SourceLocationPoint1 = _sourceLocationPoint1;
            SourceLocationPoint2 = _sourceLocationPoint2;
            parentCanvas.Children.Add(line);
        }
        #endregion Constructors

        #region Methods

        #region Privates

        #endregion Privates	

        #region Public
        public virtual void DelFromCanvas()
        {
            if (line != null)
            {
                parentCanvas?.Children.Remove(line);
            }
        }
        #endregion Public

        #endregion Methods

    }
}
