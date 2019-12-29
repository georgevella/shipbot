using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace Shipbot.Controller.Core
{
    public class OrleansClientStartup : IHostedService
    {
        private readonly IClusterClient _clusterClient;

        public OrleansClientStartup(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _clusterClient.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _clusterClient.Close();
        }
    }
}