﻿/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CNCLib.Serial.Shared;

using Framework.Arduino.SerialCommunication;
using Framework.Arduino.SerialCommunication.Abstraction;

using Microsoft.AspNetCore.SignalR.Client;

namespace CNCLib.Serial.Client
{
    public class SerialService : ServiceBase, ISerial
    {
        protected readonly string           _api = @"api/SerialPort";
        private            SerialServiceHub _serviceHub;

        private async Task InitServiceHub()
        {
            _serviceHub = new SerialServiceHub(WebServerUri, this);
            var connection = await _serviceHub.Start();

            connection.On(
                "queueEmpty",
                (int id) =>
                {
                    if (PortId == id)
                    {
                        CommandQueueEmpty?.Invoke(this, new SerialEventArgs());
                    }
                });
            connection.On(
                "queueChanged",
                (int id, int queueLength) =>
                {
                    if (PortId == id)
                    {
                        CommandQueueChanged?.Invoke(this, new SerialEventArgs(queueLength, null));
                    }
                });
            connection.On(
                "sendingCommand",
                (int id, int seqId) =>
                {
                    if (PortId == id)
                    {
                        CommandSending?.Invoke(this, new SerialEventArgs(new SerialCommand() { SeqId = seqId }));
                    }
                });
        }

        public int PortId { get; private set; } = -1;

