﻿////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2017 Herbert Aitenbichler

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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CNCLib.GCode.Commands;
using CNCLib.Logic.Contracts.DTO;
using Framework.Tools.Helpers;
using Framework.Tools.Drawing;

namespace CNCLib.GCode.Load
{
	public class LoadHPGL : LoadBase
    {
        bool _DEBUG = false;

        #region HPGLCommand und HPGLLine

        class HPGLCommand
        {
            public enum HPGLCommandType
            {
                PenUp,
                PenDown,
                Other
            };
            public HPGLCommandType CommandType { get; set; } = HPGLCommandType.Other;
            public bool IsPenCommand { get { return CommandType == HPGLCommandType.PenUp || CommandType == HPGLCommandType.PenDown; } }
            public bool IsPenDownCommand { get { return CommandType == HPGLCommandType.PenDown; } }
            public bool IsPointToValid { get { return IsPenCommand; } }
            public Point3D PointFrom { get; set; }
            public Point3D PointTo { get; set; }
            public double? LineAngle { get; set; }
            public double? DiffLineAngleWithNext { get; set; }
            public string CommandString { get; set; }

            public void ResetCalculated()
            {
                PointFrom = null;
                DiffLineAngleWithNext = null;
                LineAngle = null;
            }
        }

        private class HPGLLine
        {
            public IEnumerable<HPGLCommand> PreCommands { get; set; }
            public IEnumerable<HPGLCommand> Commands { get; set; }
            public IEnumerable<HPGLCommand> PostCommands { get; set; }

            public Polygon2D Polygon { get { Load(); return _polygon; } }

            public double MaxX { get { Load();  return _maxX; } }
            public double MinX { get { Load(); return _minX; } }
            public double MaxY { get { Load(); return _maxY; } }
            public double MinY { get { Load(); return _minY; } }

            public bool IsClosed  { get { Load();  return _isClosed;  } }

            public bool IsEmbedded(HPGLLine to)
            {
                if (ReferenceEquals(this, to)) return false;
                bool isRectangleEmbedded =
                        MaxX >= to.MaxX && MinX <= to.MinX &&
                        MaxY >= to.MaxY && MinY <= to.MinY;
                if (!isRectangleEmbedded) return false;
                return IsEmbeddedEx(to);
            }

            public bool IsEmbeddedEx(HPGLLine to)
            {
                // TODO: we test points only!!!
                // but it would be necessary to thes the whole line 

                return _polygon.ArePointsInPolygon(to._polygon.Points);
            }

            public int Level { get { return ParentLine == null ? 0 : ParentLine.Level + 1; } }

            public HPGLLine ParentLine { get; set; }

            private void Load()
            {
                if (!_isLoaded)
                {
                    var points = new List<Point2D>();
                    if (Commands != null && Commands.Count() >= 1)
                    {
                        points.Add(Commands.First().PointFrom);
                        points.AddRange(Commands.Select(c => new Point2D() { X = c.PointTo.X ?? 0.0, Y = c.PointTo.Y ?? 0.0 }));
                    }
                    _polygon = new Polygon2D() { Points = points };
                    _maxX = _polygon.MaxX;
                    _minX = _polygon.MinX;
                    _maxY = _polygon.MaxY;
                    _minY = _polygon.MinY;
                    _isClosed = _polygon.IsClosed;
                    _isLoaded = true;
                }
            }

            private bool _isLoaded = false;
            private bool _isClosed;
            private double _maxX;
            private double _minX;
            private double _maxY;
            private double _minY;
            private Polygon2D _polygon;
        }

        #endregion

        #region Read PLT

