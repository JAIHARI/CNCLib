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
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Framework.Web
{
    public abstract class RestController<T> : ApiController
    {
        protected RestController(IRest<T> controller)
        {
            Controller = controller ?? throw new ArgumentNullException();
        }

        public IRest<T> Controller { get; }

        public async Task<IEnumerable<T>> Get()
		{
			return await Controller.Get();
		}

		// GET api/values/5
		//[ResponseType(T)]
		public async Task<IHttpActionResult> Get(int id)
		{
			T m = await Controller.Get(id);
			if (m == null)
			{
				return NotFound();
			}
			return Ok(m);
		}

		// POST api/values == Create
		//[ResponseType(typeof(T))]
		public async Task<IHttpActionResult> Post([FromBody]T value)
		{
			if (!ModelState.IsValid || value == null)
			{
				return BadRequest(ModelState);
			}
			try
			{
				int newid = await Controller.Add(value);
				return CreatedAtRoute("DefaultApi", new { id = newid }, await Controller.Get(newid));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// PUT api/values/5
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> Put(int id, [FromBody]T value)
		{
			if (!ModelState.IsValid || value == null)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (Controller.CompareId(id,value) == false)
				{
					return BadRequest("Missmatch between id and machineID");
				}

				await Controller.Update(id, value);
				return StatusCode(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// DELETE api/values/5
		//[ResponseType(typeof(T))]
		public async Task<IHttpActionResult> Delete(int id)
		{
			T value = await Controller.Get(id);
			if (value == null)
			{
				return NotFound();
			}

			await Controller.Delete(id, value);
			return Ok(value);
		}
	}
}
