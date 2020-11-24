// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Script.Eventing;
using Microsoft.Azure.WebJobs.Script.Workers;
using Microsoft.Azure.WebJobs.Script.Workers.Rpc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Script.Grpc
{
    public class GrpcServer : IRpcServer, IDisposable, IAsyncDisposable
    {
        private readonly IHostBuilder _grpcHostBuilder;
        private readonly int _port;
        private bool _disposed = false;
        private ILogger _logger;
        private IHost _grpcHost;

        public GrpcServer(IScriptEventManager scriptEventManager, ILogger<GrpcServer> logger)
        {
            _port = WorkerUtilities.GetUnusedTcpPort();
            _grpcHostBuilder = GrpcHostBuilder.CreateHostBuilder(scriptEventManager, _port);
            _grpcHost = _grpcHostBuilder.Build();
            _grpcHost.Start();
            _logger = logger;
        }

        public Uri Uri => new Uri($"http://{WorkerConstants.HostName}:{_port}");

        public Task StartAsync()
        {
            _logger.LogInformation($"Grpc service started at port {_port}");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            _logger.LogInformation($"Grpc service is being ShutDown");
            return _grpcHost.StopAsync();
        }

        public Task KillAsync()
        {
            _logger.LogInformation($"Grpc service is being killed");
            return _grpcHost.StopAsync();
        }

        protected async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                _logger.LogInformation($"Grpc service is being disposed");
                if (disposing)
                {
                    await _grpcHost.StopAsync();
                    _grpcHost.Dispose();
                }
                _disposed = true;
            }
        }

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }

        public void Dispose()
        {
            DisposeAsync();
        }
    }
}
