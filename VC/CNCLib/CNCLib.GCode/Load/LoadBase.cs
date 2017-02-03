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

using CNCLib.GCode.Commands;
using System;
using System.Globalization;
using CNCLib.Logic.Contracts.DTO;
using Framework.Tools.Drawing;

namespace CNCLib.GCode.Load
{
	public abstract class LoadBase
	{
		bool _laserWasOn = false;
		bool _laserOn = true;
		CamBam.CamBam _cambam = new CamBam.CamBam();

		#region Properties

		public Commands.CommandFactory CommandFactory { get; private set; } = new Commands.CommandFactory();

		public CommandList Commands { get; protected set; } = new CommandList();

		public LoadOptions LoadOptions { get; set; }

		#endregion

		#region CamBam

		public CamBam.CamBam CamBam { get { return _cambam; } }

		private CamBam.CamBam.PLine _pline;
		private CamBam.CamBam.Layer _layer;

		protected void AddCamBamPoint(Point3D pt)
		{
			_pline.Pts.Add(new CamBam.CamBam.PLinePoints()
			{
				X = pt.X,
				Y = pt.Y,
				Z = pt.Z
			});
		}

		protected void AddCamBamPLine()
		{
			_pline = _layer.AddPLine();
		}

		protected void FinishCamBamPLine()
		{
			if (_pline != null)
			{
				_pline.CheckAndSetClosed();
				_pline = null;
			}
		}

		#endregion

		#region Factory

		static public LoadBase Create(LoadOptions info)
		{
			LoadBase load = null;
			switch (info.LoadType)
			{
				case LoadOptions.ELoadType.GCode: load = new LoadGCode(); break;
				case LoadOptions.ELoadType.HPGL: load = new LoadHPGL(); break;
				case LoadOptions.ELoadType.Image: load = new LoadImage(); break;
				case LoadOptions.ELoadType.ImageHole: load = new LoadImageHole(); break;
				default: return null;
			}
			load.LoadOptions = info;

			return load;
		}

		#endregion

		#region Load

		public abstract void Load();

		protected void PreLoad()
		{
			AddComment("Generated by CNCLib");
			AddComment("at " + DateTime.Now.ToString());

			AddComment("File", LoadOptions.FileName);

			if (!string.IsNullOrEmpty(LoadOptions.StartupCommands))
				AddCommands(LoadOptions.StartupCommands);


			_layer = CamBam.AddLayer();
			_layer.Name = @"Standard";
			_layer.Color = @"127,255,0";
		}

		protected void PostLoad()
		{
			if (LoadOptions != null && !string.IsNullOrEmpty(LoadOptions.ShutdownCommands))
				AddCommands(LoadOptions.ShutdownCommands);

			Commands.UpdateCache();

			if (LoadOptions != null)
			{
				var options = new ConvertOptions() { SubstG82 = LoadOptions.SubstG82 };

				Commands = Commands.Convert(options);
				Commands.UpdateCache();
			}
		}

		#endregion

		#region C-Code Comments

		protected void AddCommands(string commandstring)
		{
			var cmds = commandstring.Split(new string[] { @"\n", @"\r" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var s in cmds)
			{
				var r = CommandFactory.CreateOrDefault(s);
				Commands.Add(r);
			}
		}
		protected decimal ToGCode(double val)
		{
			return (decimal)Math.Round(val, 2);
		}

		protected void AddComment(string propertyName)
		{
			Commands.Add(new GxxCommand() { GCodeAdd = "; " + propertyName });
		}

		protected void AddComment(string propertyName, string propertyvalue)
		{
			Commands.Add(new GxxCommand() { GCodeAdd = "; " + propertyName + " = " + propertyvalue });
		}
		protected void AddComment(string propertyName, decimal propertyvalue)
		{
			AddComment(propertyName, propertyvalue.ToString(CultureInfo.InvariantCulture));
		}
		protected void AddComment(string propertyName, decimal? propertyvalue)
		{
			if (propertyvalue.HasValue)
				AddComment(propertyName, propertyvalue.Value.ToString(CultureInfo.InvariantCulture));
			else
				AddComment(propertyName, "");
		}
		protected void AddComment(string propertyName, double propertyvalue)
		{
			AddComment(propertyName, propertyvalue.ToString(CultureInfo.InvariantCulture));
		}
		protected void AddComment(string propertyName, byte propertyvalue)
		{
			AddComment(propertyName, propertyvalue.ToString());
		}
		protected void AddComment(string propertyName, int propertyvalue)
		{
			AddComment(propertyName, propertyvalue.ToString());
		}

		protected void AddCommentForLaser()
		{
			AddComment("LaserSize", LoadOptions.LaserSize);
			AddComment("LaserOnCommand", LoadOptions.LaserOnCommand);
			AddComment("LaserFirstOnCommand", LoadOptions.LaserFirstOnCommand);
			AddComment("LaserOffCommand", LoadOptions.LaserOffCommand);

			AddComment("Speed", LoadOptions.MoveSpeed);

			AddComment("LaserAccDist", LoadOptions.LaserAccDist);
		}

		#endregion

		#region Laser

		protected bool HaveLaserOnOffCommand()
		{
			return string.IsNullOrEmpty(LoadOptions.LaserOnCommand) == false || string.IsNullOrEmpty(LoadOptions.LaserOffCommand) == false;
		}

		protected void LaserOn()
		{
			if (_laserOn == false)
			{
				if (_laserWasOn || string.IsNullOrEmpty(LoadOptions.LaserFirstOnCommand))
				{
					if (string.IsNullOrEmpty(LoadOptions.LaserOnCommand) == false)
						AddCommands(LoadOptions.LaserOnCommand);
				}
				else
				{
					AddCommands(LoadOptions.LaserFirstOnCommand);
				}

				_laserWasOn = true;
				_laserOn = true;
			}
		}

		protected void LaserOff()
		{
			if (_laserOn)
			{
				ForceLaserOff();
			}
		}
		protected void ForceLaserOff()
		{
			if (string.IsNullOrEmpty(LoadOptions.LaserOffCommand) == false)
				AddCommands(LoadOptions.LaserOffCommand);
			_laserOn = false;
		}

		#endregion
	}
}

