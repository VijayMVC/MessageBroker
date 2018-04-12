// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class ReturnIdRow
    {
        public long Id { get; set; }

        public static ReturnIdRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new ReturnIdRow
            {
                Id = reader.Get<long>(nameof(Id)),
            };
        }
    }
}
