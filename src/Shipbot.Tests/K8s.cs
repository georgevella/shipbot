using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoDeploy.ArgoSupport.Models.K8s.Crd;
using FluentAssertions;
using k8s;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OperatorSdk;
using Shipbot.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Shipbot.Tests
{
    public class K8s : BaseUnitTestClass
    {
        class Y : IWatcherEventHandler<ArgoApplicationResource>
        {
            private readonly ILogger _log;

            private readonly ConcurrentDictionary<string, ArgoApplicationResource> _store = new ConcurrentDictionary<string, ArgoApplicationResource>();

            public Y(ILogger log)
            {
                _log = log;
            }
            
            public Task Handle(WatchEventType eventType, ArgoApplicationResource item)
            {
                _store.AddOrUpdate(
                    item.Metadata.Name,
                    (key) =>
                    {
                        _log.LogInformation($"[NEW] {eventType}: {item.Metadata.Name} [{item.Status.Health.Status}]");
                        return item;
                    },
                    (key, current) =>
                    {
                        _log.LogInformation(
                            $"[UPDATE] {eventType}: {item.Metadata.Name} [{item.Status.Health.Status}]"
                            );
                        return item;
                    }
                );
                
                return Task.CompletedTask;
            }

            private ArgoApplicationResource AddValueFactory(string arg)
            {
                throw new NotImplementedException();
            }
        }
        
        [Fact]
        public async Task WatcherTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var factory = new NamespacedResourceWatcherFactory<ArgoApplicationResource>(
                config,
                new[] {new Y(GetLogger())}
            );
            
            await factory.Start("argo", cancellationTokenSource.Token);
            
            Thread.Sleep(TimeSpan.FromMinutes(10));
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task GetNamespacedCustomResource()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var kubernetes = new Kubernetes(config);
            var entity = await kubernetes.GetNamespacedCustomResource<ArgoApplicationResource>(
                "argo",
                "azl-dev-elysium-api",
                CancellationToken.None
            );

        }
        
        [Fact]
        public async Task Crd_Equality_test()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var kubernetes = new Kubernetes(config);
            var entity1 = await kubernetes.GetNamespacedCustomResource<ArgoApplicationResource>(
                "argo",
                "azl-dev-elysium-api",
                CancellationToken.None
            );
            
            var entity2 = await kubernetes.GetNamespacedCustomResource<ArgoApplicationResource>(
                "argo",
                "azl-dev-elysium-api",
                CancellationToken.None
            );
            
            entity1.Should().BeEquivalentTo(entity2);

        }

        public K8s(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}