using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Carbon.Framework.Cloud.Blob;
using Carbon.Framework.Repositories;
using Carbon.Framework.Specifications;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace Carbon.Data.Azure.Blob
{
    public class BlobRepository<TEntity> : Repository<TEntity>
        where TEntity : BlobDomainObject, new()
    {
        private readonly CloudBlobClient _client;
        private static readonly object _syncRoot = new object();
        private bool _corsChecked;

        public BlobRepository(CloudBlobClient client)
        {
            _client = client;
        }

        public string TestNameSuffix { get; set; }

        private void EnsureCorsEnabled(CloudBlobClient client)
        {
            if (!_corsChecked)
            {
                lock (_syncRoot)
                {
                    if (!_corsChecked)
                    {
                        var properties = client.GetServiceProperties();
                        if (properties.Cors.CorsRules.Count == 0)
                        {
                            properties.DefaultServiceVersion = "2013-08-15";
                            properties.Cors.CorsRules.Clear();
                            properties.Cors.CorsRules.Add(new CorsRule
                            {
                                AllowedOrigins = new[] { "*" },
                                AllowedMethods = CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Options,
                                AllowedHeaders = new[] { "*" },
                                ExposedHeaders = new[] { "*" },
                                MaxAgeInSeconds = (int)TimeSpan.FromDays(360 * 10).TotalSeconds
                            });
                            client.SetServiceProperties(properties);
                        }

                        _corsChecked = true;
                    }
                }
            }
        }

        private CloudBlobContainer GetContainer()
        {
            EnsureCorsEnabled(_client);

            var attributes = typeof(TEntity).GetCustomAttributes(typeof(ContainerAttribute), inherit: true);
            var name = string.Empty;
            var type = ContainerType.Private;
            if (attributes.Length == 1)
            {
                var attr = (ContainerAttribute)attributes[0];
                name = attr.Name;
                type = attr.Type;
            }
            else
            {
                name = typeof(TEntity).Name;
            }

            name = Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty).ToLower();

            if (TestNameSuffix != null)
            {
                name += TestNameSuffix;
            }

            var container = _client.GetContainerReference(name);
            try
            {
                if (container.CreateIfNotExists())
                {
                    if (type == ContainerType.Public)
                    {
                        var containerPermissions = new BlobContainerPermissions();
                        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                        container.SetPermissions(containerPermissions);
                    }
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.Conflict)
                {
                    throw;
                }
            }
            return container;
        }

        private async Task SaveBlobAsync(TEntity entity)
        {
            var blob = InitBlobFromEntity(entity);
            if (entity.ContentStream != null)
            {
                using (var blobStream = await blob.OpenWriteAsync())
                using (var gzipStream = new GZipStream(blobStream, CompressionMode.Compress, leaveOpen: true))
                {
                    await entity.ContentStream.CopyToAsync(gzipStream);
                }
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                    {
                        await gzipStream.WriteAsync(entity.Content, 0, entity.Content.Length);
                    }
                    memoryStream.Position = 0;
                    await blob.UploadFromStreamAsync(memoryStream);
                }
            }
            entity.Uri = blob.Uri;
        }
        private void SaveBlob(TEntity entity)
        {
            var blob = InitBlobFromEntity(entity);
            if (entity.ContentStream != null)
            {
                using (var blobStream = blob.OpenWrite())
                using (var gzipStream = new GZipStream(blobStream, CompressionMode.Compress, leaveOpen: true))
                {
                    entity.ContentStream.CopyTo(gzipStream);
                }
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                    {
                        gzipStream.Write(entity.Content, 0, entity.Content.Length);
                    }
                    memoryStream.Position = 0;
                    blob.UploadFromStream(memoryStream);
                }
            }
            entity.Uri = blob.Uri;
        }

        private IQueryable<TEntity> BlobsToEntities(string prefix = "")
        {
            var entities = new List<TEntity>();
            var container = GetContainer();
            prefix = container.Name + "/" + prefix;

            foreach (var blob in _client.ListBlobs(prefix, useFlatBlobListing: true))
            {
                var entity = new TEntity();
                entity.Id = blob.Uri.ToString().Replace(container.Uri + "/", string.Empty);
                entity.Uri = blob.Uri;
                entity.LazyFetched = true;
                entities.Add(entity);
            }
            return entities.AsQueryable();
        }

        private TEntity InitEntityFromBlob(CloudBlockBlob blob)
        {
            var entity = new TEntity();
            entity.Id = blob.Name;
            entity.Uri = blob.Uri;
            entity.ContentType = blob.Properties.ContentType;
            if (blob.Metadata != null)
            {
                foreach (var kv in blob.Metadata)
                {
                    entity.Metadata.Add(kv.Key, kv.Value);
                }
            }
            return entity;
        }

        private CloudBlockBlob InitBlobFromEntity(TEntity entity)
        {
            var container = GetContainer();
            var blob = container.GetBlockBlobReference(entity.Id);
            blob.Properties.ContentType = entity.ContentType;
            if (entity.CacheForever)
            {
                blob.Properties.CacheControl = "public, max-age=" + TimeSpan.FromDays(365 * 10).TotalSeconds;
            }

            blob.Properties.ContentEncoding = "gzip";
            if (entity.HasMetadata())
            {
                foreach (var kv in entity.Metadata)
                {
                    blob.Metadata.Add(kv.Key, kv.Value);
                }
            }
            return blob;
        }

        public override IQueryable<TEntity> FindAll(bool cache = false)
        {
            return BlobsToEntities();
        }

        public override IQueryable<TEntity> FindAllBy(ISpecification<TEntity> specification)
        {
            var spec = specification as PrefixSpecification<TEntity>;
            if (spec == null)
            {
                throw new Exception("Only prefix specification is supported by blob storage");
            }
            return BlobsToEntities(spec.Prefix);
        }

        public override TEntity FindSingleBy(ISpecification<TEntity> specification)
        {
            throw new NotImplementedException();
        }

        public override TEntity FindById(dynamic key, bool lockForUpdate = false)
        {
            var container = GetContainer();
            var blob = (CloudBlockBlob)container.GetBlockBlobReference(key);
            if (!blob.Exists())
            {
                return default(TEntity);
            }
            blob.FetchAttributes();
            var entity = InitEntityFromBlob(blob);
            using (var stream = blob.OpenRead())
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var unzippedStream = new MemoryStream())
            {
                gzipStream.CopyTo(unzippedStream);
                entity.Content = unzippedStream.ToArray();
            }
            return entity;
        }

        public override async Task<TEntity> FindByIdAsync(dynamic key, bool lockForUpdate = false)
        {
            var container = GetContainer();
            var blob = (CloudBlockBlob)container.GetBlockBlobReference(key);
            if (!await blob.ExistsAsync())
            {
                return default(TEntity);
            }

            await blob.FetchAttributesAsync();

            var entity = InitEntityFromBlob(blob);
            entity.ContentStream = new GZipStream(await blob.OpenReadAsync(), CompressionMode.Decompress);

            return entity;
        }

        public override TEntity FindFirstOnly()
        {
            throw new NotImplementedException();
        }

        public override bool Exists(dynamic key)
        {
            return GetContainer().GetBlockBlobReference(key).Exists();
        }

        public override bool ExistsBy(ISpecification<TEntity> specification)
        {
            throw new NotImplementedException();
        }

        public override void Insert(TEntity entity)
        {
            SaveBlob(entity);
        }

        public async override Task InsertAsync(TEntity entity)
        {
            await SaveBlobAsync(entity);
        }

        public override void Update(TEntity entity)
        {
            SaveBlob(entity);
        }

        public override async Task UpdateAsync(TEntity entity)
        {
            await SaveBlobAsync(entity);
        }

        public override void Delete(TEntity entity)
        {
            Delete(entity.Id);
        }

        public override void InsertOrUpdate(TEntity entity)
        {
            SaveBlob(entity);
        }

        public override void Delete(dynamic id)
        {
            GetContainer().GetBlockBlobReference(id).DeleteIfExists();
        }

        public override void Lock(dynamic id)
        {
            throw new NotImplementedException();
        }
    }
}