// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBrokerAdministration
    {
        Task ResetDatabase(IWorkContext context);

        Task SetHistorySizeConfiguration(IWorkContext context, int queueSize);

        Task<int?> GetHistorySizeConfiguration(IWorkContext context);
    }
}
