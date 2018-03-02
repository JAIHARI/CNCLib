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

using System.Collections.Generic;

namespace CNCLib.Logic.Contracts.DTO
{
    public enum CommandSyntax
    {
        GCodeBasic = 0,
        GCode = 1,
        HPGL = 7           // max 3 bit
    }

    public class Machine
	{
		public int MachineID { get; set; }
        public string SerialServer { get; set; }
        public int SerialServerPort { get; set; }
        public string ComPort { get; set; }
        public int BaudRate { get; set; }
        public bool DtrIsReset { get; set; }
        public int Axis { get; set; }
		public string Name { get; set; }
		public decimal SizeX { get; set; }
		public decimal SizeY { get; set; }
		public decimal SizeZ { get; set; }
		public decimal SizeA { get; set; }
		public decimal SizeB { get; set; }
		public decimal SizeC { get; set; }
		public int BufferSize { get; set; }
		public bool CommandToUpper { get; set; }
		public decimal ProbeSizeX { get; set; }
		public decimal ProbeSizeY { get; set; }
		public decimal ProbeSizeZ { get; set; }
		public decimal ProbeDistUp { get; set; }
		public decimal ProbeDist { get; set; }
		public decimal ProbeFeed { get; set; }
		public bool SDSupport { get; set; }
		public bool Spindle { get; set; }
		public bool Coolant { get; set; }
        public bool Laser { get; set; }
        public bool Rotate { get; set; }

		public CommandSyntax CommandSyntax { get; set; }

		public IEnumerable<MachineCommand> MachineCommands { get; set; }
		public IEnumerable<MachineInitCommand> MachineInitCommands { get; set; }

	}
}
