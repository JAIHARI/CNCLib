﻿////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2016 Herbert Aitenbichler

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
using CNCLib.Repository.Contracts.Entities;
using CNCLib.Repository.Contracts;
using CNCLib.Logic;
using System.Linq;
using NSubstitute;
using Framework.Tools.Dependency;
using Framework.Tools.Pattern;
using Framework.EF;

namespace CNCLib.Tests.Logic
{
	[TestClass]
	public class MachineControllerTests : CNCUnitTest
	{
		private TInterface CreateMock<TInterface>() where TInterface : class, IDisposable
		{
			//			var mockfactory = CreateMock();
			TInterface rep = Substitute.For<TInterface>();
			//			mockfactory.Register(typeof(TInterface), rep);

			Dependency.Container.RegisterInstance(rep);

			return rep;
		}

		[TestMethod]
		public void AddMachine()
		{
			var rep = CreateMock<IMachineRepository>();

			MachineController ctrl = new MachineController();

			var machineEntity1 = new CNCLib.Logic.Contracts.DTO.Machine()
			{
				MachineID = 1,
				Name = "Maxi",
				MachineCommands = new CNCLib.Logic.Contracts.DTO.MachineCommand[1]
						{ new CNCLib.Logic.Contracts.DTO.MachineCommand() { MachineID = 1, MachineCommandID = 1, CommandName = @"1", CommandString=@"1",PosX=0,PosY=1 } },
				MachineInitCommands = new CNCLib.Logic.Contracts.DTO.MachineInitCommand[1]
					{ new CNCLib.Logic.Contracts.DTO.MachineInitCommand() { MachineID = 1, MachineInitCommandID = 1, CommandString ="2", SeqNo=1 } }
			};

			var machineID = ctrl.Add(machineEntity1);

			rep.ReceivedWithAnyArgs().Store(new Machine());
			Assert.AreEqual(machineID, 0);
		}

		[TestMethod]
		public void UpdateMachine()
		{
			var rep = CreateMock<IMachineRepository>();

			MachineController ctrl = new MachineController();

			var machineEntity1 = new Machine() { MachineID = 11, Name = "Maxi", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			rep.GetMachine(1).Returns(machineEntity1);

			var machine = ctrl.Get(1);
			machine.Name = "SuperMaxi";

			ctrl.Update(machine);

			rep.Received().Store(Arg.Is<Machine>(x => x.Name == "SuperMaxi"));
			rep.Received().Store(Arg.Is<Machine>(x => x.MachineID == 11));
		}

		[TestMethod]
		public void DeleteMachine()
		{
			var rep = CreateMock<IMachineRepository>();

			MachineController ctrl = new MachineController();

			var machineEntity1 = new Machine() { MachineID = 11, Name = "Maxi", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			rep.GetMachine(1).Returns(machineEntity1);

			var machine = ctrl.Get(1);
			machine.Name = "SuperMaxi";

			ctrl.Delete(machine);

			rep.Received().Delete(Arg.Is<Machine>(x => x.Name == "SuperMaxi"));
			rep.Received().Delete(Arg.Is<Machine>(x => x.MachineID == 11));
		}

		[TestMethod]
		public void GetMachinesNone()
		{
			var rep = CreateMock<IMachineRepository>();

			var machineEntity = new Machine[0];
			rep.GetMachines().Returns(machineEntity);

			MachineController ctrl = new MachineController();

			var machines = ctrl.GetAll().ToArray();
			Assert.AreEqual(true, machines.Length == 0);
		}

		[TestMethod]
		public void GetMachinesOne()
		{
			var rep = CreateMock<IMachineRepository>();

			var machineEntity = new Machine[] { new Machine() { MachineID = 1, Name = "Maxi", BufferSize = 115200, MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] } };
			rep.GetMachines().Returns(machineEntity);

			MachineController ctrl = new MachineController();

			var machines = ctrl.GetAll().ToArray();
			Assert.AreEqual(true, machines.Length == 1);
			Assert.AreEqual(1, machines[0].MachineID);
			Assert.AreEqual("Maxi", machines[0].Name);
			Assert.AreEqual(115200, machines[0].BufferSize);
			Assert.IsNotNull(machines[0].MachineCommands);
			Assert.IsNotNull(machines[0].MachineInitCommands);
			Assert.AreEqual(0, machines[0].MachineCommands.Count());
			Assert.AreEqual(0, machines[0].MachineInitCommands.Count());
		}

		[TestMethod]
		public void GetMachinesMany()
		{
			var rep = CreateMock<IMachineRepository>();

			var machineEntity = new Machine[]
			{ new Machine() { MachineID = 1, Name = "Maxi", BufferSize = 115200, MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] },
			  new Machine() { MachineID = 2, Name = "Maxi", BufferSize = 115200,
								MachineCommands = new MachineCommand[] { new MachineCommand() { MachineID =2,MachineCommandID=1,CommandName="Test",CommandString="f"  } },
								MachineInitCommands = new MachineInitCommand[] { new MachineInitCommand() { MachineID =2,MachineInitCommandID=1,SeqNo=0,CommandString="f"  } } }
			};

			rep.GetMachines().Returns(machineEntity);

			MachineController ctrl = new MachineController();

			var machines = ctrl.GetAll().ToArray();
			Assert.AreEqual(true, machines.Length == 2);
			Assert.AreEqual(1, machines[0].MachineID);
			Assert.AreEqual("Maxi", machines[0].Name);
			Assert.AreEqual(115200, machines[0].BufferSize);
			Assert.AreEqual(1, machines[1].MachineCommands.Count());
			Assert.AreEqual(1, machines[1].MachineInitCommands.Count());
			Assert.AreEqual(0, machines[0].MachineCommands.Count());
			Assert.AreEqual(0, machines[0].MachineInitCommands.Count());
			Assert.AreEqual("Test", machines[1].MachineCommands.First().CommandName);
			Assert.AreEqual("f", machines[1].MachineCommands.First().CommandString);
			Assert.AreEqual(0, machines[1].MachineInitCommands.First().SeqNo);
			Assert.AreEqual("f", machines[1].MachineInitCommands.First().CommandString);
		}

