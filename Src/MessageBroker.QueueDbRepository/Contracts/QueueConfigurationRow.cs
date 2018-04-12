// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class QueueConfigurationRow
    {
        public string QueueName { get; set; }

        public int? QueueSize { get; set; }

        public DateTime? LastProcessedDate { get; set; }

        public static QueueConfigurationRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new QueueConfigurationRow
            {
                QueueName = reader.Get<string>(nameof(QueueName)),
                QueueSize = reader.Get<int?>(nameof(QueueSize)),
                LastProcessedDate = reader.Get<DateTime?>(nameof(LastProcessedDate)),
            };
        }
    }
}
