using System;
using System.Collections;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {

        /// <summary>
        /// Retrieves the custom state (customData) of the active player.
        /// </summary>
        public void LoadPlayerData(Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No active player session token. Call LoginPlayer() first.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            string url = "/player";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Saves the player's custom state. Includes built-in Debouncing protection and token validation.
        /// </summary>
        public void SavePlayerData<T>(T dataObject, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No active player session token. Call LoginPlayer() first.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            // Convert the C# object or string to a JSON string
            string jsonPayload = (dataObject is string strJson) ? strJson : JsonUtility.ToJson(dataObject);
            string fullPayload = $"{{\"custom_data\":{jsonPayload}}}"; // _userId is no longer required in the payload

            // Apply Debounce protection (cooldown)
            float timeSinceLastSave = Time.time - _lastSaveTime;

            if (timeSinceLastSave >= _saveCooldown)
            {
                // Enough time has passed, save immediately
                ExecuteSave(fullPayload, callback);
            }
            else
            {
                // We are within the cooldown time, queue the save
                _pendingSaveData = fullPayload;
                _pendingSaveCallback = callback;

                if (_pendingSaveCoroutine != null)
                {
                    StopCoroutine(_pendingSaveCoroutine);
                }

                float delay = _saveCooldown - timeSinceLastSave;
                _pendingSaveCoroutine = StartCoroutine(ExecuteSaveDeferredCoroutine(delay));
                Debug.LogWarning($"[AstenSDK] Save request queued. Sending in {delay:F2} seconds to protect the server.");
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
