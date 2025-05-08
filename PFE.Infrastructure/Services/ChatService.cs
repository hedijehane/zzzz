using PFE.Application.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PFE.Infrastructure.Services
{
    public class UserConnectionManager : IUserConnectionManager
    {
        // Thread-safe dictionary to store user connections
        private readonly ConcurrentDictionary<int, List<string>> _userConnectionMap;

        public UserConnectionManager()
        {
            _userConnectionMap = new ConcurrentDictionary<int, List<string>>();
        }

        public void AddConnection(int userId, string connectionId)
        {
            // Get or create a list of connections for this user
            _userConnectionMap.AddOrUpdate(
                userId,
                // If key doesn't exist, create a new list with this connection
                new List<string> { connectionId },
                // If key exists, add this connection to the existing list
                (key, existingList) =>
                {
                    lock (existingList)
                    {
                        if (!existingList.Contains(connectionId))
                        {
                            existingList.Add(connectionId);
                        }
                        return existingList;
                    }
                });
        }

        public void RemoveConnection(int userId, string connectionId)
        {
            // Check if user has any connections
            if (_userConnectionMap.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    // Remove this specific connection
                    connections.Remove(connectionId);

                    // If no more connections, remove the user entirely
                    if (connections.Count == 0)
                    {
                        _userConnectionMap.TryRemove(userId, out _);
                    }
                }
            }
        }

        public List<string> GetConnections(int userId)
        {
            // Return connections for this user if any exist
            if (_userConnectionMap.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList(); // Return a copy to avoid modification issues
                }
            }

            return new List<string>();
        }
    }
}