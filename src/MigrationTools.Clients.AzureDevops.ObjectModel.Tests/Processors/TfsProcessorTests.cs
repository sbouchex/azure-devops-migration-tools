﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;
using MigrationTools.Processors.Infrastructure;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;
using MigrationTools.Shadows;
using MigrationTools.Endpoints;
using MigrationTools._EngineV1.Clients;
using MigrationTools.Endpoints.Infrastructure;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.Services.Commerce;

namespace MigrationTools.Processors.Tests
{
    public class TfsProcessorTests
    {

        [TestInitialize]
        public void Setup()
        {
        }

        protected TfsTeamSettingsProcessor GetTfsTeamSettingsProcessor(TfsTeamSettingsProcessorOptions options = null)
        {
            string SourceName = options != null ? options.SourceName : "Target";
            string TargetName = options != null ? options.TargetName : "Source";

            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            // Add required DI Bits
            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();
            services.AddSingleton<IWorkItemQueryBuilderFactory, WorkItemQueryBuilderFactory>();
            services.AddSingleton<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            // Add the Processor
            services.AddSingleton<TfsTeamSettingsProcessor>();
            // Add the Endpoints
            services.AddKeyedSingleton(typeof(IEndpoint), SourceName, (sp, key) =>
            {
                IOptions<TfsTeamSettingsEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new TfsTeamSettingsEndpointOptions()
                {
                    Organisation = "https://dev.azure.com/nkdagility-preview/",
                    Project = "migrationSource1",
                    AccessToken = TestingConstants.AccessToken,
                    AuthenticationMode = AuthenticationMode.AccessToken,
                    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId",
                });
               return ActivatorUtilities.CreateInstance(sp, typeof(TfsTeamSettingsEndpoint), options);
            });
            services.AddKeyedSingleton(typeof(IEndpoint), TargetName, (sp, key) =>
            {
                IOptions<TfsTeamSettingsEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new TfsTeamSettingsEndpointOptions()
                {
                    Organisation = "https://dev.azure.com/nkdagility-preview/",
                    Project = "migrationTarget1",
                    AccessToken = TestingConstants.AccessToken,
                    AuthenticationMode = AuthenticationMode.AccessToken,
                    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId",
                });
                return ActivatorUtilities.CreateInstance(sp, typeof(TfsTeamSettingsEndpoint), options);
            });
            // Add the settings
            services.Configure<TfsTeamSettingsProcessorOptions>(o =>
            {
                o.Enabled = options != null ? options.Enabled : true;
                o.SourceName = SourceName;
                o.TargetName = TargetName;
                o.Enrichers = options != null ? options.Enrichers : null;
                o.ProcessorEnrichers = options != null ? options.ProcessorEnrichers : null;
                o.RefName = options != null ? options.RefName : null;
                /// Add custom
                o.PrefixProjectToNodes = options != null ? options.PrefixProjectToNodes : false;
                o.MigrateTeamCapacities = options != null ? options.MigrateTeamCapacities : false;
                o.MigrateTeamSettings = options != null ? options.MigrateTeamSettings : false;
                o.Teams = options?.Teams != null ? options.Teams : new List<string>() { "Team 1" };
            });
            ///Return the processor
            return services.BuildServiceProvider().GetService<TfsTeamSettingsProcessor>();
        }


        protected TfsSharedQueryProcessor GetTfsSharedQueryProcessor(TfsSharedQueryProcessorOptions options = null)
        {
            string SourceName = options != null ? options.SourceName : "Target";
            string TargetName = options != null ? options.TargetName : "Source";

            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();

            services.AddSingleton<TfsSharedQueryProcessor>();

            services.AddKeyedSingleton(typeof(IEndpoint), SourceName, (sp, key) =>
            {
                IOptions<TfsTeamProjectEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new TfsTeamProjectEndpointOptions()
                {
                    Collection = new System.Uri("https://dev.azure.com/nkdagility-preview/"),
                    Project = "migrationSource1",
                    Authentication = new TfsAuthenticationOptions()
                    {
                        AuthenticationMode = AuthenticationMode.AccessToken,
                        AccessToken = TestingConstants.AccessToken
                    },
                    AllowCrossProjectLinking = false,
                    LanguageMaps = new TfsLanguageMapOptions()
                    {
                        AreaPath = "Area",
                        IterationPath = "Iteration"
                    },
                });
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(TfsTeamProjectEndpoint), options);
                return endpoint;
            });

            services.AddKeyedSingleton(typeof(IEndpoint), TargetName, (sp, key) =>
            {
                IOptions<TfsTeamProjectEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new TfsTeamProjectEndpointOptions()
                {
                    Collection = new System.Uri("https://dev.azure.com/nkdagility-preview/"),
                    Project = "migrationTarget1",
                    Authentication = new TfsAuthenticationOptions()
                    {
                        AuthenticationMode = AuthenticationMode.AccessToken,
                        AccessToken = TestingConstants.AccessToken
                    },
                    AllowCrossProjectLinking = false,
                    LanguageMaps = new TfsLanguageMapOptions()
                    {
                        AreaPath = "Area",
                        IterationPath = "Iteration"
                    },
                });
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(TfsTeamProjectEndpoint), options);
                return endpoint;
            });

            services.Configure<TfsSharedQueryProcessorOptions>(o =>
            {
                o.Enabled = options != null ? options.Enabled : true;
                o.SourceName = SourceName;
                o.TargetName = TargetName;
                o.Enrichers = options != null ? options.Enrichers : null;
                o.ProcessorEnrichers = options != null ? options.ProcessorEnrichers : null;
                o.RefName = options != null ? options.RefName : null;
                /// Add custom
                o.SourceToTargetFieldMappings = options != null ? options.SourceToTargetFieldMappings : new System.Collections.Generic.Dictionary<string, string> { {"sourceFieldA", "targetFieldB" } };
                o.PrefixProjectToNodes = options != null ? options.PrefixProjectToNodes : false;
                o.SharedFolderName = options != null ? options.SharedFolderName : "Shared Queries";
            });

            return services.BuildServiceProvider().GetService<TfsSharedQueryProcessor>();
        }

    }
}