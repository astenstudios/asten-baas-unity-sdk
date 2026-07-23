using System;
using System.Collections;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {

        /// <summary>
        /// Recupera el estado personalizado (customData) del jugador activo.
        /// </summary>
        public void LoadPlayerData(Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No hay un token de sesión de jugador activo. Llama a LoginPlayer() primero.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            string url = "/player";
            StartCoroutine(GetRequestCoroutine(url, _apiKey, _playerSessionToken, callback));
        }

        /// <summary>
        /// Guarda el estado personalizado del jugador. Incluye protección de Debouncing integrada y validación de token.
        /// </summary>
        public void SavePlayerData<T>(T dataObject, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_playerSessionToken))
            {
                Debug.LogError("[AstenSDK] No hay un token de sesión de jugador activo. Llama a LoginPlayer() primero.");
                callback?.Invoke(false, "No active player token session");
                return;
            }

            // Convertimos el objeto C# a cadena JSON
            string jsonPayload = JsonUtility.ToJson(dataObject);
            string fullPayload = $"{{\"custom_data\":{jsonPayload}}}"; // Ya no se requiere _userId en el payload

            // Aplicamos protección Debounce (cooldown)
            float timeSinceLastSave = Time.time - _lastSaveTime;

            if (timeSinceLastSave >= _saveCooldown)
            {
                // Ha pasado suficiente tiempo, guardamos inmediatamente
                ExecuteSave(fullPayload, callback);
            }
            else
            {
                // Estamos dentro del tiempo de cooldown, encolamos el guardado
                _pendingSaveData = fullPayload;
                _pendingSaveCallback = callback;

                if (_pendingSaveCoroutine != null)
                {
                    StopCoroutine(_pendingSaveCoroutine);
                }

                float delay = _saveCooldown - timeSinceLastSave;
                _pendingSaveCoroutine = StartCoroutine(ExecuteSaveDeferredCoroutine(delay));
                Debug.LogWarning($"[AstenSDK] Petición de guardado encolada. Enviando en {delay:F2} segundos para proteger el servidor.");
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
