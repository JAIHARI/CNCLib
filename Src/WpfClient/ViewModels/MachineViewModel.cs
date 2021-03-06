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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using AutoMapper;

using CNCLib.Service.Abstraction;
using CNCLib.WpfClient.Helpers;
using CNCLib.WpfClient.Models;

using Framework.Arduino.SerialCommunication;
using Framework.Pattern;
using Framework.Wpf.Helpers;
using Framework.Wpf.ViewModels;

using MachineDto = CNCLib.Logic.Abstraction.DTO.Machine;

namespace CNCLib.WpfClient.ViewModels
{
    public class MachineViewModel : BaseViewModel
    {
        #region crt

        public MachineViewModel(IFactory<IMachineService> machineService, IMapper mapper, Global global)
        {
            _machineService = machineService ?? throw new ArgumentNullException();
            _mapper         = mapper ?? throw new ArgumentNullException();
            _global         = global ?? throw new ArgumentNullException();
            ;
            AddNewMachine = false;
        }

        readonly         IFactory<IMachineService> _machineService;
        private readonly IMapper                   _mapper;
        private readonly Global                    _global;

        #endregion

        #region Properties

        Machine _currentMachine = new Machine();

        public Machine Machine
        {
            get => _currentMachine;
            set { SetProperty(() => _currentMachine == value, () => _currentMachine = value); }
        }

        public ObservableCollection<MachineCommand> MachineCommands => _currentMachine.MachineCommands;

        public ObservableCollection<MachineInitCommand> MachineInitCommands => _currentMachine.MachineInitCommands;

        public bool AddNewMachine { get; set; }

        #endregion

        #region Operations

        public async Task LoadMachine(int machineId)
        {
            using (var scope = _machineService.Create())
            {
                await MyLoadMachine(machineId, scope);
            }
        }

        private async Task MyLoadMachine(int machineId, IScope<IMachineService> scope)
        {
            MachineDto dto;
            AddNewMachine = machineId <= 0;
            if (AddNewMachine)
            {
                dto = await scope.Instance.DefaultMachine();
            }
            else
            {
                dto = await scope.Instance.Get(machineId);
            }

            Machine = dto.Convert(_mapper);

            RaisePropertyChanged(nameof(Machine));

            RaisePropertyChanged(nameof(MachineCommands));
            RaisePropertyChanged(nameof(MachineInitCommands));
        }

        public async void SaveMachine()
        {
            var m = _currentMachine.Convert(_mapper);

            using (var scope = _machineService.Create())
            {
                int id = m.MachineId;
                if (id == default(int))
                {
                    id = await scope.Instance.Add(m);
                }
                else
                {
                    await scope.Instance.Update(m);
                }

                await MyLoadMachine(id, scope);
            }

            CloseAction();
        }

        public bool CanSaveMachine()
        {
            return true;
        }

        public async void DeleteMachine()
        {
            using (var scope = _machineService.Create())
            {
                await scope.Instance.Delete(_currentMachine.Convert(_mapper));
            }

            CloseAction();
        }

        public bool CanDeleteMachine()
        {
            return !AddNewMachine;
        }

        public async void AddMachine()
        {
            using (var scope = _machineService.Create())
            {
                await MyLoadMachine(-1, scope);
            }
        }

        public bool CanAddMachine()
        {
            return !AddNewMachine;
        }

        public async void ReadFromMachine()
        {
            if (MessageBox?.Invoke("Read configuration from machine?", "CNCLib", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    string comport = Machine.GetComPort();

                    _global.Com.SetCurrent(comport);
                    _global.Com.Current.DtrIsReset     = Machine.DtrIsReset;
                    _global.Com.Current.ResetOnConnect = _global.ResetOnConnect;
                    _global.Com.Current.CommandToUpper = Machine.CommandToUpper;
                    _global.Com.Current.BaudRate       = Machine.BaudRate;
                    await _global.Com.Current.ConnectAsync(comport);

                    await _global.Com.Current.SendCommandAsync("?", 3000);
                    await Task.Delay(100);

                    var eeprom = await _global.Com.Current.ReadEepromAsync();
                    if (eeprom != null)
                    {
                        Machine.Coolant   = eeprom.HasCoolant;
                        Machine.Rotate    = eeprom.CanRotate;
                        Machine.Spindle   = eeprom.HasSpindle;
                        Machine.SDSupport = eeprom.HasSD;
                        Machine.Rotate    = eeprom.CanRotate;
                        Machine.Coolant   = eeprom.HasCoolant;
                        Machine.Laser     = eeprom.IsLaser;
                        Machine.Axis      = (int)eeprom.UseAxis;

                        Machine.SizeX = eeprom.GetAxis(0).Size / 1000m;
                        Machine.SizeY = eeprom.GetAxis(1).Size / 1000m;
                        Machine.SizeZ = eeprom.GetAxis(2).Size / 1000m;
                        Machine.SizeA = eeprom.GetAxis(3).Size / 1000m;

                        Machine.CommandSyntax = eeprom.CommandSyntax;

                        var orig = Machine;
                        Machine = null;
                        Machine = orig;
                    }
                }
                catch (Exception e)
                {
                    MessageBox?.Invoke("Open serial port failed? " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    await _global.Com.Current.DisconnectAsync();
                }
            }
        }

        public bool CanReadFromMachine()
        {
            return true;
        }

        #endregion

        #region Commands

        public ICommand SaveMachineCommand   => new DelegateCommand(SaveMachine,   CanSaveMachine);
        public ICommand DeleteMachineCommand => new DelegateCommand(DeleteMachine, CanDeleteMachine);
        public ICommand AddMachineCommand    => new DelegateCommand(AddMachine,    CanAddMachine);

        public ICommand ReadFromMachineCommand => new DelegateCommand(ReadFromMachine, CanReadFromMachine);

        #endregion
    }
}