// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class QueueStatusRow
    {
        public string QueueName { get; set; }

        public int QueueCount { get; set; }

        public int CurrentSizeLimit { get; set; }

        public int QueueSize { get; set; }

        public static QueueStatusRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new QueueStatusRow
            {
                QueueName = reader.Get<string>(nameof(QueueName)),
                QueueCount = reader.Get<int>(nameof(QueueCount)),
                CurrentSizeLimit = reader.Get<int>(nameof(CurrentSizeLimit)),
                QueueSize = reader.Get<int>(nameof(QueueSize)),
            };
        }
    }
}
