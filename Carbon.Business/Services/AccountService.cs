using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Repositories;

namespace Carbon.Business.Services
{
    public class AccountService
    {
        private readonly IRepository<CompanyNameRegistry> _nameRepository;
        private readonly Regex _nameFilter = new Regex(@"[^a-zA-Z0-9\-_\.]", RegexOptions.Compiled);
        private readonly List<string> _defaultNames = new List<string> {"designer", "creator", "inventor"};

        public AccountService(IRepository<CompanyNameRegistry> nameRepository)
        {
            _nameRepository = nameRepository;
        }

        public async Task<string> RegisterCompanyName(string username, string email)
        {
            var candidates = new List<string>();

            TryAddNameCandidate(username, candidates);
            var emailPrefix = email.Substring(0, email.IndexOf('@'));
            TryAddNameCandidate(emailPrefix, candidates);

            var split = username.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                foreach (var s in split)
                {
                    TryAddNameCandidate(s, candidates);
                }
            }
            split = emailPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                foreach (var s in split)
                {
                    TryAddNameCandidate(s, candidates);
                }
            }

            return await RegisterCompanyName(candidates);
        }

        private async Task<string> RegisterCompanyName(IEnumerable<string> candidates)
        {
            var batch = candidates.Select(x => new CompanyNameRegistry(x)).ToList();
            if (batch.Count > 0)
            {
                var name = await TryRegisterCompanyName(batch);
                if (name != null)
                {
                    return name;
                }
                for (var i = 0; i < 10; ++i)
                {
                    batch = batch.Select(x => new CompanyNameRegistry(x.RowKey + new Random().Next(0, 1000))).ToList();
                    name = await TryRegisterCompanyName(batch);
                    if (name != null)
                    {
                        return name;
                    }
                }                
            }

            for (var i = 0; i < 50; ++i)
            {
                batch = _defaultNames.Select(x => new CompanyNameRegistry(x + new Random().Next(0, 1000))).ToList();
                var name = await TryRegisterCompanyName(batch);
                if (name != null)
                {
                    return name;
                }
            }

            throw new Exception("Could not register company name");
        }

        private async Task<string> TryRegisterCompanyName(IReadOnlyList<CompanyNameRegistry> batch)
        {
            var tasks = batch.Select(
                x => _nameRepository.FindSingleByAsync(new FindByRowKey<CompanyNameRegistry>(x.PartitionKey, x.RowKey)))
                .ToList();
            await Task.WhenAll(tasks);
            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (task.Result == null)
                {
                    try
                    {
                        await _nameRepository.InsertAsync(batch[i]);
                        return batch[i].RowKey;
                    }
                    catch (InsertConflictException)
                    {
                    }
                }
            }
            return null;
        }

        private void TryAddNameCandidate(string candidate, ICollection<string> candidates)
        {
            candidate = candidate.Trim().Replace(" ", ".");
            var normalized = _nameFilter.Replace(candidate, string.Empty);
            if (!string.IsNullOrEmpty(normalized))
            {
                candidates.Add(normalized.ToLowerInvariant());
            }
        }
    }
}
