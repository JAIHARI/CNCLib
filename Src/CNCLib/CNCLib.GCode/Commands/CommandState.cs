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

namespace CNCLib.GCode.Commands
{
	public class CommandState
	{
		public bool UseLaser { get; set; } = false;
		public bool LaserOn { get; set; } = false;
		public bool SpindleOn { get; set; } = false;
		public bool CoolantOn { get; set; } = false;
		public Pane CurrentPane { get; set; } = Pane.XYPane;

		public bool IsSelected { get; set; } = true;

		public Command.Variable G82_R { get; set; }
		public Command.Variable G82_P { get; set; }
		public Command.Variable G82_Z { get; set; }
	}
}
