// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerAdministrationApi
    {
        Task<RestResponse> ResetDatabase(IWorkContext context);

        Task<RestResponse> SetHistorySizeConfiguration(IWorkContext context, int size);

        Task<RestResponse<HistoryDetailContractV1>> GetHistorySizeConfiguration(IWorkContext context);
    }
}
