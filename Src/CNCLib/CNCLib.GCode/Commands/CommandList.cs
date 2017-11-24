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

using System.Collections.Generic;

namespace CNCLib.GCode.Commands
{
    public class CommandList : List<Command>
	{
		#region Properties

		public Command Current { get; set; }

		#endregion

		#region Add/Update

		public void AddCommand(Command cmd)
		{
			if (Count > 0)
			{
				this[Count - 1].NextCommand = cmd;
				cmd.PrevCommand = this[Count - 1];
			}
			cmd.NextCommand = null;
			base.Add(cmd);
		}

		public new void Add(Command cmd)
		{
			AddCommand(cmd);
		}
		public void AddCommands(IEnumerable<Command> cmds)
		{
			foreach (var cmd in cmds)
			{
				AddCommand(cmd);
			}
		}

		public void UpdateCache()
		{
			foreach (Command cmd in this)
			{
				cmd.UpdateCalculatedEndPosition();
			}
		}

		#endregion

		#region Paint + Convert

		public void Paint(IOutputCommand output, object param)
		{
			var commandstate = new CommandState();
			bool haveseencurrent = Current == null;

			foreach (Command cmd in this)
			{
				if (!haveseencurrent)
					haveseencurrent = cmd == Current;
				commandstate.IsSelected = haveseencurrent;
				cmd.Draw(output, commandstate, param);
			}
		}

		public IEnumerable<string> ToStringList()
		{
			var list = new List<string>();

			Command last = null;
			CommandState state = new CommandState();

			foreach (Command r in this)
			{
				string[] cmds = r.GetGCodeCommands(last != null ? last.CalculatedEndPosition : null, state);
				if (cmds != null)
				{
					foreach (string str in cmds)
					{
						list.Add(str);
					}
				}
				last = r;
			}
			return list;
		}

		public CommandList Convert(ConvertOptions options)
		{
			var list = new CommandList();
			CommandState state = new CommandState();

			foreach (Command r in this)
			{
				list.AddCommands(r.ConvertCommand(state, options));
			}

			return list;

			#endregion
		}
	}
}
