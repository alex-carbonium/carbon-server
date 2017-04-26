using System;

namespace Carbon.Framework.Logging
{
    public class OperationContext : IDisposable
    {
        private static readonly ObjectPool<OperationContext> _poolInstance = CreatePool(32);

        public string SessionId { get; set; }
        public string OperationId { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }

        public static OperationContext GetInstance()
        {
            return _poolInstance.Allocate();
        }

        public static OperationContext GetForUser(string userId)
        {
            var context = GetInstance();
            context.UserId = userId;
            //first assuming that user performs operation in his own company
            context.CompanyId = userId;
            return context;
        }

        public void Free()
        {
            Clear();
            _poolInstance.Free(this);
        }

        public void Dispose()
        {
            Free();
        }

        private void Clear()
        {
            SessionId = null;
            OperationId = null;
            CompanyId = null;
            UserId = null;
            ProjectId = null;
        }

        private static ObjectPool<OperationContext> CreatePool(int size)
        {
            return new ObjectPool<OperationContext>(() => new OperationContext(), size);
        }
    }
}
