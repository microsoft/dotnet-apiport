// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using System;

namespace Functions
{
    public class StorageManager
    {
        private static readonly StorageManager s_manager;
        private readonly IStorage _storage;

        public IStorage Storage => _storage;

        public static StorageManager Initialize()
            => s_manager ?? new StorageManager();

        private StorageManager()
        {
            //TODO: replace with configuration service
            var connection = Environment.GetEnvironmentVariable("Connection")
                ?? "UseDevelopmentStorage=true";
            //TODO: get from DI
            _storage = new AzureStorage(CloudStorageAccount.Parse(connection));
        }
    }
}