		[TestMethod]
		public void QueryOneMachinesFound()
		{
			var rep = CreateMock<IMachineRepository>();

			var machineEntity1 = new Machine() { MachineID = 1, Name = "Maxi", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			var machineEntity2 = new Machine() { MachineID = 2, Name = "Mini", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			rep.GetMachine(1).Returns(machineEntity1);
			rep.GetMachine(2).Returns(machineEntity2);

			MachineController ctrl = new MachineController();

			var machine = ctrl.Get(1);
			Assert.AreEqual(machineEntity1.Name, machine.Name);
			Assert.AreEqual(machineEntity1.MachineID, machine.MachineID);
			Assert.IsNotNull(machine.MachineCommands);
			Assert.IsNotNull(machine.MachineInitCommands);
			Assert.AreEqual(0, machine.MachineCommands.Count());
			Assert.AreEqual(0, machine.MachineInitCommands.Count());
		}

		[TestMethod]
		public void QueryOneMachinesNotFound()
		{
			var rep = CreateMock<IMachineRepository>();

			var machineEntity1 = new Machine() { MachineID = 1, Name = "Maxi", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			var machineEntity2 = new Machine() { MachineID = 2, Name = "Mini", MachineCommands = new MachineCommand[0], MachineInitCommands = new MachineInitCommand[0] };
			rep.GetMachine(1).Returns(machineEntity1);
			rep.GetMachine(2).Returns(machineEntity2);

			MachineController ctrl = new MachineController();

			var machine = ctrl.Get(3);
			Assert.IsNull(machine);
		}

		[TestMethod]
		public void DefaultMachine()
		{
			MachineController ctrl = new MachineController();

			var machine = ctrl.DefaultMachine();
			Assert.IsNotNull(machine);
			Assert.AreEqual("New", machine.Name);
		}

		[TestMethod]
		public void GetDefaultMachine()
		{
			var rep = CreateMock<IConfigurationRepository>();

			MachineController ctrl = new MachineController();

			rep.Get("Environment", "DefaultMachineID").Returns(new Configuration() { Value = "14" });

			Assert.AreEqual(14, ctrl.GetDetaultMachine());
		}

		[TestMethod]
		public void GetDefaultMachineNotSet()
		{
			var rep = CreateMock<IConfigurationRepository>();

			MachineController ctrl = new MachineController();

			Configuration nullconfig=null;

			rep.Get("Environment", "DefaultMachineID").Returns(nullconfig);

			Assert.AreEqual(-1, ctrl.GetDetaultMachine());
		}

		[TestMethod]
		public void SetDefaultMachine()
		{
			var rep = CreateMock<IConfigurationRepository>();

			MachineController ctrl = new MachineController();

			ctrl.SetDetaultMachine(15);

			rep.Get("Environment", "DefaultMachineID").Returns(new Configuration() { Value = "14" });

			rep.Received().Save(Arg.Is<Configuration>(x => x.Group == "Environment" && x.Name == "DefaultMachineID" && x.Value == "15"));
		}
	}
}