        private IList<HPGLCommand> ReadHPGLCommandList()
        {
            var list = new List<HPGLCommand>();
            using (StreamReader sr = GetStreamReader())
            {
                string line;
                Point3D last = new Point3D();
                bool isPenUp = true;
                CommandStream stream = new CommandStream();

                while ((line = sr.ReadLine()) != null)
                {
                    stream.Line = line;

                    string[] cmds = new string[] { "PU", "PD", "PA", "PR" };
                    while (!stream.IsEOF())
                    {
                        stream.SkipSpaces();
                        int idx = stream.PushIdx();
                        int cmdidx = stream.IsCommand(cmds);

                        if (cmdidx >= 0)
                        {
                            switch (cmdidx)
                            {
                                case 0: isPenUp = true; break;
                                case 1: isPenUp = false; break;
                            }

                            while (stream.IsNumber())
                            {
                                 Point3D pt = new Point3D();
                                pt.X = stream.GetInt() / 40.0;
                                stream.IsCommand(",");
                                pt.Y = stream.GetInt() / 40.0;

                                AdjustOrig(ref pt);
 
                                if (cmdidx == 3)  // move rel
                                {
                                    pt.X += last.X;
                                    pt.Y += last.Y;
                                }

                                list.Add(new HPGLCommand()
                                {
                                    CommandType = isPenUp ? HPGLCommand.HPGLCommandType.PenUp : HPGLCommand.HPGLCommandType.PenDown,
                                    PointTo = pt
                                }
                                );

                                last = pt;

                                stream.IsCommand(",");
                            }
                        }
                        else if (stream.SkipSpaces() == ';')
                        {
                            stream.Next();
                        }
                        else
                        {
                            var hpglcmd = stream.ReadString(new char[] { ';' });
                            list.Add(new HPGLCommand() { CommandString = hpglcmd });
                        }
                    }
                }
            }

            return list;
        }

        private void RemoveFirstPenUp(IList<HPGLCommand> list)
        {
            // remove first PU0,0 PU50,50 PU 100,100 => autoscale problem
            var rlist = list.TakeWhile(h => !h.IsPenCommand || !h.IsPenDownCommand).ToList();
            int countPenUp = rlist.Count(h => h.IsPenCommand);

            foreach (var h in rlist)
            {
                if (h.IsPenCommand)
                {
                    if (countPenUp > 1)
                        list.Remove(h);
                    countPenUp--;
                }
            }
        }

        private void RemoveLastPenUp(IList<HPGLCommand> list)
        {
            // remove last PU0,0 => autoscale problem
            var rlist = list.Reverse().TakeWhile(h => !h.IsPenCommand || !h.IsPenDownCommand).ToList();

            foreach (var h in rlist)
            {
                if (h.IsPenCommand)
                {
                    list.Remove(h);
                }
            }
        }

        private void CalculateAngles(IEnumerable<HPGLCommand> list, Point3D firstfrom)
        {
            HPGLCommand last = null;
            if (firstfrom != null)
                last = new HPGLCommand()
                {
                    PointTo = firstfrom,
                    CommandType = HPGLCommand.HPGLCommandType.PenDown
                };

            foreach(var cmd in list)
            {
                cmd.ResetCalculated();
                if (cmd.IsPointToValid)
                {
                    if (last != null)
                    {
                        cmd.PointFrom = last.PointTo;
                        cmd.LineAngle = Math.Atan2((cmd.PointTo.Y ?? 0.0) - (cmd.PointFrom.Y ?? 0.0), (cmd.PointTo.X ?? 0.0) - (cmd.PointFrom.X ?? 0.0));
                        cmd.DiffLineAngleWithNext = null;

                        if (last.LineAngle.HasValue && cmd.IsPenDownCommand)
                        {
                            last.DiffLineAngleWithNext = last.LineAngle - cmd.LineAngle;
                            if (last.DiffLineAngleWithNext > Math.PI)
                                last.DiffLineAngleWithNext -= Math.PI * 2.0;

                            if (last.DiffLineAngleWithNext < -Math.PI)
                                last.DiffLineAngleWithNext += (Math.PI*2.0);
                        }
                    }
                    last = cmd;
                }
            }
        }

        private void AdjustOrig(ref Point3D pt)
        {
            if (LoadOptions.SwapXY)
            {
                var tmp = pt.X.Value;
                pt.X = pt.Y;
                pt.Y = -tmp;
            }
        }

        #endregion

        #region Load

        bool _lastIsPenUp = false;
        bool _needSpeed = false;

