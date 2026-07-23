using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {

        /// <summary>
        /// Registra o actualiza la puntuación del jugador si supera su marca previa.
        /// </summary>
        public void SubmitScore(string leaderboardId, float score, string username, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No hay un token de sesión de jugador activo. Llama a LoginPlayer() primero.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            // Sanitizar el JSON
            string payload = $"{{\"leaderboard_id\":\"{leaderboardId}\", \"score\":{score}, \"username\":\"{username}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/leaderboard/submit", payload, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Obtiene el ranking de mejores puntuaciones para una tabla de clasificación.
        /// </summary>
        public void GetTopScores(string leaderboardId, int limit, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No hay un token de sesión de jugador activo. Llama a LoginPlayer() primero.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            string url = $"/player/leaderboard/top?leaderboard_id={leaderboardId}&limit={limit}";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }
    }
}
