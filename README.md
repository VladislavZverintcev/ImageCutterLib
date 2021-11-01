# ImageCutterLib
UserControl Libary contains CutControl, for make Image clippings and export to png

  Usercontrol contains horizontal bottom panel with import and export button. 
You can hide this panel with property set: ModeShowHorizontalPanel = false; 
(And import or export button with ModeShowImportButton, ModeShowExportButton)
Also control contains work area, where the imported image is located, and user
click on area and make a markers and lines. When contour is closed, you can export
cutted image to a specific path with save file dialog, or programmatically with method ExportWithoutDialog();
  In Right side of control (or left side if ModeLeftSideTools == true) located vertical tools bar with two buttons.
First button - for delete last user step. Second button for clear all markers and lines.
Cut contol supported Drag & Drop images, for this you must set ModeDragAndDrop = true;
And some additions: if PathToImg has been changed, picture load automatical...


Opened parameters for configurate:

BrushLines;
BrushMarkers;
BrushArea;
BrushNotify;
BrushBorderImage;
BorderImageThickness;
ButtonStyle;
LineThickness;
LineSpeed;
MarkerSize;
MarkerSizeLarge;
SelectionMarkerArea;
ImportButText;
ExportButText;
SaveFileDialogTitle;
OpenFileDialogTitle;
PngFormatAnnotation;
OpenFormatAnnotation;
PathToImg;
PathtoExp;
NotifyText;
NotifyFontSize;
ResetDrawingsButText;
UnsupportedFormatText;
TitleOfMbox;
DragNDropText;
ModeDragAndDrop;
ModeShowHorizontalPanel;
ModeShowImportButton;
ModeShowExportButton;
ModeLeftSideTools;