        public override void Load()
        {
            PreLoad();
            var list = ReadHPGLCommandList();

            RemoveFirstPenUp(list);
            RemoveLastPenUp(list);
            CalculateAngles(list, null);

            if (LoadOptions.AutoScale)
			{
				AutoScale(list);
			}

            if (LoadOptions.SmoothType != LoadOptions.SmoothTypeEnum.NoSmooth)
            {
                list = Smooth(list);
            }

            if (LoadOptions.ConvertType != LoadOptions.ConvertTypeEnum.NoConvert)
            {
                list = ConvertInvert(list);
            }

            AddComment("PenMoveType" , LoadOptions.PenMoveType.ToString() );

            switch (LoadOptions.PenMoveType)
            {
                case LoadOptions.PenType.CommandString:
					AddCommentForLaser();
                    break;
                case LoadOptions.PenType.ZMove:
                    AddComment("PenDownSpeed" , LoadOptions.EngraveDownSpeed );
                    AddComment("PenUpPos" ,     LoadOptions.EngravePosUp );
                    AddComment("PenDownPos" ,   LoadOptions.EngravePosDown );
                    break;
            }

            AddComment("Speed" , LoadOptions.MoveSpeed.ToString() );

			if (LoadOptions.PenMoveType == LoadOptions.PenType.ZMove)
			{
                AddCommands("M3");

				if (LoadOptions.EngravePosInParameter)
				{
					Commands.AddCommand(new SetParameterCommand() { GCodeAdd = "#1 = " + LoadOptions.EngravePosUp.ToString(CultureInfo.InvariantCulture) } );
					Commands.AddCommand(new SetParameterCommand() { GCodeAdd = "#2 = " + LoadOptions.EngravePosDown.ToString(CultureInfo.InvariantCulture) });
				}
			}

            if (LoadOptions.MoveSpeed.HasValue)
            {
                var setspeed = new G01Command();
                setspeed.AddVariable('F', LoadOptions.MoveSpeed.Value);
                Commands.Add(setspeed);
            }

            foreach(var cmd in list)
            { 
                if (!Command(cmd))
                {
                    Commands.Clear();
                    break;
                }
            }

			if (!_lastIsPenUp)
			{
                LoadPenUp();
			}

			if (LoadOptions.PenMoveType == LoadOptions.PenType.ZMove)
			{
                AddCommands("M5");
			}
			PostLoad();
        }

        private bool Command(HPGLCommand cmd)
        {
            bool isPenUp = true;

            if (cmd.IsPenCommand)
            {
                switch (cmd.CommandType)
                {
                    case HPGLCommand.HPGLCommandType.PenDown: isPenUp = false; break;
                    case HPGLCommand.HPGLCommandType.PenUp: isPenUp = true; break;
                }

                Point3D pt = Adjust(cmd.PointTo);

                if (isPenUp != _lastIsPenUp)
                {
                    if (isPenUp)
                    {
                        LoadPenUp();
                    }
                    else
                    {
                        LoadPenDown(Adjust(cmd.PointFrom));
                    }
                    _lastIsPenUp = isPenUp;
                }

                string hpglCmd;
                Command r;
                if (isPenUp)
                {
                    r = new G00Command();
                    hpglCmd = "PU";
                }
                else
                {
                    r = new G01Command();
                    AddCamBamPoint(pt);
                    hpglCmd = "PD";
                }
                r.AddVariable('X', pt.X.Value, false);
                r.AddVariable('Y', pt.Y.Value, false);
                if (_needSpeed)
                {
                    _needSpeed = false;
                    r.AddVariable('F', LoadOptions.MoveSpeed.Value);
                }
                Commands.AddCommand(r);

                r.ImportInfo = $"{hpglCmd}{(int)(pt.X.Value * 40.0)},{(int)(pt.Y.Value * 40.0)}";
            }
            else
            {
                var r = new GxxCommand();
                r.SetCode($";HPGL={cmd.CommandString}");
                r.ImportInfo = cmd.CommandString;
                Commands.AddCommand(r);
            }

            return true;
        }

        private void LoadPenDown(Point3D pt)
        {
            if (LoadOptions.PenMoveType == LoadOptions.PenType.ZMove)
            {
                var r = new G01Command();
                if (LoadOptions.EngravePosInParameter)
                {
                    r.AddVariableParam('Z', "2");
                }
                else
                {
                    r.AddVariable('Z', LoadOptions.EngravePosDown);
                }
                if (LoadOptions.EngraveDownSpeed.HasValue)
                {
                    r.AddVariable('F', LoadOptions.EngraveDownSpeed.Value);
                    _needSpeed = LoadOptions.MoveSpeed.HasValue;
                }
                Commands.AddCommand(r);
            }
            else // if (LoadOptions.PenMoveType == LoadInfo.PenType.Command)
            {
                LaserOn();
            }

            AddCamBamPLine();
            AddCamBamPoint(pt);
        }