        private async Task<SerialPortDefinition> GetSerialPortDefinition(HttpClient client, string portName)
        {
            // first ge all ports
            HttpResponseMessage responseAll = client.GetAsync($@"{_api}").GetAwaiter().GetResult();
            if (responseAll.IsSuccessStatusCode)
            {
                IEnumerable<SerialPortDefinition> allPorts = await responseAll.Content.ReadAsAsync<IEnumerable<SerialPortDefinition>>();
                return allPorts.FirstOrDefault((p) => 0 == string.Compare(p.PortName, portName, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        private async Task<SerialPortDefinition> RefreshAndGetSerialPortDefinition(HttpClient client, string portName)
        {
            HttpResponseMessage responseAll = client.PostAsJsonAsync($@"{_api}/refresh", "dummy").GetAwaiter().GetResult();
            if (responseAll.IsSuccessStatusCode)
            {
                IEnumerable<SerialPortDefinition> allPorts = await responseAll.Content.ReadAsAsync<IEnumerable<SerialPortDefinition>>();
                return allPorts.FirstOrDefault((p) => 0 == string.Compare(p.PortName, portName, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        public async Task ConnectAsync(string portName)
        {
            if (WaitForSend != null || CommandSent != null || WaitCommandSent != null || ReplyReceived != null || ReplyOK != null || ReplyError != null || ReplyInfo != null || ReplyUnknown != null)
            {
                // dummy do not get "unused"                
            }

            // linuxPort => http://a0:5000/dev/ttyUSB0
            // win       => http://a0:5000/com4

            int lastColon = portName.LastIndexOf(':');
            int lastSlash = portName.IndexOf('/', lastColon);
            if (lastSlash > 0)
            {
                WebServerUri = portName.Substring(0, lastSlash);
                portName     = portName.Substring(lastSlash + 1);
                if (portName.IndexOf('/') >= 0)
                {
                    portName = "/" + portName;
                }

                using (HttpClient client = CreateHttpClient())
                {
                    var port = await GetSerialPortDefinition(client, portName) ?? await RefreshAndGetSerialPortDefinition(client, portName);

                    if (port != null)
                    {
                        HttpResponseMessage response = await client.PostAsJsonAsync($@"{_api}/{port.Id}/connect?baudRate={BaudRate}&dtrIsReset={DtrIsReset}&resetOnConnect={ResetOnConnect}", "x");
                        if (response.IsSuccessStatusCode)
                        {
                            var value = await response.Content.ReadAsAsync<SerialPortDefinition>();
                            IsConnected = true;
                            PortId      = port.Id;

                            await InitServiceHub();
                            return;
                        }
                    }
                }
            }

            throw new Exception("Connect to SerialPort failed");
        }

        public async Task DisconnectAsync()
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync($@"{_api}/{PortId}/disconnect", "x");
                    if (response.IsSuccessStatusCode)
                    {
                        _serviceHub?.Stop();
                        _serviceHub = null;
                        IsConnected = false;
                        PortId      = -1;
                        return;
                    }
                }
            }

            throw new Exception("DisConnect to SerialPort failed");
        }

        public void AbortCommands()
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    HttpResponseMessage response = client.PostAsJsonAsync($@"{_api}/{PortId}/abort", "x").ConfigureAwait(false).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
            }

            throw new Exception("AbortCommands to SerialPort failed");
        }

        public void ResumeAfterAbort()
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    HttpResponseMessage response = client.PostAsJsonAsync($@"{_api}/{PortId}/resume", "x").ConfigureAwait(false).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
            }

            throw new Exception("ResumeAfterAbort to SerialPort failed");
        }

        public async Task<IEnumerable<SerialCommand>> QueueCommandsAsync(IEnumerable<string> lines)
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    var                 cmds     = new SerialCommands() { Commands = lines.ToArray() };
                    HttpResponseMessage response = client.PostAsJsonAsync($@"{_api}/{PortId}/queue", cmds).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var value = await response.Content.ReadAsAsync<IEnumerable<SerialCommand>>();
                        return value;
                    }
                }
            }

            throw new Exception("Queue to SerialPort failed");
        }

        public async Task<IEnumerable<SerialCommand>> SendCommandsAsync(IEnumerable<string> lines, int waitForMilliseconds)
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    client.Timeout = new TimeSpan(10000L * (((long)waitForMilliseconds) + 5000));
                    var                 cmds     = new SerialCommands() { Commands = lines.ToArray(), TimeOut = waitForMilliseconds };
                    HttpResponseMessage response = await client.PostAsJsonAsync($@"{_api}/{PortId}/send", cmds);
                    if (response.IsSuccessStatusCode)
                    {
                        var value = await response.Content.ReadAsAsync<IEnumerable<SerialCommand>>();
                        return value;
                    }
                }
            }

            throw new Exception("Send to SerialPort failed");
        }

        public Task<bool> WaitUntilResponseAsync(int maxMilliseconds)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WaitUntilQueueEmptyAsync(int maxMilliseconds)
        {
            throw new NotImplementedException();
        }

        Task<string> ISerial.WaitUntilResponseAsync(int maxMilliseconds)
        {
            throw new NotImplementedException();
        }

        public event CommandEventHandler WaitForSend;
        public event CommandEventHandler CommandSending;
        public event CommandEventHandler CommandSent;
        public event CommandEventHandler WaitCommandSent;
        public event CommandEventHandler ReplyReceived;
        public event CommandEventHandler ReplyOK;
        public event CommandEventHandler ReplyError;
        public event CommandEventHandler ReplyInfo;
        public event CommandEventHandler ReplyUnknown;
        public event CommandEventHandler CommandQueueChanged;
        public event CommandEventHandler CommandQueueEmpty;
        public bool                      IsConnected            { get; private set; }
        public int                       CommandsInQueue        { get; }
        public bool                      Pause                  { get; set; }
        public bool                      SendNext               { get; set; }
        public int                       BaudRate               { get; set; }
        public bool                      DtrIsReset             { get; set; }
        public bool                      ResetOnConnect         { get; set; }
        public string                    OkTag                  { get; set; }
        public string                    ErrorTag               { get; set; }
        public string                    InfoTag                { get; set; }
        public bool                      CommandToUpper         { get; set; }
        public bool                      ErrorIsReply           { get; set; }
        public int                       MaxCommandHistoryCount { get; set; }
        public int                       ArduinoBufferSize      { get; set; }
        public int                       ArduinoLineSize        { get; set; }
        public bool                      Aborted                { get; }

        public SerialCommand LastCommand { get; }

        public void WriteCommandHistory(string filename)
        {
            throw new NotImplementedException();
        }

        public List<SerialCommand> CommandHistoryCopy
        {
            get
            {
                if (PortId >= 0)
                {
                    using (HttpClient client = CreateHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync($@"{_api}/{PortId}/history").ConfigureAwait(false).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            var value = response.Content.ReadAsAsync<List<SerialCommand>>().Result;
                            return value;
                        }
                    }
                }

                throw new Exception("ClearCommandHistory to SerialPort failed");
            }
        }

        public IEnumerable<SerialCommand> PendingCommands => throw new NotImplementedException();

        public void ClearCommandHistory()
        {
            if (PortId >= 0)
            {
                using (HttpClient client = CreateHttpClient())
                {
                    HttpResponseMessage response = client.PostAsJsonAsync($@"{_api}/{PortId}/history/clear", "x").ConfigureAwait(false).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
            }

            throw new Exception("ClearCommandHistory to SerialPort failed");
        }
    }
}