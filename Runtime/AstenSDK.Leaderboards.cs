using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        /// <summary>
        /// Registers or updates the active player's score on a global leaderboard.
        /// Backend automatically preserves the player's highest score.
        /// </summary>
        /// <param name="leaderboardId">Target leaderboard identifier in Asten web console.</param>
        /// <param name="score">Numeric score value achieved.</param>
        /// <param name="username">Optional custom display name for leaderboard ranking.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void SubmitScore(string leaderboardId, float score, string username, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                LogError("No active player session token. Call LoginPlayer() or LoginWithDeviceId() first.");
                callback?.Invoke(false, "No active player session token");
                return;
            }

            string payload = $"{{\"leaderboard_id\":\"{leaderboardId}\", \"score\":{score}, \"username\":\"{username}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/leaderboard/submit", payload, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Registers or updates the active player's score on a global leaderboard (without custom display name).
        /// </summary>
        /// <param name="leaderboardId">Target leaderboard identifier in Asten web console.</param>
        /// <param name="score">Numeric score value achieved.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void SubmitScore(string leaderboardId, float score, Action<bool, string> callback)
        {
            SubmitScore(leaderboardId, score, "", callback);
        }

        /// <summary>
        /// Queries top high scores for a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Target leaderboard identifier in Asten web console.</param>
        /// <param name="limit">Maximum number of entries to return (e.g. 10 or 50).</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void GetTopScores(string leaderboardId, int limit, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                LogError("No active player session token. Call LoginPlayer() or LoginWithDeviceId() first.");
                callback?.Invoke(false, "No active player session token");
                return;
            }

            string url = $"/player/leaderboard/top?leaderboard_id={leaderboardId}&limit={limit}";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }
    }
}