        private void LoadPenUp()
        {
            if (LoadOptions.PenMoveType == LoadOptions.PenType.ZMove)
            {
                var r = new G00Command();
                if (LoadOptions.EngravePosInParameter)
                {
                    r.AddVariableParam('Z', "1");
                }
                else
                {
                    r.AddVariable('Z', LoadOptions.EngravePosUp);
                }
                Commands.AddCommand(r);
            }
            else // if (LoadOptions.PenMoveType == LoadInfo.PenType.Command)
            {
                LaserOff();
            }

            FinishCamBamPLine();
        }

        private Point3D Adjust(Point3D pt)
        {
            var ret = new Point3D
            {
                X = pt.X,
                Y = pt.Y,
                Z = pt.Z
            };

            ret.X += (double)LoadOptions.OfsX;
            ret.Y += (double)LoadOptions.OfsY;

            if (LoadOptions.ScaleX != 0)
                ret.X = Math.Round(ret.X.Value * (double)LoadOptions.ScaleX, 3);
            if (LoadOptions.ScaleY != 0)
                ret.Y = Math.Round(ret.Y.Value * (double)LoadOptions.ScaleY, 3);

            return ret;
        }

        #endregion

        #region AutoScale

        private void AutoScale(IList<HPGLCommand> list)
		{
			AddComment("AutoScaleX", LoadOptions.AutoScaleSizeX);
			AddComment("AutoScaleY", LoadOptions.AutoScaleSizeY);

			AddComment("AutoScaleDistX", LoadOptions.AutoScaleBorderDistX);
			AddComment("AutoScaleDistY", LoadOptions.AutoScaleBorderDistY);

            AddComment("AutoScaleCenter", LoadOptions.AutoScaleCenter.ToString());

            var minpt = new Point3D()
            {
                X = list.Where((x) => x.IsPenCommand).Min((c) => c.PointTo.X),
                Y = list.Where((x) => x.IsPenCommand).Min((c) => c.PointTo.Y)
            };
            var maxpt = new Point3D()
            {
                X = list.Where((x) => x.IsPenCommand).Max((c) => c.PointTo.X),
                Y = list.Where((x) => x.IsPenCommand).Max((c) => c.PointTo.Y)
            };

            decimal sizex = (decimal)maxpt.X.Value - (decimal)minpt.X.Value;
			decimal sizey = (decimal)maxpt.Y.Value - (decimal)minpt.Y.Value;

            decimal borderX = LoadOptions.AutoScaleBorderDistX;
            decimal borderY = LoadOptions.AutoScaleBorderDistY;

            decimal destSizeX = LoadOptions.AutoScaleSizeX - 2m * borderX;
            decimal destSizeY = LoadOptions.AutoScaleSizeY - 2m * borderY;

            LoadOptions.ScaleX = destSizeX / sizex;
			LoadOptions.ScaleY = destSizeY / sizey;

            if (LoadOptions.AutoScaleKeepRatio)
			{
				LoadOptions.ScaleX =
				LoadOptions.ScaleY = Math.Min(LoadOptions.ScaleX, LoadOptions.ScaleY);

                if (LoadOptions.AutoScaleCenter)
                {
                    decimal sizeXscaled = LoadOptions.ScaleX * sizex;
                    decimal sizeYscaled = LoadOptions.ScaleY * sizey;

                    borderX += (destSizeX - sizeXscaled) / 2m;
                    borderY += (destSizeY - sizeYscaled) / 2m;
                }
            }

            LoadOptions.OfsX = -((decimal)minpt.X.Value - borderX / LoadOptions.ScaleX);
            LoadOptions.OfsY = -((decimal)minpt.Y.Value - borderY / LoadOptions.ScaleY);
        }

        #endregion

        #region Convert-Line

