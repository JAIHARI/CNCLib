﻿////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2018 Herbert Aitenbichler

  CNCLib is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  CNCLib is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  http://www.gnu.org/licenses/
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CNCLib.GCode.Commands;
using Framework.Drawing;

namespace CNCLib.GCode.GUI.Controls
{
    /// <summary>
    /// Interaction logic for GCodeUserControl.xaml
    /// </summary>
    public partial class GCodeUserControl : System.Windows.Controls.UserControl
    {
        private readonly GCodeBitmapDraw _bitmapDraw = new GCodeBitmapDraw();

        public GCodeUserControl()
        {
            InitializeComponent();

            MouseWheel += GCodeUserControl_MouseWheel;

            MouseDown += GCodeUserControl_MouseDown;
            MouseUp   += GCodeUserControl_MouseUp;
            MouseMove += GCodeUserControl_MouseMove;
        }

        #region Properties

        public double SizeX { get; set; } = 140.0;
        public double SizeY { get; set; } = 45.0;

        public static readonly DependencyProperty GotoPosCommandProperty = DependencyProperty.Register("GotoPos", typeof(ICommand), typeof(GCodeUserControl), new PropertyMetadata(default(ICommand)));

        public ICommand GotoPos { get => (ICommand) GetValue(GotoPosCommandProperty); set => SetValue(GotoPosCommandProperty, value); }

        /// <summary>
        /// Command Property
        /// </summary>
        public static DependencyProperty CommandsProperty = DependencyProperty.Register("Commands", typeof(CommandList), typeof(GCodeUserControl), new PropertyMetadata(OnCommandsChanged));

        public CommandList Commands { get => (CommandList) GetValue(CommandsProperty); set => SetValue(CommandsProperty, value); }

        private static void OnCommandsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl.InvalidateVisual();
        }

        #region View Properties

