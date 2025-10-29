using ERP.Infrastructure.Models.DTOs;
using System.Collections.Concurrent;
using System.Security;

namespace ERP.Infrastructure.Helpers
{
    public class PermissionCache
    {
        //private readonly Dictionary<string, HashSet<string>> _cache
        //    = new Dictionary<string, HashSet<string>>();
        private readonly ConcurrentDictionary<string, (PermissionDto Permission, DateTime Expiry)> _cache
           = new ConcurrentDictionary<string, (PermissionDto, DateTime)>();

        public bool TryGet(string key, out PermissionDto? permission)
        {
            permission = null;
            if (_cache.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow < entry.Expiry)
                {
                    permission = entry.Permission;
                    return true;
                }

                // expired → remove it
                _cache.TryRemove(key, out _);
            }
            return false;
        }

        public void Set(string key, PermissionDto permission, TimeSpan duration)
        {
            var expiry = DateTime.UtcNow.Add(duration);
            _cache[key] = (permission, expiry);
        }
        public void Clear(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void ClearAll()
        {
            _cache.Clear();
        }

        public void ClearByUser(int userId)
        {
            var prefix = $"{userId}:";
            var keys = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();

            foreach (var key in keys)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }
}
