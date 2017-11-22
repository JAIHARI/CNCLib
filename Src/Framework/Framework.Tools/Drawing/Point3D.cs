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

namespace Framework.Tools.Drawing
{
	public class Point3D
	{
		public Point3D(double x, double y, double z)
		{
			X = x; Y = y; Z = z;
		}
		public Point3D()
		{
			//X = new decimal?();
			//Y = new decimal?();
			//Z = new decimal?();
		}
		public double? X { get; set; }
		public double? Y { get; set; }
		public double? Z { get; set; }

        public static implicit operator Point2D (Point3D pt)
        {
            return new Point2D() { X = pt.X ?? 0.0, Y = pt.Y ?? 0.0 };
        }

		public double? this[int axis]
		{
			get
			{
				if (axis == 0) return X;
				if (axis == 1) return Y;
				if (axis == 2) return Z;
				throw new ArgumentOutOfRangeException();
			}
			set
			{
				if (axis == 0) X = value;
				else if (axis == 1) Y = value;
				else if (axis == 2) Z = value;
				else throw new ArgumentOutOfRangeException();
			}
		}

        public bool Compare2D(Point3D to)
        {
            return (X ?? 0.0) == (to.X ?? 0.0) && (Y ?? 0.0) == (to.Y ?? 0.0);
        }

		public bool HasAllValues => X.HasValue && Y.HasValue && Z.HasValue;

	    public void AssignMissing(Point3D from)
		{
			if (!X.HasValue && from.X.HasValue) X = from.X;
			if (!Y.HasValue && from.Y.HasValue) Y = from.Y;
			if (!Z.HasValue && from.Z.HasValue) Z = from.Z;
		}

		public static implicit operator System.Drawing.Point(Point3D sc)
		{
			return new System.Drawing.Point((int) (sc.X??0.0), (int) (sc.Y??0.0));
		}

		public void Offset(Point3D p)
		{
			if (X.HasValue && p.X.HasValue) X += p.X;
			if (Y.HasValue && p.Y.HasValue) Y += p.Y;
			if (Z.HasValue && p.Z.HasValue) Z += p.Z;
		}
	}
}
