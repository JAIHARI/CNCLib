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
using System.Linq;
using Framework.Tools.Pattern;

namespace Framework.EF
{
    public class RepositoryBase
	{
        public RepositoryBase(IUnitOfWork uow)
        {
            Uow = uow ?? throw new ArgumentNullException();
        }

        public IUnitOfWork Uow { get; set; }

		public void Sync<T>(ICollection<T> inDb, ICollection<T> toDb, Func<T, T, bool> predicate) 
		{
			// 1. Delete from DB (in DB) and update
			var delete = new List<T>();

			foreach (T entityInDb in inDb)
			{
                var entityToDb = toDb.FirstOrDefault(x => predicate(x, entityInDb));
				if (entityToDb != null && predicate(entityToDb, entityInDb))
				{
					Uow.SetValue(entityInDb,entityToDb);
				}
				else
				{
					delete.Add(entityInDb);
				}
			}

			foreach (var del in delete)
			{
				Uow.MarkDeleted(del);
			}

			// 2. Add To DB

			foreach (T entityToDb in toDb)
			{
				var entityInDb = inDb.FirstOrDefault(x => predicate(x, entityToDb));
				if (entityInDb == null || predicate(entityToDb, entityInDb) == false)
				{
					Uow.MarkNew(entityToDb);
				}
			}
		}
	}
}