        private IList<HPGLCommand> ConvertInvert(IList<HPGLCommand> list)
        {
            // split 

            var linelist = new List<HPGLLine>();
            IEnumerable<HPGLCommand> postCommands=null;
            IEnumerable<HPGLCommand> preCommands = null;

            int startidx = 0;
            preCommands = list.Skip(startidx).TakeWhile((e) => !e.IsPenCommand );
            startidx += preCommands.Count();

            while (startidx < list.Count())
            {
                HPGLLine line = GetHPGLLine(list, ref startidx);

                if (startidx >= list.Count() && line.Commands.Count() == 0)
                {
                    postCommands = line.PreCommands;
                }
                else
                {
                    linelist.Add(line);
                }
            }

            // rearrange

            var lines = OrderLines(linelist);

            // rebuild list

            var newlist = new List<HPGLCommand>();
            newlist.AddRange(preCommands);

            foreach(var line in lines)
            {
                newlist.AddRange(line.PreCommands);
                newlist.AddRange(line.Commands);
                newlist.AddRange(line.PostCommands);
            }

            if (postCommands!=null)
                newlist.AddRange(postCommands);

            return newlist;
        }

        private IEnumerable<HPGLLine> OrderLines(IEnumerable<HPGLLine> lines)
        {
            var newlist = new List<HPGLLine>();
            newlist.AddRange(lines.Where(l => !l.IsClosed));
            newlist.AddRange(OrderClosedLine(lines.Where(l => l.IsClosed)));

            return newlist;
        }

        private void CalcClosedLineParent(IEnumerable<HPGLLine> closedLines)
        {
            foreach (var line in closedLines)
            {
                foreach (var parentline in closedLines.Where(l => l.IsEmbedded(line)))
                {
                    if (line.ParentLine == null || line.ParentLine.IsEmbedded(parentline))
                    {
                        line.ParentLine = parentline;
                    }
                }
            }
        }

        const double _scale = 1000;

        private IEnumerable<HPGLLine> OrderClosedLine(IEnumerable<HPGLLine> closedLines)
        {
            var orderdlist = new List<HPGLLine>();
            if (closedLines.Count() > 0)
            {
                CalcClosedLineParent(closedLines);
                int maxlevel = closedLines.Max(l => l.Level);

                for (int level = maxlevel; level >= 0; level--)
                {
                    var linesOnLevel = closedLines.Where(l => l.Level == level);

                    if (LoadOptions.LaserSize != 0)
                    {
                        linesOnLevel = OffsetLines(_scale / 2.0 * (double)LoadOptions.LaserSize * ((level % 2 == 0) ? 1.0 : -1.0), linesOnLevel);
                    }

                    orderdlist.AddRange(OptimizeDistanze(linesOnLevel));
                }
            }
            return orderdlist;
        }

        private IEnumerable<HPGLLine> OffsetLines(double offset, IEnumerable<HPGLLine> lines)
        {
            var newlines = new List<HPGLLine>();

            foreach (var line in lines)
            {
                newlines.AddRange(OffsetLine(offset, line));
            }
            return newlines;
        }

        private IEnumerable<HPGLLine> OffsetLine(double offset, HPGLLine line)
        {
            var newlines = new List<HPGLLine>();
            newlines.Add(line);

            var co = new ClipperLib.ClipperOffset();
            var solution = new List<List<ClipperLib.IntPoint>>();
            var solution2 = new List<List<ClipperLib.IntPoint>>();
            solution.Add(line.Commands.Select(x => new ClipperLib.IntPoint(_scale * (x.PointFrom.X ?? 0.0), _scale * (x.PointFrom.Y ?? 0.0))).ToList());
            co.AddPaths(solution, ClipperLib.JoinType.jtRound, ClipperLib.EndType.etClosedPolygon);
            co.Execute(ref solution2, offset);
            var existingline = line;

            foreach (var polygon in solution2)
            {
                var newcmds = new List<HPGLCommand>();
                HPGLCommand last = null;

                foreach (var pt in polygon)
                {
                    var from = new Point3D() { X = pt.X / _scale, Y = pt.Y / _scale };
                    var hpgl = new HPGLCommand() { PointFrom = from, CommandType = HPGLCommand.HPGLCommandType.PenDown };
                    newcmds.Add(hpgl);
                    if (last != null)
                        last.PointTo = from;
                    last = hpgl;
                }
                last.PointTo = newcmds.First().PointFrom;

                if (existingline == null)
                {
                    // add new line
                    existingline = new HPGLLine()
                    {
                        PreCommands = new List<HPGLCommand>()
                                        {
                                            new HPGLCommand() { CommandType = HPGLCommand.HPGLCommandType.PenUp }
                                        },
                        PostCommands = new List<HPGLCommand>(),
                        ParentLine = line.ParentLine,
                    };
                    newlines.Add(existingline);
                }

                existingline.Commands = newcmds;
                existingline.PreCommands.Last(l => l.IsPenCommand).PointTo = newcmds.First().PointFrom;
                existingline = null;
            }
            return newlines;
        }

