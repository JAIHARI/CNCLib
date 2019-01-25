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

namespace CNCLib.Repository.Contract.Entities
{
    public class MachineInitCommand
    {
        public         int     MachineInitCommandId { get; set; }
        public         int     SeqNo                { get; set; }
        public         string  CommandString        { get; set; }
        public         int     MachineId            { get; set; }
        public virtual Machine Machine              { get; set; }
    }
}