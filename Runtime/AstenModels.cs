using System;
using UnityEngine;

namespace AstenBaaS
{
    /// <summary>
    /// Authentication response payload returned by Asten BaaS backend.
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        public string message;
        public string token;
        public string id;
        public string _id;

        /// <summary>
        /// Gets the verified player ID regardless of field naming variation ('id' vs '_id').
        /// </summary>
        public string PlayerId => !string.IsNullOrEmpty(id) ? id : _id;
    }

    /// <summary>
    /// Leaderboard entry model representing a single score entry.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public float score;
        public string created_at;
    }

    /// <summary>
    /// Wrapper for list of leaderboard entries returned by top scores query.
    /// </summary>
    [Serializable]
    public class LeaderboardResponse
    {
        public LeaderboardEntry[] scores;
        public string leaderboard_id;
    }

    /// <summary>
    /// Standard player custom data container.
    /// </summary>
    [Serializable]
    public class PlayerDataResponse
    {
        public string custom_data;
        public string updated_at;
    }
}
