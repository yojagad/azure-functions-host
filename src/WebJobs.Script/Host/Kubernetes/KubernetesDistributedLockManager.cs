// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Script
{
    // This is an implementation of IDistributedLockManager to be used when running
    // in Kubernetes environments.
    internal class KubernetesDistributedLockManager : IDistributedLockManager
    {
        private readonly KubernetesClient _kubernetesClient;
        private readonly ILogger _logger;

        public KubernetesDistributedLockManager(ILoggerFactory loggerFactory, IEnvironment environment)
        {
            _logger = loggerFactory.CreateLogger(ScriptConstants.LogCategoryHostGeneral);
            _kubernetesClient = new KubernetesClient(environment);
        }

        public async Task<string> GetLockOwnerAsync(string account, string lockId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("K8se: getlockownerasync");
            var response = await _kubernetesClient.GetLock(lockId);
            return response.Owner;
        }

        public async Task ReleaseLockAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
        {
            var kubernetesLock = (KubernetesLockHandle)lockHandle;
            var response = await _kubernetesClient.ReleaseLock(kubernetesLock.LockId, kubernetesLock.Owner);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> RenewAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
        {
            var kubernetesLock = (KubernetesLockHandle)lockHandle;
            _logger.LogDebug($"K8se: RenewAsync for {kubernetesLock.LockId} owner {kubernetesLock.Owner} for time {kubernetesLock.LockPeriod.ToString()}");
            var duration = TimeSpan.Parse(kubernetesLock.LockPeriod);
            var renewedLockHandle = await _kubernetesClient.TryAcquireLock(kubernetesLock.LockId, kubernetesLock.Owner, duration, cancellationToken);
            return !string.IsNullOrEmpty(renewedLockHandle.LockId);
        }

        public async Task<IDistributedLock> TryLockAsync(string account, string lockId, string lockOwnerId, string proposedLeaseId, TimeSpan lockPeriod, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"K8se: Trylockasync for {lockId} owner {lockOwnerId} for time {lockPeriod.ToString()}");
            var kubernetesLock = await _kubernetesClient.TryAcquireLock(lockId, lockOwnerId, lockPeriod, cancellationToken);
            if (string.IsNullOrEmpty(kubernetesLock.LockId))
            {
                return null;
            }
            return kubernetesLock;
        }
    }
}
