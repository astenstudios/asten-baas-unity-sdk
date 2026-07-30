using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {

        /// <summary>
        /// Registers or updates the player's score if it beats their previous mark.
        /// </summary>
        public void SubmitScore(string leaderboardId, float score, string username, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No active player session token. Call LoginPlayer() first.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            // Sanitize the JSON
            string payload = $"{{\"leaderboard_id\":\"{leaderboardId}\", \"score\":{score}, \"username\":\"{username}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/leaderboard/submit", payload, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Registers or updates the player's score on a leaderboard (without custom username).
        /// </summary>
        public void SubmitScore(string leaderboardId, float score, Action<bool, string> callback)
        {
            SubmitScore(leaderboardId, score, "", callback);
        }

        /// <summary>
        /// Gets the top scores ranking for a leaderboard.
        /// </summary>
        public void GetTopScores(string leaderboardId, int limit, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No active player session token. Call LoginPlayer() first.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            string url = $"/player/leaderboard/top?leaderboard_id={leaderboardId}&limit={limit}";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }
    }
}
