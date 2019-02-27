﻿/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2019 Herbert Aitenbichler

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

namespace Framework.Pattern
{
    using System.Linq;
    using System.Reflection;

    public class Singleton<T> where T : new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }

        public static bool Allocated => _instance != null;

        public static void Free()
        {
            if (_instance != null)
            {
                foreach (MethodInfo mi in typeof(T).GetMethods())
                {
                    if (mi.Name == "Dispose" && !mi.GetParameters().Any())
                    {
                        mi.Invoke(_instance, new object[0]);
                    }
                }

                _instance = default(T);
            }
        }
    }
}