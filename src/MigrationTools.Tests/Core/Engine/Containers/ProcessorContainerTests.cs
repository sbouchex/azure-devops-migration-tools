﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Processors;
using MigrationTools.Tests;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Processors.Infrastructure.Shadows;

namespace MigrationTools.Engine.Containers.Tests
{
    [TestClass()]
    public class ProcessorContainerTests
    {
        private IOptions<ProcessorContainerOptions> CreateProcessorContainerOptions()
        {
            var options = new ProcessorContainerOptions();
            var opts = Microsoft.Extensions.Options.Options.Create(options);
            return opts;
        }


        private IServiceProvider CreateServiceProvider()
        {
            return ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        [TestMethod(), TestCategory("L0")]
        public void ProcessorContainerTest()
        {
            var config = CreateProcessorContainerOptions();
            var testSimple = new MockSimpleProcessorOptions();

            Assert.AreEqual(0, config.Value.Processors.Count);

            testSimple.Enabled = true;
            config.Value.Processors.Add(testSimple);

            Assert.AreEqual(1, config.Value.Processors.Count);
            var processorContainer = ActivatorUtilities.CreateInstance<ProcessorContainer>(CreateServiceProvider(), config, new NullLogger<ProcessorContainer>());
            Assert.AreEqual(1, processorContainer.Count);
        }
    }
}