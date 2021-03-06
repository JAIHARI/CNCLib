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

using System.Linq;
using System.Threading.Tasks;

using CNCLib.Repository;
using CNCLib.Repository.Abstraction;
using CNCLib.Repository.Abstraction.Entities;
using CNCLib.Repository.Context;

using FluentAssertions;

using Framework.Repository;
using Framework.Tools;
using Framework.UnitTest.Repository;

using Xunit;

namespace CNCLib.UnitTest.Repository
{
    public class ConfigurationRepositoryTests : RepositoryTests, IClassFixture<RepositoryTestFixture>
    {
        #region crt and overrides

        public ConfigurationRepositoryTests(RepositoryTestFixture testFixture) : base(testFixture)
        {
        }

        protected CRUDRepositoryTests<CNCLibContext, Configuration, ConfigurationPrimary, IConfigurationRepository> CreateTestContext()
        {
            return new CRUDRepositoryTests<CNCLibContext, Configuration, ConfigurationPrimary, IConfigurationRepository>()
            {
                CreateTestDbContext = () =>
                {
                    var context = TestFixture.CreateDbContext();
                    var uow     = new UnitOfWork<CNCLibContext>(context);
                    var rep     = new ConfigurationRepository(context, UserContext);
                    return new CRUDTestDbContext<CNCLibContext, Configuration, ConfigurationPrimary, IConfigurationRepository>(context, uow, rep);
                },
                GetEntityKey = (entity) => new ConfigurationPrimary() { Group = entity.Group, Name = entity.Name },
                SetEntityKey = (entity, key) =>
                {
                    entity.Group = key.Group;
                    entity.Name  = key.Name;
                },
                CompareEntity = (entity1, entity2) => CompareProperties.AreObjectsPropertiesEqual(entity1, entity2, new string[0])
            };
        }

        #endregion

        #region CRUD Test

        [Fact]
        public async Task GetAllTest()
        {
            var entities = (await CreateTestContext().GetAll()).OrderBy(cfg => cfg.Name);
            entities.Count().Should().BeGreaterThan(3);
            entities.ElementAt(0).Group.Should().Be("TestGroup");
            entities.ElementAt(0).Name.Should().Be("TestBool");
        }

        [Fact]
        public async Task GetOKTest()
        {
            var entity = await CreateTestContext().GetOK(new ConfigurationPrimary() { Group = "TestGroup", Name = "TestBool" });
            entity.Value.Should().Be(@"True");
        }

        [Fact]
        public async Task GetTrackingOKTest()
        {
            var entity = await CreateTestContext().GetTrackingOK(new ConfigurationPrimary() { Group = "TestGroup", Name = "TestDecimal" });
            entity.Value.Should().Be(@"1.2345");
        }

        [Fact]
        public async Task GetNotExistTest()
        {
            await CreateTestContext().GetNotExist(new ConfigurationPrimary() { Group = "NotExist", Name = "NotExist" });
        }

        [Fact]
        public async Task AddUpdateDeleteTest()
        {
            await CreateTestContext().AddUpdateDelete(() => CreateConfiguration("TestGroup", "TestName"), (entity) => entity.Value = "testValueModified");
        }

        [Fact]
        public async Task AddUpdateDeleteBulkTest()
        {
            await CreateTestContext().AddUpdateDeleteBulk(
                () => new[]
                {
                    CreateConfiguration(@"AddUpdateDeleteBulk", "Test1"), CreateConfiguration(@"AddUpdateDeleteBulk", "Test2"), CreateConfiguration(@"AddUpdateDeleteBulk", "Test3")
                },
                (entities) =>
                {
                    int i = 0;
                    foreach (var entity in entities)
                    {
                        entity.Value = $"DummyNameValue{i++}";
                    }
                });
        }

        [Fact]
        public async Task AddRollbackTest()
        {
            await CreateTestContext().AddRollBack(
                () => new Configuration()
                {
                    Group = "TestGroup",
                    Name  = "TestName",
                    Type  = "string",
                    Value = "TestValue"
                });
        }

        [Fact]
        public async Task StoreTest()
        {
            await CreateTestContext().Store(
                () => new Configuration()
                {
                    Group = "TestGroup",
                    Name  = "TestName",
                    Type  = "string",
                    Value = "TestValue"
                },
                (entity) => entity.Value = "testValueModified");
        }

        private static Configuration CreateConfiguration(string group, string name)
        {
            return new Configuration() { Group = group, Name = name, Type = "string", Value = "TestValue" };
        }

        #endregion

        #region Additiona Tests

        [Fact]
        public async Task GetEmptyConfiguration()
        {
            using (var ctx = CreateTestContext().CreateTestDbContext())
            {
                var entity = await ctx.Repository.Get("Test", "Test");
                entity.Should().BeNull();
            }
        }

        [Fact]
        public async Task SaveConfiguration()
        {
            using (var ctx = CreateTestContext().CreateTestDbContext())
            {
                await ctx.Repository.Store(new Configuration("Test", "TestNew1", "Content"));
                await ctx.UnitOfWork.SaveChangesAsync();
            }
        }

        #endregion
    }
}