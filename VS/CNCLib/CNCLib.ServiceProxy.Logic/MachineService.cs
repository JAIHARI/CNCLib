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
using CNCLib.Logic.Contracts.DTO;
using CNCLib.Logic.Contracts;
using Framework.Tools.Dependency;
using System.Threading.Tasks;

namespace CNCLib.ServiceProxy.Logic
{
    public class MachineService : IMachineService
	{
		private IMachineController _controller = Dependency.Resolve<IMachineController>();

		public async Task<int> Add(Machine value)
		{
			return await _controller.Add(value);
		}

		public async Task<Machine> DefaultMachine()
		{
			return await _controller.DefaultMachine();
		}

		public async Task Delete(Machine value)
		{
			await _controller.Delete(value);
		}

		public async Task<Machine> Get(int id)
		{
			return await _controller.Get(id);
		}

		public async Task<IEnumerable<Machine>> GetAll()
		{
			return await _controller.GetAll();
		}

		public async Task<int> GetDetaultMachine()
		{
			return await _controller.GetDetaultMachine();
		}

		public async Task SetDetaultMachine(int defaultMachineID)
		{
			await _controller.SetDetaultMachine(defaultMachineID);
		}

		public async Task<int> Update(Machine value)
		{
			return await _controller.Update(value);
		}

		#region IDisposable Support
		private bool _disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_controller.Dispose();
					_controller = null;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~MachineRest() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

	}
}