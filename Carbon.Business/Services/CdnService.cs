using Carbon.Business.CloudDomain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Extensions;
using Carbon.Framework.Repositories;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Carbon.Business.Services
{
    public class CdnService
    {
        private readonly HttpClient _httpClient;
        private readonly IRepository<File> _fileRepository;
        private readonly AppSettings _appSettings;
        private readonly Uri _cdnEndpoint;

        public CdnService(AppSettings appSettings, IRepository<File> fileRepository)
        {
            _httpClient = new HttpClient();
            _fileRepository = fileRepository;
            _appSettings = appSettings;
            _cdnEndpoint = appSettings.Endpoints.Cdn;
        }

        public async Task<string> UploadImage(File file, string imageData)
        {
            if (imageData == null)
            {
                throw new ArgumentNullException(nameof(imageData));
            }

            Uri uri;
            if (!imageData.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(imageData, UriKind.RelativeOrAbsolute, out uri))
            {
                return await UploadFileFromUri(file, uri);
            }

            var split = imageData.Split(',');
            if (split.Length != 2)
            {
                throw new ArgumentException(nameof(imageData));
            }

            file.SetContent(Convert.FromBase64String(split[1]));
            await _fileRepository.InsertAsync(file);

            return CdnUrl(file);
        }

        public async Task<string> UploadFile(File file, byte[] content)
        {
            file.SetContent(content);
            await _fileRepository.InsertAsync(file);
            return CdnUrl(file);
        }

        public async Task<string> UploadFile(File file, string content)
        {
            file.SetContent(content);
            await _fileRepository.InsertAsync(file);
            return CdnUrl(file);
        }

        private async Task<string> UploadFileFromUri(File file, Uri uri)
        {
            var uriToCheck = uri;
            if (!uriToCheck.IsAbsoluteUri && uriToCheck.OriginalString.StartsWith("//"))
            {
                uriToCheck = new Uri("http:" + uriToCheck.OriginalString);
            }
            if (uriToCheck.Host.Equals(_cdnEndpoint.Host, StringComparison.OrdinalIgnoreCase))
            {
                return uriToCheck.WithoutScheme();
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new BadUrlException(uri.OriginalString);
            }

            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified)
            {
                throw new BadUrlException(uri.OriginalString);
            }

            file.ContentStream = await response.Content.ReadAsStreamAsync();
            await _fileRepository.InsertAsync(file);

            return CdnUrl(file);
        }

        private string CdnUrl(File file)
        {
            return _cdnEndpoint.AddPath(File.ContainerName).AddPath(file.Id).WithoutScheme();
        }
    }
}
