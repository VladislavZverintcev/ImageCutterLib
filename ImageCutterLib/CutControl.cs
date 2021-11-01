using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageCutterLib
{
    public class CutControl : UserControl
    {
        #region Fields
        Panel panel;
        Canvas workingCanvas;
        Grid grid;
        Grid workingGrid;
        Grid horizontalPanel;
        Grid toolsGrid;
        EImage img;
        Polygon cutArea;
        Label notify;
        string notifyText = "No image load";
        int notifyFontSize = 15;
        Border borderImage;
        Thickness borderImageThickness = new Thickness(1, 1, 1, 1);
        Button importBut;
        string importButText = "Import";
        Button exportBut;
        string exportButText = "Export";
        Button eraseLastStepBut;
        string eraseLastStepButText = "🢀";
        Button resetDrawingsBut;
        string resetDrawingsButText = "❌";
        int buttonHeight = 40;
        string saveFileDialogTitle = "Please, select a path to save the cut contour to a .png file:";
        string openFileDialogTitle = "Please, select a path to image for import:";
        string pngFormatAnnotation = "Png image format";
        string openFormatAnnotation = "Images formats";
        string unsupportedFormatText = "File format is not supported!";
        string titleOfMbox = "ImageCutter";
        string dragNDropText = "❏ Drag & Drop picture";
        List<ELine> lines;
        List<EMarker> markers;
        Thread ownerThread;
        SolidColorBrush brushLines = new SolidColorBrush(Color.FromArgb(180, 255, 0, 0));
        SolidColorBrush brushMarkers = new SolidColorBrush(Color.FromArgb(180, 0, 0, 255));
        SolidColorBrush brushArea = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));
        SolidColorBrush brushNotify = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        SolidColorBrush brushBorderImage = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        Style buttonStyle;
        int lineThickness = 2;
        int markerSize = 6;
        int markerSizeLarge = 12;
        int selectionMarkerArea = 10;
        double[] lineStrokeDash = new double[] { 3, 3 };
        double lineStrokeOffset = 0;
        int lineStrokeOffsetSpeed = 10;
        private System.Timers.Timer lineShiftTimer;
        string pathToImg;
        string pathToExp;
        int roundLevel = 0;

        double old_workingCanvasWidth;
        double old_workingCanvasHeight;
        double old_imageWidth;
        double old_imageHeight;

        #region ControlModeParam

        bool modeDragAndDrop = false;
        bool modeShowHorizontalPanel = true;
        bool modeShowImportButton = true;
        bool modeShowExportButton = true;
        bool modeLeftSideTools = false;

        #endregion ControlModeParam

        #endregion Fields

        #region Properties
        public SolidColorBrush BrushLines
        {
            get { return brushLines; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid BrushLines");
                }
                brushLines = value;
            }
        }
        public SolidColorBrush BrushMarkers
        {
            get { return brushMarkers; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid BrushMarkers");
                }
                brushMarkers = value;
            }
        }
        public SolidColorBrush BrushArea
        {
            get { return brushArea; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid BrushArea");
                }
                brushArea = value;
            }
        }
        public SolidColorBrush BrushNotify
        {
            get { return brushNotify; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid Brush for Label");
                }
                brushNotify = value;
                if (notify != null)
                {
                    notify.Foreground = brushNotify;
                }
            }
        }
        public SolidColorBrush BrushBorderImage
        {
            get { return brushBorderImage; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid Brush for Border");
                }
                brushBorderImage = value;
                if (borderImage != null)
                {
                    borderImage.BorderBrush = brushBorderImage;
                }
            }
        }
        public Thickness BorderImageThickness
        {
            get { return borderImageThickness; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("No valid Thickness for Border");
                }
                borderImageThickness = value;
                if (borderImage != null)
                {
                    borderImage.BorderThickness = borderImageThickness;
                }
            }
        }
        public Style ButtonStyle
        {
            get { return buttonStyle; }
            set
            {
                if (value != null)
                {
                    buttonStyle = value;
                    SetButtonStyle();
                }
            }
        }
        public int LineThickness
        {
            get { return lineThickness; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("No valid LineThickness");
                }
                lineThickness = value;
            }
        }
        public int LineSpeed
        {
            get { return lineStrokeOffsetSpeed; }
            set { lineStrokeOffsetSpeed = value; }
        }
        public int MarkerSize
        {
            get { return markerSize; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("No valid MarkerSize");
                }
                markerSize = value;
            }
        }
        public int MarkerSizeLarge
        {
            get { return markerSizeLarge; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("No valid MarkerSizeLarge");
                }
                markerSizeLarge = value;
            }
        }
        public int SelectionMarkerArea
        {
            get { return selectionMarkerArea; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("No valid SelectionMarkerArea");
                }
                selectionMarkerArea = value;
            }
        }
        public string ImportButText
        {
            get { return importButText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    importButText = value;
                    if (importBut != null)
                    {
                        importBut.Content = importButText;
                    }
                }
            }
        }
        public string ExportButText
        {
            get { return exportButText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    exportButText = value;
                    if (exportBut != null)
                    {
                        exportBut.Content = exportButText;
                    }
                }
            }
        }
        public string SaveFileDialogTitle
        {
            get { return saveFileDialogTitle; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    saveFileDialogTitle = value;
                }
            }
        }
        public string OpenFileDialogTitle
        {
            get { return openFileDialogTitle; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    openFileDialogTitle = value;
                }
            }
        }
        public string PngFormatAnnotation
        {
            get { return pngFormatAnnotation; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    pngFormatAnnotation = value;
                }
            }
        }
        public string OpenFormatAnnotation
        {
            get { return openFormatAnnotation; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    openFormatAnnotation = value;
                }
            }
        }
        public string PathToImg
        {
            get { return pathToImg; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("No valid PathToImg");
                }
                if (!File.Exists(value))
                {
                    throw new FileNotFoundException("No image File found  in the path to Import");
                }
                if (!ImageCutter.IsFileImage(value))
                {
                    throw new FileFormatException("Imported file is not an image");
                }
                pathToImg = value;
                ClearAllDrawings();
                SetImage(pathToImg);
            }
        }
        public string PathtoExp
        {
            get { return pathToExp; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Incorrect export path");
                }
                pathToExp = value;
            }
        }
        public string NotifyText
        {
            get { return notifyText; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Incorrect label text");
                }
                notifyText = value;
                if (notify != null && !ModeDragAndDrop)
                {
                    notify.Content = notifyText;
                }
            }
        }
        public int NotifyFontSize
        {
            get { return notifyFontSize; }
            set
            {
                if (value > 0)
                {
                    notifyFontSize = value;
                    if (notify != null)
                    {
                        notify.FontSize = notifyFontSize;
                    }
                }
            }
        }
        public string ResetDrawingsButText
        {
            get { return resetDrawingsButText; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Incorrect resetButton text");
                }
                resetDrawingsButText = value;
                if (resetDrawingsBut != null)
                {
                    resetDrawingsBut.Content = resetDrawingsButText;
                }
            }
        }
        public string UnsupportedFormatText
        {
            get { return unsupportedFormatText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    unsupportedFormatText = value;
                }
            }
        }
        public string TitleOfMbox
        {
            get { return titleOfMbox; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    titleOfMbox = value;
                }
            }
        }
        public string DragNDropText
        {
            get { return dragNDropText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (dragNDropText != value)
                    {
                        dragNDropText = value;
                        if (ModeDragAndDrop == true && notify != null)
                        {
                            notify.Content = dragNDropText;
                        }
                    }
                }

            }
        }

        #region controlModeParam
        public bool ModeDragAndDrop
        {
            get { return modeDragAndDrop; }
            set
            {
                if (modeDragAndDrop != value)
                {
                    modeDragAndDrop = value;
                    SetAllowDragNDrop(modeDragAndDrop);
                }
            }
        }
        public bool ModeShowHorizontalPanel
        {
            get { return modeShowHorizontalPanel; }
            set
            {
                if (modeShowHorizontalPanel != value)
                {
                    modeShowHorizontalPanel = value;
                    RecalculateModeParam();
                }
            }
        }
        public bool ModeShowImportButton
        {
            get { return modeShowImportButton; }
            set
            {
                if (modeShowImportButton != value)
                {
                    modeShowImportButton = value;
                    RecalculateModeParam();
                }
            }
        }
        public bool ModeShowExportButton
        {
            get { return modeShowExportButton; }
            set
            {
                if (modeShowExportButton != value)
                {
                    modeShowExportButton = value;
                    RecalculateModeParam();
                }
            }
        }
        public bool ModeLeftSideTools
        {
            get { return modeLeftSideTools; }
            set
            {
                if (modeLeftSideTools != value)
                {
                    modeLeftSideTools = value;
                    RebuildUI();
                }
            }
        }
        #endregion controlModeParam

        #endregion Properties

        #region Events
        public event Action ContourIsClosed;
        public event Action ContourIsOpened;
        #endregion Events

        #region Constructors
        public CutControl()
        {
            Init();
        }

        #endregion Constructors

        #region Methods

        #region Privates
        void Init()
        {
            BuildUI();

            old_imageWidth = img.ActualWidth;
            old_imageHeight = img.ActualHeight;
            old_workingCanvasWidth = workingCanvas.ActualWidth;
            old_workingCanvasHeight = workingCanvas.ActualHeight;

            lines = new List<ELine>();
            markers = new List<EMarker>();
            workingCanvas.MouseDown += CutControl_MouseDown;
            workingCanvas.MouseMove += CutControl_MouseMove;
            img.SizeChanged += Img_SizeChanged;
            workingCanvas.SizeChanged += WorkingCanvas_SizeChanged;
            ContourIsClosed += OnContourIsClosed;
            ContourIsOpened += OnContourIsOpened;
            img.CutBitmapChanged += OnCutBitmapChanged;
            img.SourceBitmapChanged += OnSourceBitmapChanged;
            workingCanvas.DragEnter += WorkingCanvas_DragEnter;
            workingCanvas.Drop += WorkingCanvas_Drop;
            ownerThread = Thread.CurrentThread;
            panel = (Panel)grid;
            Content = panel;
            SetLineShiftTimer();
            //AfterAll
            OnCutBitmapChanged();
        }
        void RecalculateModeParam([CallerMemberName] string propertyName = null)
        {
            if (propertyName == "ModeShowHorizontalPanel")
            {
                if (!modeShowHorizontalPanel)
                {
                    if (modeShowImportButton)
                    {
                        modeShowImportButton = false;
                    }
                    if (modeShowExportButton)
                    {
                        modeShowExportButton = false;
                    }
                }
            }
            if (propertyName == "ModeShowImportButton" || propertyName == "ModeShowExportButton")
            {
                if (!modeShowImportButton && !modeShowExportButton)
                {
                    if (modeShowHorizontalPanel)
                    {
                        modeShowHorizontalPanel = false;
                    }
                }
                else
                {
                    if (!modeShowHorizontalPanel)
                    {
                        modeShowHorizontalPanel = true;
                    }
                }
            }
            RebuildUI();
        }
        #region BuildUI
        void BuildUI()
        {
            //WorkingGrid
            img = BuildImage();
            borderImage = BuildBorder();
            borderImage.Child = img;
            notify = BuildLabel();
            workingCanvas = BuildWorkCanvas();
            toolsGrid = BuildToolsGrid();
            resetDrawingsBut = BuildResetDrawingsButton();
            eraseLastStepBut = BuildEraseLastStepButton();
            toolsGrid.Children.Add(eraseLastStepBut);
            toolsGrid.Children.Add(resetDrawingsBut);
            Grid.SetRow(eraseLastStepBut, 0);
            Grid.SetRow(resetDrawingsBut, 1);
            workingGrid = BuildWorkingGrid();
            workingGrid.Children.Add(notify);
            workingGrid.Children.Add(borderImage);
            workingGrid.Children.Add(toolsGrid);
            workingGrid.Children.Add(workingCanvas);
            Grid.SetColumn(notify, 0);
            Grid.SetColumn(borderImage, 0);
            Grid.SetColumn(workingCanvas, 0);
            Grid.SetColumn(toolsGrid, 1);
            //HorizontalPanel
            horizontalPanel = BuildHorizontalPanel();
            importBut = BuildImportButton();
            exportBut = BuildExportButton();
            horizontalPanel.Children.Add(importBut);
            horizontalPanel.Children.Add(exportBut);
            Grid.SetColumn(importBut, 0);
            Grid.SetColumn(exportBut, 1);
            //MainGrid
            grid = BuildGrid();
            grid.Children.Add(workingGrid);
            grid.Children.Add(horizontalPanel);
            Grid.SetRow(workingGrid, 0);
            Grid.SetRow(horizontalPanel, 1);
        }
        void RebuildUI([CallerMemberName] string propertyOrMethodName = null)
        {
            #region HorizontalPanel
            if (propertyOrMethodName == "RecalculateModeParam")
            {
                if (!ModeShowHorizontalPanel)
                {
                    if (grid != null)
                    {
                        if (grid.RowDefinitions.Count > 1)
                        {
                            if (horizontalPanel != null && grid.Children.Contains(horizontalPanel))
                            {
                                grid.Children.Remove(horizontalPanel);
                            }
                            grid.RowDefinitions.RemoveAt(1);
                        }
                    }
                }
                else
                {
                    if (grid.RowDefinitions.Count == 1)
                    {
                        grid.RowDefinitions.Add(new RowDefinition
                        {
                            Height = new GridLength(buttonHeight)
                        });
                        if (horizontalPanel != null && !grid.Children.Contains(horizontalPanel))
                        {
                            grid.Children.Add(horizontalPanel);
                            Grid.SetRow(horizontalPanel, 1);
                        }
                    }
                    if (!modeShowImportButton)
                    {
                        if (importBut != null)
                        {
                            if (horizontalPanel != null && horizontalPanel.Children.Contains(importBut))
                            {
                                horizontalPanel.Children.Remove(importBut);
                                if (horizontalPanel.ColumnDefinitions.Count > 1)
                                {
                                    horizontalPanel.ColumnDefinitions.RemoveAt(0);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (importBut != null && !horizontalPanel.Children.Contains(importBut))
                        {
                            horizontalPanel.Children.Add(importBut);
                            if (horizontalPanel.ColumnDefinitions.Count == 1)
                            {
                                horizontalPanel.ColumnDefinitions.Add(new ColumnDefinition());
                            }
                            Grid.SetColumn(importBut, 0);
                        }
                    }
                    if (!modeShowExportButton)
                    {
                        if (exportBut != null && horizontalPanel.Children.Contains(exportBut))
                        {
                            horizontalPanel.Children.Remove(exportBut);
                            if (horizontalPanel.ColumnDefinitions.Count > 1)
                            {
                                horizontalPanel.ColumnDefinitions.RemoveAt(1);
                            }
                        }
                    }
                    else
                    {
                        if (exportBut != null && !horizontalPanel.Children.Contains(exportBut))
                        {
                            horizontalPanel.Children.Add(exportBut);
                            if (horizontalPanel.ColumnDefinitions.Count == 1)
                            {
                                horizontalPanel.ColumnDefinitions.Add(new ColumnDefinition());
                            }
                            Grid.SetColumn(exportBut, 1);
                        }
                    }
                }
            }
            #endregion HorizontalPanel
            if (propertyOrMethodName == "ModeLeftSideTools")
            {
                if (modeLeftSideTools)
                {
                    if (workingGrid != null && toolsGrid != null && borderImage != null
                        && workingCanvas != null && notify != null
                        && workingGrid.ColumnDefinitions.Count == 2)
                    {
                        workingGrid.ColumnDefinitions[0] = new ColumnDefinition
                        {
                            Width = new GridLength(buttonHeight)
                        };
                        workingGrid.ColumnDefinitions[1] = new ColumnDefinition();
                        Grid.SetColumn(toolsGrid, 0);
                        Grid.SetColumn(borderImage, 1);
                        Grid.SetColumn(workingCanvas, 1);
                        Grid.SetColumn(notify, 1);
                    }
                }
                else
                {
                    if (workingGrid != null && toolsGrid != null && borderImage != null
                        && workingCanvas != null && notify != null
                        && workingGrid.ColumnDefinitions.Count == 2)
                    {
                        workingGrid.ColumnDefinitions[1] = new ColumnDefinition
                        {
                            Width = new GridLength(buttonHeight)
                        };
                        workingGrid.ColumnDefinitions[0] = new ColumnDefinition();
                        Grid.SetColumn(toolsGrid, 1);
                        Grid.SetColumn(borderImage, 0);
                        Grid.SetColumn(workingCanvas, 0);
                        Grid.SetColumn(notify, 0);
                    }
                }
            }
        }
        Canvas BuildWorkCanvas()
        {
            return new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            };

        }
        Grid BuildGrid()
        {
            var result = new Grid();
            result.RowDefinitions.Add(new RowDefinition());
            result.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(buttonHeight)
            });
            return result;
        }
        Grid BuildWorkingGrid()
        {
            var result = new Grid();
            result.ColumnDefinitions.Add(new ColumnDefinition());
            result.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(buttonHeight)
            });
            return result;
        }
        Grid BuildHorizontalPanel()
        {
            var result = new Grid();
            var columnDefenition0 = new ColumnDefinition();
            var columnDefenition1 = new ColumnDefinition();
            result.ColumnDefinitions.Add(columnDefenition0);
            result.ColumnDefinitions.Add(columnDefenition1);
            return result;
        }
        Grid BuildToolsGrid()
        {
            var result = new Grid();
            result.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(buttonHeight)
            });
            result.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(buttonHeight)
            });
            result.IsEnabled = false;
            return result;
        }
        Button BuildImportButton()
        {
            var result = new Button
            {
                Content = importButText,
            };
            result.Click += OnImportButClick;
            return result;
        }
        Button BuildExportButton()
        {
            var result = new Button
            {
                Content = exportButText
            };
            result.Click += OnExportButClick;
            return result;
        }
        Button BuildResetDrawingsButton()
        {
            var result = new Button
            {
                Content = resetDrawingsButText
            };
            result.Click += OnResetDrawingsButtonClick;
            return result;
        }
        Button BuildEraseLastStepButton()
        {
            var result = new Button
            {
                Content = eraseLastStepButText
            };
            result.Click += OnEraseLastStepButtonClick;
            return result;
        }
        EImage BuildImage()
        {
            var result = new EImage
            {
                Stretch = Stretch.Uniform,

            };
            return result;
        }
        Label BuildLabel()
        {
            var result = new Label
            {
                Foreground = brushNotify,
                Content = notifyText,
                FontSize = notifyFontSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            return result;
        }
        Border BuildBorder()
        {
            var result = new Border
            {
                BorderBrush = brushBorderImage,
                BorderThickness = borderImageThickness,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Hidden
            };
            return result;
        }
        void SetButtonStyle()
        {
            if (importBut != null) { importBut.Style = buttonStyle; }
            if (exportBut != null) { exportBut.Style = buttonStyle; }
            if (eraseLastStepBut != null) { eraseLastStepBut.Style = buttonStyle; }
            if (resetDrawingsBut != null) { resetDrawingsBut.Style = buttonStyle; }
        }

        #endregion BuildUI
        #region OnEvents
        private void CutControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsCursorOnImage(PointToScreen(e.GetPosition(this))))
            {
                return;
            }
            if (!IsContourClosed())
            {
                if (markers.Count == 0)
                {
                    AddNewStep(e.GetPosition(workingCanvas));
                }
                else
                {
                    EMarker m = GetSelectedMarker();
                    if (m != null)
                    {
                        AddNewStep(GetLastMarker().Center, m.Center);
                    }
                    else
                    {
                        AddNewStep(GetLastMarker().Center, e.GetPosition(workingCanvas));
                    }
                }
            }
            //MessageBox.Show($"lines: {lines.Count}{Environment.NewLine}" +
            //    $"markers: {markers.Count}{Environment.NewLine}" +
            //    $"pointsRoad: {GetPointRoad().Count()}{Environment.NewLine}" +
            //    $"IsClosed: {IsContourClosed()}");
            if (IsContourClosed())
            {
                ContourIsClosed?.Invoke();
            }
            else { ContourIsOpened?.Invoke(); }
            CheckForEnableTools();
        }
        private void CutControl_MouseMove(object sender, MouseEventArgs e)
        {
            MarkerSelection(e.GetPosition(workingCanvas));
        }
        private void WorkingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawingElementsUpdateSize();
        }
        private void Img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawingElementsUpdateSize();
            if (img != null && img.Source != null && borderImage != null)
            {
                borderImage.Clip = borderImage.Child.Clip;
            }
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.FromThread(ownerThread)?.Invoke(() =>
                {
                    if (lines?.Count != 0)
                    {
                        if (lineStrokeOffset + 1 == double.MaxValue)
                        {
                            lineStrokeOffset = 0;
                        }
                        else
                        {
                            lineStrokeOffset++;
                        }
                        foreach (var line in lines)
                        {
                            line.line.StrokeDashOffset = lineStrokeOffset;
                        }
                    }
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            { return; }
        }
        private void OnContourIsOpened()
        {
            if (img != null && img.CuttedBitmap != null)
            {
                img.DelCuttedBitmap();
            }
            DeleteCutArea();
        }
        private void OnContourIsClosed()
        {
            BuildCutArea();
            GetCutImg();
        }
        void OnCutBitmapChanged()
        {
            if (img?.CuttedBitmap != null)
            {
                if (exportBut != null)
                {
                    exportBut.IsEnabled = true;
                }
            }
            else
            {
                if (exportBut != null)
                {
                    exportBut.IsEnabled = false;
                }
            }
        }
        private void OnExportButClick(object sender, RoutedEventArgs e)
        {
            SaveCutImg();
        }
        private void OnImportButClick(object sender, RoutedEventArgs e)
        {
            ImportImage();
        }
        private void OnResetDrawingsButtonClick(object sender, RoutedEventArgs e)
        {
            ClearAllDrawings();
            CheckForEnableTools();
        }
        private void OnEraseLastStepButtonClick(object sender, RoutedEventArgs e)
        {
            DeleteLastStep();
            CheckForEnableTools();
        }
        private void OnSourceBitmapChanged()
        {
            if (img == null || img.Source == null)
            {
                notify.Visibility = Visibility.Visible;
                borderImage.Visibility = Visibility.Hidden;
                return;
            }
            if (img != null && img.Source != null)
            {
                notify.Visibility = Visibility.Hidden;
                borderImage.Visibility = Visibility.Visible;
            }
        }
        private void WorkingCanvas_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                if (!ImageCutter.IsFileImage(files[0]))
                {
                    System.Windows.MessageBox.Show(unsupportedFormatText, titleOfMbox, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    PathToImg = files[0];
                }
            }
        }
        private void WorkingCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
        #endregion OnEvents

        #region DrawingInstruments
        void AddNewStep(Point startPoint, Point? secondPoint = null)
        {
            DrawingMarker(startPoint);
            if (secondPoint != null)
            {
                DrawingMarker((Point)secondPoint);
                DrawingLine(startPoint, (Point)secondPoint);
            }
        }
        void DeleteLastStep()
        {
            if (markers == null || markers.Count == 0) { return; }
            if (markers.Count == 1) { markers[0].DelFromCanvas(); markers.Clear(); }
            if (lines.Count > 0)
            {
                var lastPoint = new Point(lines.Last().line.X2, lines.Last().line.Y2);
                var markerFromLastLine = GetMarkerFromPoint(lastPoint);
                if (markerFromLastLine != null)
                {
                    if (!IsMoreOneLineMarker(markerFromLastLine))
                    {
                        markerFromLastLine.DelFromCanvas();
                        markers.Remove(markerFromLastLine);
                    }
                    lines.Last().DelFromCanvas();
                    if (lines.Remove(lines.Last()))
                    {
                        ContourIsOpened?.Invoke();
                    }
                }

            }
        }
        void DrawingLine(Point startPoint, Point finishPoint)
        {
            if (startPoint.X == finishPoint.X && startPoint.Y == finishPoint.Y)
            { return; }
            var nLine = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = finishPoint.X,
                Y2 = finishPoint.Y,
                StrokeThickness = lineThickness,
                Stroke = brushLines,
                StrokeDashArray = new DoubleCollection(lineStrokeDash),
                StrokeDashOffset = lineStrokeOffset
            };
            if (!IsLineAlreadyExist(nLine))
            {
                if (workingCanvas != null)
                {
                    var eLine = new ELine(workingCanvas, nLine,
                    ConvertToSourceCoordinatesImagePoint(ConvertCanvasPointToImagePoint(new Point(nLine.X1, nLine.Y1))),
                    ConvertToSourceCoordinatesImagePoint(ConvertCanvasPointToImagePoint(new Point(nLine.X2, nLine.Y2))));
                    lines?.Add(eLine);
                }
            }
        }
        void DrawingMarker(Point center)
        {
            if (!IsMarkerAlreadyExist(center))
            {
                var marker = new EMarker(workingCanvas, center,
                    ConvertToSourceCoordinatesImagePoint(ConvertCanvasPointToImagePoint(center)),
                    markerSize, markerSizeLarge, brushMarkers);
                markers?.Add(marker);
            }
        }
        void DrawingPoligon(PointCollection points)
        {
            if (points != null && points.Count > 2)
            {
                cutArea = new Polygon
                {
                    Points = points,
                    Fill = brushArea
                };
                workingCanvas.Children.Add(cutArea);
            }
        }
        void BuildCutArea()
        {
            if (workingCanvas != null)
            {
                if (IsContourClosed())
                {
                    if (cutArea != null)
                    {
                        workingCanvas.Children.Remove(cutArea);
                    }
                    var contour = GetContourPoints(false);
                    DrawingPoligon(contour);
                }
            }
        }
        EMarker GetLastMarker()
        {
            if (markers.Count != 0)
            {
                return markers.Last<EMarker>();
            }
            return null;
        }
        private void SetLineShiftTimer()
        {
            lineShiftTimer = new System.Timers.Timer(1000 / lineStrokeOffsetSpeed);
            lineShiftTimer.Elapsed += OnTimedEvent;
            lineShiftTimer.Enabled = true;
        }
        private void MarkerSelection(Point cursor)
        {
            if (IsCursorOnImage(workingCanvas.PointToScreen(cursor)))
            {
                if (markers?.Count != 0)
                {
                    foreach (var marker in markers)
                    {
                        if (IsPointInSelectedArea(marker.Center, cursor, selectionMarkerArea))
                        {
                            marker.IsSelected = true;
                        }
                        else
                        {
                            marker.IsSelected = false;
                        }
                    }
                }
            }
            else
            {
                while (GetSelectedMarker() != null)
                {
                    GetSelectedMarker().IsSelected = false;
                }
            }
        }
        EMarker GetMarkerFromPoint(Point point)
        {
            if (markers != null && markers.Count != 0)
            {
                foreach (var marker in markers)
                {
                    if (marker.Center.X == point.X && marker.Center.Y == point.Y) { return marker; }
                }
            }
            return null;
        }
        EMarker GetSelectedMarker()
        {
            if (markers.Count != 0)
            {
                foreach (var marker in markers)
                {
                    if (marker.IsSelected)
                    {
                        return marker;
                    }
                }
            }
            return null;
        }
        List<Point> GetPointRoad(bool fromSource)
        {
            var result = new List<Point>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i == 0)
                {
                    if (fromSource)
                    {
                        result.Add(new Point { X = Math.Round(lines[0].SourceLocationPoint1.X, roundLevel), Y = Math.Round(lines[0].SourceLocationPoint1.Y, roundLevel) });
                        result.Add(new Point { X = Math.Round(lines[0].SourceLocationPoint2.X, roundLevel), Y = Math.Round(lines[0].SourceLocationPoint2.Y, roundLevel) });
                    }
                    else
                    {
                        result.Add(new Point { X = Math.Round(lines[0].line.X1, roundLevel), Y = Math.Round(lines[0].line.Y1, roundLevel) });
                        result.Add(new Point { X = Math.Round(lines[0].line.X2, roundLevel), Y = Math.Round(lines[0].line.Y2, roundLevel) });
                    }
                }
                else
                {
                    if (fromSource)
                    {
                        if (Math.Round(result.Last<Point>().X, roundLevel) != Math.Round(lines[i].SourceLocationPoint1.X, roundLevel) || Math.Round(result.Last<Point>().Y, roundLevel) != Math.Round(lines[i].SourceLocationPoint1.Y, roundLevel))
                        {
                            result.Add(new Point { X = Math.Round(lines[i].SourceLocationPoint1.X, roundLevel), Y = Math.Round(lines[i].SourceLocationPoint1.Y, roundLevel) });
                        }
                        else if (Math.Round(result.Last<Point>().X, roundLevel) != Math.Round(lines[i].SourceLocationPoint2.X, roundLevel) || Math.Round(result.Last<Point>().Y, roundLevel) != Math.Round(lines[i].SourceLocationPoint2.Y, roundLevel))
                        {
                            result.Add(new Point { X = Math.Round(lines[i].SourceLocationPoint2.X, roundLevel), Y = Math.Round(lines[i].SourceLocationPoint2.Y, roundLevel) });
                        }
                    }
                    else
                    {
                        if (Math.Round(result.Last<Point>().X, roundLevel) != Math.Round(lines[i].line.X1, roundLevel) || Math.Round(result.Last<Point>().Y, roundLevel) != Math.Round(lines[i].line.Y1, roundLevel))
                        {
                            result.Add(new Point { X = Math.Round(lines[i].line.X1, roundLevel), Y = Math.Round(lines[i].line.Y1, roundLevel) });
                        }
                        else if (Math.Round(result.Last<Point>().X, roundLevel) != Math.Round(lines[i].line.X2, roundLevel) || Math.Round(result.Last<Point>().Y, roundLevel) != Math.Round(lines[i].line.Y2, roundLevel))
                        {
                            result.Add(new Point { X = Math.Round(lines[i].line.X2, roundLevel), Y = Math.Round(lines[i].line.Y2, roundLevel) });
                        }
                    }
                }
            }
            return result;
        }
        PointCollection GetContourPoints(bool fromSource)
        {
            var result = new PointCollection();
            var road = new List<Point>();
            if (fromSource)
            {
                road = GetPointRoad(true);
            }
            else { road = GetPointRoad(false); }
            if (road.Count == 0) { return result; }
            var listElementsEntryMoreOnce = road.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (listElementsEntryMoreOnce?.Count() != 0)
            {
                bool IsStartContour = false;
                for (int i = 0; i < road.Count(); i++)
                {
                    if (!IsStartContour && road[i] == listElementsEntryMoreOnce.First<Point>())
                    {
                        result.Add(road[i]);
                        IsStartContour = true;
                        continue;
                    }
                    if (IsStartContour && road[i] != listElementsEntryMoreOnce.First<Point>())
                    {
                        result.Add(road[i]);
                        continue;
                    }
                    if (IsStartContour && road[i] == listElementsEntryMoreOnce.First<Point>())
                    {
                        result.Add(road[i]);
                        break;
                    }
                }
            }
            return result;
        }
        private bool IsPointInSelectedArea(Point point, Point centerOfArea, int areaRadius)
        {
            double dx = point.X - centerOfArea.X;
            double dy = point.Y - centerOfArea.Y;
            if (Math.Sqrt(dx * dx + dy * dy) <= areaRadius)
            {
                return true;
            }
            else { return false; }
        }
        bool IsLinesEquals(Line firstLine, Line secondLine)
        {
            if (firstLine.X1 == secondLine.X1 && firstLine.X2 == secondLine.X2 ||
                firstLine.X1 == secondLine.X2 && firstLine.X2 == secondLine.X1)
            {
                if (firstLine.Y1 == secondLine.Y1 && firstLine.Y2 == secondLine.Y2 ||
                firstLine.Y1 == secondLine.Y2 && firstLine.Y2 == secondLine.Y1)
                {
                    return true;
                }
            }
            return false;
        }
        bool IsLineAlreadyExist(Line line)
        {
            if (lines != null)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (IsLinesEquals(lines[i].line, line))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        bool IsMarkerEquals(Point firstMarkerCenter, Point secondMarkerCenter)
        {
            if (firstMarkerCenter.X == secondMarkerCenter.X && firstMarkerCenter.Y == secondMarkerCenter.Y)
            {
                return true;
            }
            return false;
        }
        bool IsMarkerAlreadyExist(Point markerCenter)
        {
            foreach (var m in markers)
            {
                if (IsMarkerEquals(m.Center, markerCenter))
                {
                    return true;
                }
            }
            return false;
        }
        bool IsContourClosed()
        {
            if (markers.Count() != 0)
            {
                if (markers.Count() == lines.Count())
                {
                    return true;
                }
            }
            return false;
        }
        bool IsMoreOneLineMarker(EMarker marker)
        {
            if (lines == null || lines.Count == 0)
            {
                return false;
            }
            int i = 0;
            foreach (var line in lines)
            {
                if (line.line.X1 == marker.Center.X && line.line.Y1 == marker.Center.Y) { i++; }
                if (line.line.X2 == marker.Center.X && line.line.Y2 == marker.Center.Y) { i++; }
            }
            if (i > 1) { return true; }
            return false;
        }
        bool IsCursorOnImage(Point relativeToScreenCursorPos)
        {
            if (img == null)
            {
                return false;
            }
            else
            {
                var imgPointTopLeftRelative = img.PointToScreen(new Point { X = 0, Y = 0 });
                var imgPointBottomRightRelative = img.PointToScreen(new Point { X = img.ActualWidth, Y = img.ActualHeight });
                if (relativeToScreenCursorPos.X < imgPointTopLeftRelative.X || relativeToScreenCursorPos.Y < imgPointTopLeftRelative.Y
                    || relativeToScreenCursorPos.X > imgPointBottomRightRelative.X || relativeToScreenCursorPos.Y > imgPointBottomRightRelative.Y)
                {
                    return false;
                }
                return true;
            }
        }
        bool IsImgSizeChanged()
        {
            if (img == null)
            {
                throw new NullReferenceException("EImage is a null");
            }
            if (old_imageWidth != img.ActualWidth || old_imageHeight != img.ActualHeight)
            {
                return true;
            }
            return false;
        }
        bool IsCanvasSizeChanged()
        {
            if (workingCanvas == null)
            {
                throw new NullReferenceException("WorkingCanvas is a null");
            }
            if (old_workingCanvasWidth != workingCanvas.ActualWidth || old_workingCanvasHeight != workingCanvas.ActualHeight)
            {
                return true;
            }
            return false;
        }
        void UpdateMarkersPosition()
        {
            if (markers == null && markers.Count == 0) { return; }

            if (IsCanvasSizeChanged() || IsImgSizeChanged())
            {
                foreach (var marker in markers)
                {
                    marker.Center = ConvertImagePointToCanvasPoint(ConvertToImgCoordinatesSourcePoint(marker.SourceLocation));
                }
            }
        }
        void UpdateLinesPosition()
        {
            if (lines == null && lines.Count == 0) { return; }
            if (IsCanvasSizeChanged() || IsImgSizeChanged())
            {
                foreach (var line in lines)
                {
                    var point1 = ConvertImagePointToCanvasPoint(ConvertToImgCoordinatesSourcePoint(line.SourceLocationPoint1));
                    var point2 = ConvertImagePointToCanvasPoint(ConvertToImgCoordinatesSourcePoint(line.SourceLocationPoint2));
                    line.line.X1 = point1.X; line.line.Y1 = point1.Y; line.line.X2 = point2.X; line.line.Y2 = point2.Y;
                }
            }
        }
        void UpdateCutAreaPosition()
        {
            if (cutArea != null && workingCanvas != null)
            {
                workingCanvas.Children.Remove(cutArea);
                cutArea = null;
                BuildCutArea();
            }
        }
        void DrawingElementsUpdateSize()
        {
            if (markers == null && lines == null)
            {
                UpdateOldSizes();
                return;
            }
            if (markers.Count == 0 && lines.Count == 0)
            {
                UpdateOldSizes();
                return;
            }
            if (markers.Count > 0)
            {
                UpdateMarkersPosition();
            }
            if (lines.Count > 0)
            {
                UpdateLinesPosition();
            }
            UpdateCutAreaPosition();
            UpdateOldSizes();
        }
        void UpdateOldSizes()
        {
            if (img != null)
            {
                old_imageWidth = img.ActualWidth;
                old_imageHeight = img.ActualHeight;
            }
            if (workingCanvas != null)
            {
                old_workingCanvasWidth = workingCanvas.ActualWidth;
                old_workingCanvasHeight = workingCanvas.ActualHeight;
            }
        }
        void DeleteCutArea()
        {
            if (workingCanvas != null)
            {
                if (!IsContourClosed())
                {
                    if (cutArea != null)
                    {
                        workingCanvas.Children.Remove(cutArea);
                    }
                }
            }
        }
        void DeleteMarkers()
        {
            if (markers == null || markers.Count == 0) { return; }
            foreach (var marker in markers)
            {
                marker.DelFromCanvas();
            }
            markers.Clear();
        }
        void DeleteLines()
        {
            if (lines == null || lines.Count == 0) { return; }
            foreach (var eline in lines)
            {
                eline.DelFromCanvas();
            }
            lines.Clear();
        }
        void ClearAllDrawings()
        {
            DeleteMarkers();
            DeleteLines();
            ContourIsOpened?.Invoke();
            CheckForEnableTools();
        }

        #region Converts
        Point ConvertImagePointToCanvasPoint(Point imagePoint)
        {
            if (img == null)
            {
                throw new NullReferenceException("EImage is Empty");
            }
            if (workingCanvas == null)
            {
                throw new NullReferenceException("WorkingCanvas is Empty");
            }
            var imageWorldPoint = img.PointToScreen(imagePoint);
            return workingCanvas.PointFromScreen(imageWorldPoint);
        }
        Point ConvertCanvasPointToImagePoint(Point canvasPoint)
        {
            if (img == null)
            {
                throw new NullReferenceException("EImage is Empty");
            }
            if (workingCanvas == null)
            {
                throw new NullReferenceException("WorkingCanvas is Empty");
            }

            var canvasWorldPoint = workingCanvas.PointToScreen(canvasPoint);

            if (!IsCursorOnImage(canvasWorldPoint))
            {
                throw new ArgumentOutOfRangeException("Point out of EImage");
            }
            return img.PointFromScreen(canvasWorldPoint);
        }
        Point ConvertToSourceCoordinatesImagePoint(Point relativeToImage)
        {
            if (img == null)
            {
                throw new NullReferenceException("image is a null");
            }
            var sourceSize = ImageCutter.GetImageSizePixels(img.CurrentBitmap);
            if (sourceSize == null) { throw new ArgumentException("Bitmap image not exist or not Valid"); }
            double[] currentImgSize = { img.ActualWidth, img.ActualHeight };
            double[] relation = { relativeToImage.X / currentImgSize[0], relativeToImage.Y / currentImgSize[1] };
            return new Point(sourceSize[0] * relation[0], sourceSize[1] * relation[1]);
        }
        Point ConvertToImgCoordinatesSourcePoint(Point relativeToSource)
        {
            if (img == null)
            {
                throw new NullReferenceException("image is a null");
            }
            var sourceSize = ImageCutter.GetImageSizePixels(img.CurrentBitmap);
            if (sourceSize == null) { throw new ArgumentException("Bitmap image not exist or not Valid"); }
            double[] currentImgSize = { img.ActualWidth, img.ActualHeight };
            double[] relation = { relativeToSource.X / sourceSize[0], relativeToSource.Y / sourceSize[1] };
            return new Point(currentImgSize[0] * relation[0], currentImgSize[1] * relation[1]);
        }
        #endregion Converts

        void GetCutImg()
        {
            if (IsContourClosed())
            {
                if (img?.CurrentBitmap != null)
                {
                    var contour = GetContourPoints(true);
                    img.CutBitmap(contour);
                }
            }
        }
        void SaveCutImg()
        {
            #region BuildSaveFileDialog
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = saveFileDialogTitle,
                DefaultExt = ".png",
                Filter = $"{pngFormatAnnotation}|*.png"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
            }
            #endregion BuildSaveFileDialog

            if (!string.IsNullOrEmpty(dlg.FileName))
            {
                img.SaveCuttedBitmap(dlg.FileName);
            }
        }
        void ImportImage()
        {
            #region BuildOpenFileDialog
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = openFileDialogTitle,
                Filter = $"{openFormatAnnotation}|*.png;*.bmp;*.jpg;*.jpeg;*.gif",
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
            }
            #endregion BuildOpenFileDialog

            if (!string.IsNullOrEmpty(dlg.FileName))
            {
                if (ImageCutter.IsFileImage(dlg.FileName))
                {
                    PathToImg = dlg.FileName;
                }
                else
                {
                    System.Windows.MessageBox.Show(unsupportedFormatText, titleOfMbox, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        void SetAllowDragNDrop(bool key)
        {
            if (key)
            {
                if (workingCanvas != null && notify != null)
                {
                    workingCanvas.AllowDrop = true;
                    notify.Content = DragNDropText;
                }
            }
            else
            {
                if (workingCanvas != null)
                {
                    workingCanvas.AllowDrop = false;
                    notify.Content = notifyText;
                }
            }
        }
        void CheckForEnableTools()
        {
            if (markers != null)
            {
                if (markers.Count != 0)
                {
                    SetEnableTools(true);
                }
                else
                {
                    SetEnableTools(false);
                }
            }
        }
        void SetEnableTools(bool key)
        {
            if (toolsGrid != null)
            {
                if (key)
                {
                    toolsGrid.IsEnabled = true;
                }
                else
                {
                    toolsGrid.IsEnabled = false;
                }
            }


        }
        #endregion DrawingInstruments

        #endregion Privates	

        #region Public
        public void ExportWithoutDialog()
        {
            if (img.CuttedBitmap != null)
            {
                if (!string.IsNullOrEmpty(pathToExp))
                {
                    try
                    {
                        img.SaveCuttedBitmap(pathToExp);
                    }
                    catch
                    {
                        throw new Exception("Error on save cutted bitmap, may be path to save is not valid");
                    }
                }
            }
        }
        public void SetImage(string pathToImage)
        {
            if (img != null)
            {
                img.SetBitmapSource(pathToImage);
            }
            else
            {
                throw new ArgumentNullException("EImage is a null");
            }
        }
        #endregion Public

        #endregion Methods
    }
}
