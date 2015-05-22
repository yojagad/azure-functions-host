﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Dashboard.Data
{
    internal class FunctionIndexVersionManager : IFunctionIndexVersionManager
    {
        private readonly IVersionedMetadataTextStore _store;

        internal const string VersionBlobName = "version";

        public FunctionIndexVersionManager(CloudBlobClient client)
            : this(VersionedTextStore.CreateBlobStore(
                client, DashboardContainerNames.Dashboard, DashboardDirectoryNames.Functions))
        {
        }

        private FunctionIndexVersionManager(IVersionedMetadataTextStore store)
        {
            _store = store;
        }

        public void UpdateOrCreateIfLatest(DateTimeOffset version)
        {
            _store.UpdateOrCreateIfLatest(VersionBlobName, version, null, String.Empty);
        }
    }
}
