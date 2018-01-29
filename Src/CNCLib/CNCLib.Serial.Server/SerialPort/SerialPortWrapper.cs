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

using System;
using System.Collections.Generic;
using System.Linq;
using CNCLib.Serial.Server.Hubs;
using Framework.Tools.Dependency;
using Microsoft.AspNetCore.SignalR;

namespace CNCLib.Serial.Server.SerialPort
{
    public class SerialPortWrapper
    {
        #region ctr/SignalR

        public SerialPortWrapper(IHubContext<CNCLibHub> clients)
        {
            Clients = clients;
        }

        private IHubContext<CNCLibHub> clients;

        private IHubContext<CNCLibHub> Clients
        {
            get;
            set;
        }

        public void InitPort()
        {
            if (Serial == null)
            {
                Serial = new Framework.Arduino.SerialCommunication.Serial();
                Serial.CommandQueueEmpty += async (sender, e) => await Clients.Clients.All.InvokeAsync("queueEmpty");
            }
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        public string PortName { get; set; }

        public Framework.Arduino.SerialCommunication.Serial Serial { get; set; }

        public bool IsConnected => Serial != null ? Serial.IsConnected : false;

        public bool IsAborted => Serial != null ? Serial.Aborted : false;
        public bool IsSingleStep => Serial != null ? Serial.Pause : false;
        public int CommandsInQueue => Serial != null ? Serial.CommandsInQueue : 0;


        #endregion
    }
}