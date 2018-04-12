// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class AgentRow
    {
        public int AgentId { get; set; }

        public string AgentName { get; set; }

        public DateTime _createdDate { get; set; }

        public static AgentRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new AgentRow
            {
                AgentId = reader.Get<int>(nameof(AgentId)),
                AgentName = reader.Get<string>(nameof(AgentName)),
                _createdDate = reader.Get<DateTime>(nameof(_createdDate))
            };
        }
    }
}
