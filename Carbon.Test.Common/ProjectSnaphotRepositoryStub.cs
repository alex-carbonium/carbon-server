using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Framework.Repositories;

namespace Carbon.Test.Common
{
    public class ProjectSnaphotRepositoryStub : InMemoryRepository<ProjectSnapshot>
    {
        private readonly Dictionary<string, byte[]> _buffers = new Dictionary<string, byte[]>();

        public override Task InsertAsync(ProjectSnapshot entity)
        {
            SaveStream(entity);
            return base.InsertAsync(entity);
        }

        public override Task UpdateAsync(ProjectSnapshot entity)
        {
            SaveStream(entity);
            return base.UpdateAsync(entity);
        }

        public override async Task<ProjectSnapshot> FindByIdAsync(dynamic key, bool lockForUpdate = false)
        {
            var snapshot = await base.FindByIdAsync((string)key, lockForUpdate);
            if (snapshot != null)
            {
                snapshot.ContentStream = new MemoryStream(_buffers[snapshot.Id]);
            }
            return snapshot;
        }

        private void SaveStream(ProjectSnapshot entity)
        {
            var buffer = new byte[entity.ContentStream.Length];
            entity.ContentStream.Read(buffer, 0, buffer.Length);
            entity.ContentStream.Position = 0;
            _buffers[entity.Id] = buffer;
        }
    }
}