        private static IEnumerable<HPGLLine> OptimizeDistanze(IEnumerable<HPGLLine> lines)
        {
            var newlist = new List<HPGLLine>();
            newlist.Add(lines.First());

            var list = new List<HPGLLine>();
            list.AddRange(lines.Skip(1));

            while(list.Count() > 0)
            {
                Point3D ptfrom = newlist.Last().Commands.Last().PointTo;
                double maxdist = double.MaxValue;
                HPGLLine minDistLine=null;

                foreach (var l in list)
                {
                    Point3D pt = l.Commands.First().PointFrom;
                    double dx = (pt.X ?? 0.0) - (ptfrom.X ?? 0.0);
                    double dy = (pt.Y ?? 0.0) - (ptfrom.Y ?? 0.0);
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < maxdist)
                    {
                        maxdist = dist;
                        minDistLine = l;
                    }
                }

                list.Remove(minDistLine);
                newlist.Add(minDistLine);
            }
            return newlist;
        }

        private static HPGLLine GetHPGLLine(IList<HPGLCommand> list, ref int startidx)
        {
            var line = new HPGLLine();
            line.PreCommands = list.Skip(startidx).TakeWhile((e) => !e.IsPenDownCommand);
            startidx += line.PreCommands.Count();

            line.Commands = list.Skip(startidx).TakeWhile((e) => e.IsPenDownCommand);
            startidx += line.Commands.Count();

            line.PostCommands = list.Skip(startidx).TakeWhile((e) => false);    // always empty
            startidx += line.PostCommands.Count();
            return line;
        }

        #endregion

        #region Smooth 

        private IList<HPGLCommand> Smooth(IList<HPGLCommand> list)
        {
            var newlist = new List<HPGLCommand>();

            int startidx = 0;
            while (startidx < list.Count())
            {
                var nopenlist = list.Skip(startidx).TakeWhile((e) => !e.IsPenDownCommand);
                newlist.AddRange(nopenlist);
                startidx += nopenlist.Count();

                var line = list.Skip(startidx).TakeWhile((e) => e.IsPenDownCommand);
                startidx += line.Count();

                newlist.AddRange(SmoothLine(line));
            }

            CalculateAngles(newlist,null);
            return newlist;
        }

        int lineidx = 1;
        private IList<HPGLCommand> SmoothLine(IEnumerable<HPGLCommand> line)
        {
            if (_DEBUG)
                WriteLineToFile(line, lineidx++);

            var list = new List<HPGLCommand>();
            double maxAngle = LoadOptions.SmoothMinAngle.HasValue ? (double)LoadOptions.SmoothMinAngle.Value : (45 * (Math.PI / 180));

            int startidx = 0;
            while (startidx < line.Count())
            {
                // check for angle
                var linepart = line.Skip(startidx).TakeWhile((c) => Math.Abs(c.DiffLineAngleWithNext??(0.0)) < maxAngle);
                if (linepart.Count() > 0)
                {
                    startidx += linepart.Count();
                    list.AddRange(SplitLine(linepart));
                }
                else
                {
                    linepart = line.Skip(startidx).TakeWhile((c) => Math.Abs(c.DiffLineAngleWithNext??(0.0)) >= maxAngle);
                    startidx += linepart.Count();
                    list.AddRange(linepart);
                }
            }
            return list;
        }
        private IEnumerable<HPGLCommand> SplitLine(IEnumerable<HPGLCommand> line)
        {
            if (line.Count() < 2)
                return line;

            Point3D firstfrom = line.ElementAt(0).PointFrom;
            for (int i = 0; i < 100; i++)
            {
                var newline = SplitLineImpl(line);
                if (newline.Count() == line.Count())
                    return newline;
                line = newline;
                CalculateAngles(line, firstfrom);
            }
            return line;
        }

