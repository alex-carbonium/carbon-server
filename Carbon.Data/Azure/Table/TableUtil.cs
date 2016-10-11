using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Business.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Data.Azure.Table
{
    public static class TableUtil
    {
        public static async Task WithOptimisticLockAsync(this CloudTable table, Func<CloudTable, Task> action)
        {
            try
            {
                await action(table);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    throw new UpdateConflictException(ex);
                }
                throw;
            }
        }

        public static async Task WithConflictHandlerAsync(this CloudTable table, Func<CloudTable, Task> action)
        {
            try
            {
                await action(table);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                {
                    throw new InsertConflictException(ex);
                }
                throw;
            }
        }

        public static void WithOptimisticLock(this CloudTable table, Action<CloudTable> action)
        {
            try
            {
                action(table);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    throw new UpdateConflictException(ex);
                }
                throw;
            }
        }

        public static void WithConflictHandler(this CloudTable table, Action<CloudTable> action)
        {
            try
            {
                action(table);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                {
                    throw new InsertConflictException(ex);
                }
                throw;
            }
        }

        public static async Task<IList<T>> ExecuteQueryAsync<T>(
           this CloudTable table,
           TableQuery<T> query,
           CancellationToken ct = default(CancellationToken))
           where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;

            do
            {
                var seg = await table.ExecuteQueryAsync(query, token, ct);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }

        public static Task<TableQuerySegment<T>> ExecuteQueryAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            TableContinuationToken token,
            CancellationToken ct = default(CancellationToken))
            where T : ITableEntity, new()
        {            
            var ar = table.BeginExecuteQuerySegmented(query, token, null, null);
            ct.Register(ar.Cancel);
            return Task.Factory.FromAsync<TableQuerySegment<T>>(ar, table.EndExecuteQuerySegmented<T>);
        }
    }
}
