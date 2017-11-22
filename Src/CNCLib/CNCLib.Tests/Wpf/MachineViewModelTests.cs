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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using NSubstitute;
using CNCLib.Wpf.ViewModels;
using CNCLib.Logic.Contracts.DTO;
using Framework.Tools.Dependency;
using CNCLib.ServiceProxy;
using System.Threading.Tasks;

namespace CNCLib.Tests.Wpf
{
    [TestClass]
	public class MachineViewModelTests : CNCUnitTest
	{
		/*
				[ClassInitialize]
				public static void ClassInit(TestContext testContext)
				{
				}

				[TestInitialize]
				public void Init()
				{
				}
		*/
/*
		private FactoryType2Obj CreateMock()
		{
			var mockfactory = new FactoryType2Obj();
			BaseViewModel.LogicFactory = mockfactory;
			return mockfactory;
        }
*/
		private TInterface CreateMock<TInterface>() where TInterface : class, IDisposable
        {
//			var mockfactory = CreateMock();
			TInterface rep = Substitute.For<TInterface>();
//			mockfactory.Register(typeof(TInterface), rep);

            Dependency.Container.RegisterInstance(rep);

			return rep;
		}

		[TestMethod]
		public async Task GetMachine()
		{
			var rep = CreateMock<IMachineService>();

			Machine machine = CreateMachine(1);
			rep.Get(1).Returns(machine);

			MachineViewModel mv = new MachineViewModel(rep);
			await mv.LoadMachine(1);

			Assert.AreEqual(false, mv.AddNewMachine);
			Assert.AreEqual(machine.Name, mv.Machine.Name);

			Assert.AreEqual(machine.MachineCommands.Count(), mv.MachineCommands.Count);
			Assert.AreEqual(machine.MachineInitCommands.Count(), mv.MachineInitCommands.Count);

			Assert.AreEqual(machine.Name,mv.Machine.Name);
			Assert.AreEqual(machine.ComPort,mv.Machine.ComPort);
			Assert.AreEqual(machine.Axis,mv.Machine.Axis);
			Assert.AreEqual(machine.BaudRate,mv.Machine.BaudRate);;
			Assert.AreEqual(machine.CommandToUpper, mv.Machine.CommandToUpper);
			Assert.AreEqual(machine.SizeX	 ,mv.Machine.SizeX);
			Assert.AreEqual(machine.SizeY	 ,mv.Machine.SizeY);
			Assert.AreEqual(machine.SizeZ	 ,mv.Machine.SizeZ);
			Assert.AreEqual(machine.SizeA	 ,mv.Machine.SizeA);
			Assert.AreEqual(machine.SizeB	 ,mv.Machine.SizeB);
			Assert.AreEqual(machine.SizeC	 ,mv.Machine.SizeC);
			Assert.AreEqual(machine.BufferSize,mv.Machine.BufferSize);
			Assert.AreEqual(machine.ProbeSizeX,mv.Machine.ProbeSizeX);
			Assert.AreEqual(machine.ProbeSizeY,mv.Machine.ProbeSizeY);
			Assert.AreEqual(machine.ProbeSizeZ,mv.Machine.ProbeSizeZ);
			Assert.AreEqual(machine.ProbeDist,mv.Machine.ProbeDist);
			Assert.AreEqual(machine.ProbeDistUp,mv.Machine.ProbeDistUp);
			Assert.AreEqual(machine.ProbeFeed,mv.Machine.ProbeFeed);
			Assert.AreEqual(machine.SDSupport,mv.Machine.SDSupport);
			Assert.AreEqual(machine.Spindle,mv.Machine.Spindle);
			Assert.AreEqual(machine.Coolant,mv.Machine.Coolant);
			Assert.AreEqual(machine.Rotate,mv.Machine.Rotate);
		}

		[TestMethod]
		public void GetMachineAddNew()
		{
			var rep = CreateMock<IMachineService>();

			Machine machine1 = CreateMachine(1);
			rep.Get(1).Returns(machine1);

			Machine machinedef = CreateMachine(0);
			rep.DefaultMachine().Returns(machinedef);

			MachineViewModel mv = new MachineViewModel(rep);
			mv.LoadMachine(-1);

			Assert.AreEqual(true, mv.AddNewMachine);
			Assert.AreEqual(machinedef.Name, mv.Machine.Name);

			Assert.AreEqual(machinedef.MachineCommands.Count(), mv.MachineCommands.Count);
			Assert.AreEqual(machinedef.MachineInitCommands.Count(), mv.MachineInitCommands.Count);
		}

		private Machine CreateMachine(int machineid)
		{
            MachineCommand[] machinecommand = new MachineCommand[]
				{ new MachineCommand() { MachineID = machineid, CommandName = "Test1", CommandString = "G20",MachineCommandID = machineid*10 + 0  },
				  new MachineCommand() { MachineID = machineid, CommandName = "Test2", CommandString = "G21",MachineCommandID = machineid*10 + 1 }
				};
			MachineInitCommand[] machineinitcommand = new MachineInitCommand[]
				{ new MachineInitCommand() { MachineID = machineid, SeqNo = 1, CommandString = "G20",MachineInitCommandID = machineid*20 },
				  new MachineInitCommand() { MachineID = 1, SeqNo = 2, CommandString = "G21",MachineInitCommandID =machineid*20 + 1 }
				};
			var machine = new Machine()
			{
				MachineID = machineid,
				Name = "Maxi" + machineid.ToString(),
				ComPort = "Com7",
				Axis = 3,
				BaudRate = 115200,
                NeedDtr = false,
                CommandToUpper = true,
				SizeX = 1234,
				SizeY = 5678,
				SizeZ = 987,
				SizeA = 1,
				SizeB = 2,
				SizeC = 3,
				BufferSize = 63,
				ProbeSizeX = 0,
				ProbeSizeY = 0,
				ProbeSizeZ = 25,
				ProbeDist = 3,
				ProbeDistUp = 10,
				ProbeFeed = 300,
				SDSupport = true,
				Spindle = true,
				Coolant = true,
				Rotate = true,
				MachineCommands = machinecommand,
				MachineInitCommands = machineinitcommand
			};

			return machine;
        }
	}
}
