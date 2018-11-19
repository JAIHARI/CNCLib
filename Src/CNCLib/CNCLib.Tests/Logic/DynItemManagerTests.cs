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
using System.Linq;
using System.Threading.Tasks;

using CNCLib.Logic.Client;
using CNCLib.Logic.Contracts.DTO;
using CNCLib.Service.Contracts;

using FluentAssertions;

using Framework.Dependency;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace CNCLib.Tests.Logic
{
    [TestClass]
    public class DynItemManagerTests : LogicTests
    {
        private TInterface CreateMock<TInterface>() where TInterface : class, IDisposable
        {
            var srv = Substitute.For<TInterface>();
            Dependency.Container.RegisterInstance(srv);
            return srv;
        }

        [TestMethod]
        public async Task GetItemNone()
        {
            var srv = CreateMock<IItemService>();

            var itemEntity = new Item[0];
            srv.GetAll().Returns(itemEntity);

            var ctrl = new DynItemController(srv);

            var all = (await ctrl.GetAll()).ToArray();
            all.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetItemAll()
        {
            var srv = CreateMock<IItemService>();

            var itemEntity = new[]
            {
                new Item { ItemId = 1, Name = "Test1" }, new Item { ItemId = 2, Name = "Test2" }
            };
            srv.GetAll().Returns(itemEntity);

            var ctrl = new DynItemController(srv);
            var all  = (await ctrl.GetAll()).ToArray();

            all.Should().HaveCount(2);
            all.FirstOrDefault().
                Should().
                BeEquivalentTo(new
                {
                    ItemId = 1,
                    Name   = "Test1"
                }, options => options.ExcludingMissingMembers());
        }

        [TestMethod]
        public async Task GetAllType()
        {
            var srv = CreateMock<IItemService>();

            var itemEntity = new[]
            {
                new Item { ItemId = 1, Name = "Test1" }, new Item { ItemId = 2, Name = "Test2" }
            };
            srv.GetByClassName(DynItemController.GetClassName(typeof(string))).Returns(itemEntity);

            var ctrl = new DynItemController(srv);
            var all  = await ctrl.GetAll(typeof(string));

            all.Should().HaveCount(2);
            all.FirstOrDefault().
                Should().
                BeEquivalentTo(new
                {
                    ItemId = 1,
                    Name   = "Test1"
                }, options => options.ExcludingMissingMembers());
        }

        [TestMethod]
        public async Task GetItem()
        {
            var srv = CreateMock<IItemService>();
            srv.Get(1).Returns(new Item { ItemId = 1, Name = "Test1" });

            var ctrl = new DynItemController(srv);
            var all  = await ctrl.Get(1);

            all.Should().
                BeEquivalentTo(new
                {
                    ItemId = 1,
                    Name   = "Test1"
                }, options => options.ExcludingMissingMembers());
        }

        [TestMethod]
        public async Task GetItemNull()
        {
            var srv = CreateMock<IItemService>();

            var ctrl = new DynItemController(srv);
            var all  = await ctrl.Get(10);

            all.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateObject()
        {
            var srv = CreateMock<IItemService>();

            Item itemEntity = CreateItem();

            srv.Get(1).Returns(itemEntity);

            var ctrl = new DynItemController(srv);

            var item = await ctrl.Create(1);
            item.Should().NotBeNull();
            item.Should().BeOfType(typeof(DynItemManagerTestClass));

            var item2 = (DynItemManagerTestClass) item;

            item2.StringProperty.Should().Be("Hallo", item2.StringProperty);
            item2.IntProperty.Should().Be(1);
            item2.IntProperty.Should().Be(1);
            item2.DoubleProperty.Should().Be(1.234);
            item2.DoubleNullProperty.Should().Be(1.234);
            item2.DecimalProperty.Should().Be(9.876m);
            item2.DecimalNullProperty.Should().Be(9.876m);
        }

        private static Item CreateItem()
        {
            return new Item
            {
                ItemId    = 1,
                Name      = "Hallo",
                ClassName = typeof(DynItemManagerTestClass).AssemblyQualifiedName,
                ItemProperties = new[]
                {
                    new ItemProperty { ItemId = 1, Name = "StringProperty", Value                        = "Hallo" },
                    new ItemProperty { ItemId = 1, Name = "IntProperty", Value                           = "1" },
                    new ItemProperty { ItemId = 1, Name = "DoubleProperty", Value                        = "1.234" },
                    new ItemProperty { ItemId = 1, Name = "DecimalProperty", Value                       = "9.876" },
                    new ItemProperty { ItemId = 1, Name = "IntNullProperty" }, new ItemProperty { ItemId = 1, Name = "DoubleNullProperty", Value = "1.234" },
                    new ItemProperty { ItemId = 1, Name = "DecimalNullProperty", Value                   = "9.876" }
                }
            };
        }

        [TestMethod]
        public async Task AddObject()
        {
            var srv = CreateMock<IItemService>();

            Item itemEntity = CreateItem();

            var obj = new DynItemManagerTestClass
            {
                StringProperty      = "Hallo",
                IntProperty         = 1,
                DoubleProperty      = 1.234,
                DoubleNullProperty  = 1.234,
                DecimalProperty     = 9.876m,
                DecimalNullProperty = 9.876m
            };

            var ctrl = new DynItemController(srv);

            int id = await ctrl.Add("Hallo", obj);

            await srv.Received().Add(Arg.Is<Item>(x => x.Name == "Hallo"));
            await srv.Received().Add(Arg.Is<Item>(x => x.ItemId == 0));
            await srv.Received().Add(Arg.Is<Item>(x => x.ItemProperties.Count == 7));
            await srv.Received().Add(Arg.Is<Item>(x => x.ItemProperties.FirstOrDefault(y => y.Name == "StringProperty").Value == "Hallo"));
            await srv.Received().Add(Arg.Is<Item>(x => x.ItemProperties.FirstOrDefault(y => y.Name == "DoubleProperty").Value == "1.234"));
            await srv.Received().Add(Arg.Is<Item>(x => x.ItemProperties.FirstOrDefault(y => y.Name == "DecimalNullProperty").Value == "9.876"));
        }

        [TestMethod]
        public async Task DeleteItem()
        {
            // arrange

            var srv = CreateMock<IItemService>();

            Item itemEntity = CreateItem();
            srv.Get(1).Returns(itemEntity);

            var ctrl = new DynItemController(srv);

            //act

            await ctrl.Delete(1);

            //assert
            await srv.Received().Get(1);
            await srv.Received().Delete(itemEntity);
        }

        [TestMethod]
        public async Task DeleteItemNone()
        {
            // arrange

            var srv = CreateMock<IItemService>();

            var ctrl = new DynItemController(srv);

            //act

            await ctrl.Delete(1);

            //assert
            await srv.Received().Get(1);
            await srv.DidNotReceiveWithAnyArgs().Delete((Item) null);
        }

        [TestMethod]
        public async Task SaveItem()
        {
            // arrange

            var srv  = CreateMock<IItemService>();
            var ctrl = new DynItemController(srv);

            //act

            await ctrl.Save(1, "Test", new DynItemManagerTestClass { IntProperty = 1 });

            //assert
            await srv.Received().Update(Arg.Is<Item>(x => x.ItemId == 1));
            await srv.Received().Update(Arg.Is<Item>(x => x.ItemProperties.FirstOrDefault(y => y.Name == "IntProperty").Value == "1"));
            await srv.DidNotReceiveWithAnyArgs().Delete((Item) null);
        }
    }
}