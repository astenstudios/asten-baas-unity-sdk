using System;
using System.Collections;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        /// <summary>
        /// Retrieves the stored custom state (customData) of the currently authenticated player.
        /// </summary>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void LoadPlayerData(Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                LogError("No active player session token. Call LoginPlayer() or LoginWithDeviceId() first.");
                callback?.Invoke(false, "No active player session token");
                return;
            }

            string url = "/player";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Saves the player's custom state to cloud database.
        /// Features built-in Debounce protection (3-second rate limit cooldown) to optimize performance.
        /// </summary>
        /// <typeparam name="T">Class or struct type serializable via JsonUtility (or raw JSON string).</typeparam>
        /// <param name="dataObject">Object instance or raw JSON string to persist.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void SavePlayerData<T>(T dataObject, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                LogError("No active player session token. Call LoginPlayer() or LoginWithDeviceId() first.");
                callback?.Invoke(false, "No active player session token");
                return;
            }

            string jsonPayload = (dataObject is string strJson) ? strJson : JsonUtility.ToJson(dataObject);
            string fullPayload = $"{{\"custom_data\":{jsonPayload}}}";

            float timeSinceLastSave = Time.time - _lastSaveTime;

            if (timeSinceLastSave >= _saveCooldown)
            {
                ExecuteSave(fullPayload, callback);
            }
            else
            {
                _pendingSaveData = fullPayload;
                _pendingSaveCallback = callback;

                if (_pendingSaveCoroutine != null)
                {
                    StopCoroutine(_pendingSaveCoroutine);
                }

                float delay = _saveCooldown - timeSinceLastSave;
                _pendingSaveCoroutine = StartCoroutine(ExecuteSaveDeferredCoroutine(delay));
                LogWarning($"Save request queued. Sending in {delay:F2} seconds to protect server bandwidth.");
            }
        }

        private void ExecuteSave(string payload, Action<bool, string> callback)
        {
            _lastSaveTime = Time.time;
            StartCoroutine(PostRequestCoroutine("/player/data", payload, _apiKey, _playerSessionToken, callback));
        }

        private IEnumerator ExecuteSaveDeferredCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!string.IsNullOrEmpty(_pendingSaveData))
            {
                ExecuteSave(_pendingSaveData, _pendingSaveCallback);
                _pendingSaveData = null;
                _pendingSaveCallback = null;
            }
        }
    }
}