        /// <summary>
        /// Zoom Property
        /// </summary>
        public static DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(OnZoomChanged));

        public double Zoom { get => (double) GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }

        private static void OnZoomChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl.OnZoomChanged(e);
        }

        private void OnZoomChanged(DependencyPropertyChangedEventArgs e)
        {
            var center    = new System.Drawing.PointF((float) (ActualWidth / 2.0), (float) (ActualHeight / 2.0));
            var centerold = _bitmapDraw.FromClient(center);

            _bitmapDraw.Zoom = (double) e.NewValue;

            var centernew = _bitmapDraw.FromClient(center);
            OffsetX += centerold.X0 - centernew.X0;
            OffsetY -= centerold.Y0 - centernew.Y0;

            // adjust x/y to center again
            InvalidateVisual();
        }

        /// <summary>
        /// OffsetX Property
        /// </summary>
        public static DependencyProperty OffsetXProperty = DependencyProperty.Register("OffsetX", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(OnOffsetXChanged));

        public double OffsetX { get => (double) GetValue(OffsetXProperty); set => SetValue(OffsetXProperty, value); }

        private static void OnOffsetXChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.OffsetX = (double) e.NewValue;
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// OffsetY Property
        /// </summary>
        public static DependencyProperty OffsetYProperty = DependencyProperty.Register("OffsetY", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(OnOffsetYChanged));

        public double OffsetY { get => (double) GetValue(OffsetYProperty); set => SetValue(OffsetYProperty, value); }

        private static void OnOffsetYChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.OffsetY = (double) e.NewValue;
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// RotateAngle Property
        /// </summary>
        public static DependencyProperty RotateAngleProperty = DependencyProperty.Register("RotateAngle", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(OnRotateAngleChanged));

        public double RotateAngle { get => (double) GetValue(RotateAngleProperty); set => SetValue(RotateAngleProperty, value); }

        private static void OnRotateAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl                                   = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.Rotate = godeCtrl._rotate = new Rotate3D((double) e.NewValue, godeCtrl.RotateVector);
            //godeCtrl._rotateInvers = new Rotate3D(-(double)e.NewValue, godeCtrl._rotaryVector);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// RotateAngle Property
        /// </summary>
        public static DependencyProperty RotateVectorProperty =
            DependencyProperty.Register("RotateVector", typeof(double[]), typeof(GCodeUserControl), new PropertyMetadata(new double[] { 0, 0, 1 }, OnRotateVectorChanged));

        public double[] RotateVector { get => (double[]) GetValue(RotateVectorProperty); set => SetValue(RotateVectorProperty, value); }

        private static void OnRotateVectorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl                                   = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.Rotate = godeCtrl._rotate = new Rotate3D(godeCtrl.RotateAngle, (double[]) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        #endregion

        #region Color Properties

        /// <summary>
        /// MachineColor Property
        /// </summary>
        public static DependencyProperty MachineColorProperty =
            DependencyProperty.Register("MachineColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Black, OnMachineColorChanged));

        public Color MachineColor { get => (Color) GetValue(MachineColorProperty); set => SetValue(MachineColorProperty, value); }

        private static void OnMachineColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.MachineColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// LaserOnColor Property
        /// </summary>
        public static DependencyProperty LaserOnColorProperty =
            DependencyProperty.Register("LaserOnColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Red, OnLaserOnColorChanged));

        public Color LaserOnColor { get => (Color) GetValue(LaserOnColorProperty); set => SetValue(LaserOnColorProperty, value); }

        private static void OnLaserOnColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.LaserOnColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// LaserOffColor Property
        /// </summary>
        public static DependencyProperty LaserOffColorProperty =
            DependencyProperty.Register("LaserOffColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnLaserOffColorChanged));

        public Color LaserOffColor { get => (Color) GetValue(LaserOffColorProperty); set => SetValue(LaserOffColorProperty, value); }

        private static void OnLaserOffColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.LaserOffColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// CutColor Property
        /// </summary>
        public static DependencyProperty CutColorProperty = DependencyProperty.Register("CutColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnCutColorChanged));

        public Color CutColor { get => (Color) GetValue(CutColorProperty); set => SetValue(CutColorProperty, value); }

        private static void OnCutColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.CutColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// CutDotColor Property
        /// </summary>
        public static DependencyProperty CutDotColorProperty =
            DependencyProperty.Register("CutDotColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnCutDotColorChanged));

        public Color CutDotColor { get => (Color) GetValue(CutDotColorProperty); set => SetValue(CutDotColorProperty, value); }

        private static void OnCutDotColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.CutDotColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }


        /// <summary>
        /// CutArcColor Property
        /// </summary>
        public static DependencyProperty CutEllipseColorProperty =
            DependencyProperty.Register("CutEllipseColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnCutEllipseColorChanged));

        public Color CutEllipseColor { get => (Color) GetValue(CutEllipseColorProperty); set => SetValue(CutEllipseColorProperty, value); }

        private static void OnCutEllipseColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.CutEllipseColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// CutArcColor Property
        /// </summary>
        public static DependencyProperty CutArcColorProperty =
            DependencyProperty.Register("CutArcColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnCutArcColorChanged));

        public Color CutArcColor { get => (Color) GetValue(CutArcColorProperty); set => SetValue(CutArcColorProperty, value); }

        private static void OnCutArcColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.CutArcColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// HelpLineColor Property
        /// </summary>
        public static DependencyProperty FastMoveColorProperty =
            DependencyProperty.Register("FastMoveColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnFastMoveColorChanged));

        public Color FastMoveColor { get => (Color) GetValue(FastMoveColorProperty); set => SetValue(FastMoveColorProperty, value); }

        private static void OnFastMoveColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.FastMoveColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// HelpLineColor Property
        /// </summary>
        public static DependencyProperty HelpLineColorProperty =
            DependencyProperty.Register("HelpLineColor", typeof(Color), typeof(GCodeUserControl), new PropertyMetadata(Colors.Orange, OnHelpLineColorChanged));

        public Color HelpLineColor { get => (Color) GetValue(HelpLineColorProperty); set => SetValue(HelpLineColorProperty, value); }

        private static void OnHelpLineColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.HelpLineColor = ColorToColor((Color) e.NewValue);
            godeCtrl.InvalidateVisual();
        }

        #endregion

        /// <summary>
        /// LaserSize Property
        /// </summary>
        public static DependencyProperty LaserSizeProperty = DependencyProperty.Register("LaserSize", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(0.25, OnLaserSizeChanged));

        public double LaserSize { get => (double) GetValue(LaserSizeProperty); set => SetValue(LaserSizeProperty, value); }

        private static void OnLaserSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.LaserSize = (double) e.NewValue;
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// CutterSize Property
        /// </summary>
        public static DependencyProperty CutterSizeProperty = DependencyProperty.Register("CutterSize", typeof(double), typeof(GCodeUserControl), new PropertyMetadata(0.0, OnCutterSizeChanged));

        public double CutterSize { get => (double) GetValue(CutterSizeProperty); set => SetValue(CutterSizeProperty, value); }

        private static void OnCutterSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var godeCtrl = (GCodeUserControl) dependencyObject;
            godeCtrl._bitmapDraw.CutterSize = (double) e.NewValue;
            godeCtrl.InvalidateVisual();
        }

        /// <summary>
        /// MouseOverX Property
        /// </summary>
        private static readonly DependencyPropertyKey MouseOverPositionXPropertyKey =
            DependencyProperty.RegisterReadOnly("MouseOverPositionX", typeof(double?), typeof(GCodeUserControl), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MouseOverPositionXProperty = MouseOverPositionXPropertyKey.DependencyProperty;

        public double? MouseOverPositionX { get => (double?) GetValue(MouseOverPositionXProperty); private set => SetValue(MouseOverPositionXPropertyKey, value); }

        /// <summary>
        /// MouseOverY Property
        /// </summary>
        private static readonly DependencyPropertyKey MouseOverPositionYPropertyKey =
            DependencyProperty.RegisterReadOnly("MouseOverPositionY", typeof(double?), typeof(GCodeUserControl), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MouseOverPositionYProperty = MouseOverPositionYPropertyKey.DependencyProperty;

        public double? MouseOverPositionY { get => (double?) GetValue(MouseOverPositionYProperty); private set => SetValue(MouseOverPositionYPropertyKey, value); }

        #endregion

        #region Drag/Drop

        enum EDraggingType
        {
            NoDragging,
            Position,
            RotateAngle
        }

        private EDraggingType _draggingType = EDraggingType.NoDragging;

        private Point     _mouseDownPos;
        private Point3D   _mouseDownCNCPos;
        private double    _mouseDownCNCOffsetX;
        private double    _mouseDownCNCOffsetY;
        private Stopwatch _sw = new Stopwatch();

        private void GCodeUserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Zoom *= 1.1;
            }
            else
            {
                Zoom /= 1.1;
            }

            InvalidateVisual();
        }

        Rotate3D _rotate = new Rotate3D();

        private bool IsGotoPosKey()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        private void GCodeUserControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (_draggingType == EDraggingType.NoDragging)
            {
                var mousePos = e.GetPosition(this);
                var pt       = new System.Drawing.PointF((float) mousePos.X, (float) mousePos.Y);

                if (IsGotoPosKey())
                {
                    var gcoderotated = _bitmapDraw.FromClient(pt, 0.0);
                    if (GotoPos != null && GotoPos.CanExecute(gcoderotated))
                    {
                        GotoPos.Execute(gcoderotated);
                    }
                }
                else
                {
                    _mouseDownPos        = mousePos;
                    _mouseDownCNCPos     = _bitmapDraw.FromClient(pt);
                    _mouseDownCNCOffsetX = OffsetX;
                    _mouseDownCNCOffsetY = OffsetY;
                    _sw.Start();
                    Mouse.Capture(this);

                    _draggingType = e.RightButton == MouseButtonState.Pressed ? EDraggingType.RotateAngle : EDraggingType.Position;
                }
            }
        }

        private void GCodeUserControl_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos     = e.GetPosition(this);
            var pt           = new System.Drawing.PointF((float) mousePos.X, (float) mousePos.Y);
            var gcoderotated = _bitmapDraw.FromClient(pt, 0.0);

            MouseOverPositionX = Math.Round(gcoderotated.X0, 3);
            MouseOverPositionY = Math.Round(gcoderotated.Y0, 3);

            if (MouseOverPositionX >= 10000.0)
            {
                MouseOverPositionX = 9999.999;
            }

            if (MouseOverPositionY >= 10000.0)
            {
                MouseOverPositionY = 9999.999;
            }

            if (MouseOverPositionX <= -10000.0)
            {
                MouseOverPositionX = -9999.999;
            }

            if (MouseOverPositionY <= -10000.0)
            {
                MouseOverPositionY = -9999.999;
            }

            switch (_draggingType)
            {
                case EDraggingType.NoDragging: return;

                case EDraggingType.Position:
                {
                    _bitmapDraw.OffsetX = _mouseDownCNCOffsetX; // faster: do not assign with Dependent Property
                    _bitmapDraw.OffsetY = _mouseDownCNCOffsetY;
                    var    c    = _bitmapDraw.FromClient(pt); // recalculate with orig offset
                    double newX = _mouseDownCNCOffsetX - (c.X0 - _mouseDownCNCPos.X0);
                    double newY = _mouseDownCNCOffsetY + (c.Y0 - _mouseDownCNCPos.Y0);
                    _bitmapDraw.OffsetX = newX;
                    _bitmapDraw.OffsetY = newY;
                    break;
                }
                case EDraggingType.RotateAngle:
                {
                    double diffX = mousePos.X - _mouseDownPos.X;
                    double diffY = mousePos.Y - _mouseDownPos.Y;

                    double maxdiffX = RenderSize.Width;
                    double maxdiffY = RenderSize.Height;

                    double rotateX = diffX / maxdiffX;
                    double rotateY = diffY / maxdiffY;

                    RotateAngle = 2.0 * Math.PI * (Math.Abs(rotateX) > Math.Abs(rotateY) ? rotateX : rotateY);

                    RotateVector[1] = diffX;
                    RotateVector[0] = -diffY;

                    _bitmapDraw.Rotate = _rotate = new Rotate3D(RotateAngle, RotateVector);
                    break;
                }
            }

            if (_sw.ElapsedMilliseconds > 300)
            {
                _sw.Start();
                InvalidateVisual();
            }
        }

        private void GCodeUserControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (_draggingType != EDraggingType.NoDragging)
            {
                switch (_draggingType)
                {
                    case EDraggingType.Position:
                        OffsetX = _bitmapDraw.OffsetX;
                        OffsetY = _bitmapDraw.OffsetY;
                        break;
                }

                Mouse.Capture(null);
                InvalidateVisual();
                _draggingType = EDraggingType.NoDragging;
            }
        }

        #endregion

        #region render

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_draggingType == EDraggingType.NoDragging)
            {
                _bitmapDraw.RenderSize = new System.Drawing.Size((int) sizeInfo.NewSize.Width, (int) sizeInfo.NewSize.Height);
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            DrawCommands(drawingContext);
        }

        #endregion

        #region private

        static System.Drawing.Color ColorToColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }


        private void DrawCommands(DrawingContext context)
        {
            if (_bitmapDraw.RenderSize.Height == 0 || _bitmapDraw.RenderSize.Width == 0)
            {
                return;
            }

            if (_draggingType == EDraggingType.NoDragging)
            {
                _bitmapDraw.SizeX = SizeX;
                _bitmapDraw.SizeY = SizeY;
            }

            var curBitmap = _bitmapDraw.DrawToBitmap(Commands);
            var stream    = new MemoryStream();
            curBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            var cc = new ImageSourceConverter().ConvertFrom(stream);
            context.DrawImage((ImageSource) cc, new Rect(0, 0, ActualWidth, ActualHeight));
            curBitmap.Dispose();
        }

        #endregion
    }
}