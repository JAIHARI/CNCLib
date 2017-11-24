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
using System.Windows.Input;
using Framework.Wpf.Helpers;
using CNCLib.Wpf.Helpers;

namespace CNCLib.Wpf.ViewModels.ManualControl
{
	public class ToolViewModel : DetailViewModel, IDisposable
	{
		#region ctr

		public ToolViewModel(IManualControlViewModel vm)
			: base(vm)
		{
			Com.CommandQueueChanged += OnCommandQueueChanged;
		}

		public void Dispose()
		{
			Com.CommandQueueChanged -= OnCommandQueueChanged;
		}

		#endregion

		#region Properties

		public int PendingCommandCount => Com.CommandsInQueue;

	    public bool Pause
		{
			get => Com.Pause;
	        set => Com.Pause = value;
	    }

		private bool _updateAfterSendNext = false;

		private void OnCommandQueueChanged(object sender, Framework.Arduino.SerialCommunication.SerialEventArgs arg)
		{
			RaisePropertyChanged(nameof(PendingCommandCount));

			if (_updateAfterSendNext)
			{
				_updateAfterSendNext = false;
				((ManualControlViewModel)Vm).CommandHistory.RefreshAfterCommand();
			}
		}

		private void SetSendNext()
		{
			_updateAfterSendNext = true;
			Com.SendNext = true; 
		}

		#endregion

		#region Commands / CanCommands
		public bool CanSendSpindle()
		{
			return CanSendPlotter() && Global.Instance.Machine.Spindle;
		}
		public bool CanSendCoolant()
		{
			return CanSendPlotter() && Global.Instance.Machine.Coolant;
		}
		public bool CanSendLaser()
		{
			return CanSendPlotter() && Global.Instance.Machine.Laser;
		}

		public void SendInfo() { RunAndUpdate(() => { Com.QueueCommand("?"); }); }
		public void SendDebug() { RunAndUpdate(() => { Com.QueueCommand("&"); }); }
		public void SendAbort() { RunAndUpdate(() => { Com.AbortCommands(); Com.ResumeAfterAbort(); Com.QueueCommand("!"); }); }
		public void SendResurrect() { RunAndUpdate(() => { Com.AbortCommands(); Com.ResumeAfterAbort(); Com.QueueCommand("!!!"); }); }
		public void ClearQueue() { RunAndUpdate(() => { Com.AbortCommands(); Com.ResumeAfterAbort(); }); }
		public void SendM03SpindleOn() { RunAndUpdate(() => { Com.QueueCommand("m3"); }); }
		public void SendM05SpindleOff() { RunAndUpdate(() => { Com.QueueCommand("m5"); }); }
		public void SendM07CoolandOn() { RunAndUpdate(() => { Com.QueueCommand("m7"); }); }
		public void SendM09CoolandOff() { RunAndUpdate(() => { Com.QueueCommand("m9"); }); }
		public void SendM106LaserOn() { RunAndUpdate(() => { Com.QueueCommand("m106 s255"); }); }
		public void SendM106LaserOnMin() { RunAndUpdate(() => { Com.QueueCommand("m106 s1"); }); }
		public void SendM107LaserOff() { RunAndUpdate(() => { Com.QueueCommand("m107"); }); }
        public void SendM100ProbeDefault() { RunAndUpdate(() => { Com.QueueCommand("m100"); }); }
        public void SendM101ProbeInvert() { RunAndUpdate(() => { Com.QueueCommand("m101"); }); }
        public void SendM114PrintPos()
		{
			RunAndUpdate(async () =>
			{
				string message = await Com.SendCommandAndReadOKReplyAsync(MachineGCodeHelper.PrepareCommand("m114"));

				if (!string.IsNullOrEmpty(message))
				{
					message = message.Replace("ok", "");
					message = message.Replace(" ", "");
					SetPositions(message.Split(':'), 0);
				}

				message = await Com.SendCommandAndReadOKReplyAsync(MachineGCodeHelper.PrepareCommand("m114 s1"));

				if (!string.IsNullOrEmpty(message))
				{
					message = message.Replace("ok", "");
					message = message.Replace(" ", "");
					SetPositions(message.Split(':'), 1);
				}
			});
		}
		public void WritePending() { RunInNewTask(() => { Com.WritePendingCommandsToFile(System.IO.Path.GetTempPath() + "PendingCommands.nc"); }); }

		#endregion

		#region ICommand
		public ICommand SendInfoCommand => new DelegateCommand(SendInfo, CanSend);
		public ICommand SendDebugCommand => new DelegateCommand(SendDebug, CanSend);
		public ICommand SendAbortCommand => new DelegateCommand(SendAbort, CanSend);
		public ICommand SendResurrectCommand => new DelegateCommand(SendResurrect, CanSend);
		public ICommand SendClearQueue => new DelegateCommand(ClearQueue, CanSend);
		public ICommand SendM03SpindleOnCommand => new DelegateCommand(SendM03SpindleOn, CanSendSpindle);
		public ICommand SendM05SpindleOffCommand => new DelegateCommand(SendM05SpindleOff, CanSendSpindle);
		public ICommand SendM07CoolandOnCommand => new DelegateCommand(SendM07CoolandOn, CanSendCoolant);
		public ICommand SendM09CoolandOffCommand => new DelegateCommand(SendM09CoolandOff, CanSendCoolant);
        public ICommand SendM100ProbeDefaultCommand => new DelegateCommand(SendM100ProbeDefault, CanSend);
        public ICommand SendM101ProbeInvertCommand => new DelegateCommand(SendM101ProbeInvert, CanSend);
        public ICommand SendM106LaserOnCommand => new DelegateCommand(SendM106LaserOn, CanSendLaser);
		public ICommand SendM106LaserOnMinCommand => new DelegateCommand(SendM106LaserOnMin, CanSendLaser);
		public ICommand SendM107LaserOffCommand => new DelegateCommand(SendM107LaserOff, CanSendLaser);
		public ICommand SendM114Command => new DelegateCommand(SendM114PrintPos, CanSend);
		public ICommand WritePendingCommands => new DelegateCommand(WritePending, CanSend);
		public ICommand SendNextCommands => new DelegateCommand(SetSendNext, () => CanSend() && Pause && PendingCommandCount > 0);

		#endregion
	}
}
