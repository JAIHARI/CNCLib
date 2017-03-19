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

using CNCLib.Logic.Contracts.DTO;
using Framework.Tools.Dependency;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Framework.Web;
using CNCLib.ServiceProxy;
using System.Threading.Tasks;

namespace CNCLib.WebAPI.Controllers
{
    public class MachineController : RestController<Machine>
	{
		[Route("api/Machine/default")]
		[HttpGet] //Always explicitly state the accepted HTTP method
		public async Task<IHttpActionResult> DefaultMachine()
		{
			using (IMachineService service = Dependency.Resolve<IMachineService>())
			{
				var m = await service.DefaultMachine();
				if (m == null)
				{
					return NotFound();
				}
				return Ok(m);
			}
		}

		[Route("api/Machine/defaultmachine")]
		[HttpGet] //Always explicitly state the accepted HTTP method
		public async Task<IHttpActionResult> GetDetaultMachine()
		{
			using (IMachineService service = Dependency.Resolve<IMachineService>())
			{
				int id = await service.GetDetaultMachine();
				return Ok(id);
			}
		}

		[Route("api/Machine/defaultmachine")]
		[HttpPut] //Always explicitly state the accepted HTTP method
		public async Task<IHttpActionResult> SetDetaultMachine(int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				using (IMachineService service = Dependency.Resolve<IMachineService>())
				{
					await service.SetDetaultMachine(id);
				}
				return StatusCode(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

	}

	public class MachineRest : IRest<Machine>
	{
		private IMachineService _service = Dependency.Resolve<IMachineService>();

		public async Task<IEnumerable<Machine>> Get()
		{
			return await _service.GetAll();
		}

		public async Task<Machine> Get(int id)
		{
			if (id == -1)
				return await _service.DefaultMachine();

			return await _service.Get(id);
		}

		public async Task<int> Add(Machine value)
		{
			return await _service.Add(value);
		}

		public async Task Update(int id, Machine value)
		{
			await _service.Update(value);
		}

		public async Task Delete(int id, Machine value)
		{
			await _service.Delete(value);
		}

		public bool CompareId(int id, Machine value)
		{
			return id == value.MachineID;
		}

		#region IDisposable Support
		private bool _disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_service.Dispose();
					_service = null;
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