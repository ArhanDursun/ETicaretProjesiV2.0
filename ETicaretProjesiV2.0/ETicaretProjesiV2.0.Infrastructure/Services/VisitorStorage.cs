using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Services
{
    public class VisitorStorage
    {
        private readonly ConcurrentDictionary<string, int> _activeConnections = new();

        public void AddOrUpdateUser(string userId)
        {
            _activeConnections.AddOrUpdate(userId, 1, (key, count) => count + 1);
        }
        public void RemoveUser(string userId)
        {
            if (_activeConnections.TryGetValue(userId, out int count))
            {
                if (count <= 1)
                {
                    
                    _activeConnections.TryRemove(userId, out _);
                }
                else
                {
                    
                    _activeConnections.TryUpdate(userId, count - 1, count);
                }
            }
        }
        public void ForceRemoveUser(string userId)
        {
            _activeConnections.TryRemove(userId,out _);
        }
        public List<string> GetOnlineUserIds()
        {
            return _activeConnections.Keys.ToList();
        }
    }
}