        private IEnumerable<HPGLCommand> SplitLineImpl(IEnumerable<HPGLCommand> line)
        {
            if (line.Count() < 3)
                return line;

            var newline = new List<HPGLCommand>();
            HPGLCommand prev=null;
            double minLineLenght = LoadOptions.SmoothMinLineLenght.HasValue ? (double) LoadOptions.SmoothMinLineLenght.Value : double.MaxValue;
            double maxerror = LoadOptions.SmoothMaxError.HasValue ? (double)LoadOptions.SmoothMaxError.Value : 1.0/40.0;
            minLineLenght /= (double) LoadOptions.ScaleX;
            maxerror /= (double)LoadOptions.ScaleX;

            foreach (var pt in line)
            {
                double x = (pt.PointTo.X??0.0) - (pt.PointFrom.X??0.0);
                double y = (pt.PointTo.Y??0.0) - (pt.PointFrom.Y??0.0);

                var c = Math.Sqrt(x * x + y * y);

                if (minLineLenght <= c)
                {
                    double alpha = pt.DiffLineAngleWithNext ?? (prev?.DiffLineAngleWithNext ?? 0.0);
                    double beta = prev != null ? (prev.DiffLineAngleWithNext ?? 0.0) : alpha;
                    double swapscale = 1.0;

                    if ((alpha >= 0.0 && beta >= 0.0) || (alpha <= 0.0 && beta <= 0.0))
                    {

                    }
                    else
                    {
                        beta = -beta;
                        swapscale = 0.5;
                    }
                    if ((alpha >= 0.0 && beta >= 0.0) || (alpha <= 0.0 && beta <= 0.0))
                    {
                        double gamma = Math.PI - alpha - beta;

                        double b = Math.Sin(beta) / Math.Sin(gamma) * c;
                        //double a = Math.Sin(alpha) / Math.Sin(gamma) * c;

                        double hc = b * Math.Sin(alpha) * swapscale;
                        double dc = Math.Sqrt(b * b - hc * hc);
                        double hc4 = hc / 4.0;

                        if (Math.Abs(hc4) > maxerror && Math.Abs(hc4) < c && Math.Abs(dc)< c )
                        {
                            newline.Add(GetNewCommand(pt, dc, hc4));
                        }
                    }
                }
                prev = pt;
                newline.Add(pt);
            }

            return newline;
        }

        /// <summary>
        /// Create a new point based on a existing line (x,y are based on the line vector)
        /// </summary>
        private HPGLCommand GetNewCommand(HPGLCommand pt, double x, double y)
        {
            double diffalpha = Math.Atan2(y, x);
            double linealpha = diffalpha + (pt.LineAngle ?? 0);

            double dx = x * Math.Cos(linealpha);
            double dy = x * Math.Sin(linealpha);

            return new HPGLCommand()
            {
                CommandType = pt.CommandType,
                PointTo = new Point3D() { X = pt.PointFrom.X + dx, Y = pt.PointFrom.Y + dy }
            };
        }

        #endregion

        #region Debug-Helpers

        private void WriteLineToFile(IEnumerable<HPGLCommand> list, int lineIdx)
        {
            if (list.Count() > 0)
            {
                var firstfrom = list.First().PointFrom;
                using (StreamWriter sw = new StreamWriter(Environment.ExpandEnvironmentVariables($"%TMP%\\CNCLib_Line{lineIdx}.plt")))
                {
                    sw.WriteLine($"PU {(int) (firstfrom.X.Value*40)},{(int) (firstfrom.Y.Value*40)}");
                    foreach (var cmd in list)
                    {
                        sw.WriteLine($"PD {(int) (cmd.PointTo.X.Value*40)},{(int) (cmd.PointTo.Y.Value*40)}");
                    }
                }
            }
        }

        #endregion

    }
}
