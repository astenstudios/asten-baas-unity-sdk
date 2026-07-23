using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        /// <summary>
        /// Registra un nuevo usuario final (jugador) en el juego.
        /// </summary>
        public void RegisterPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes registrar un jugador sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.LogError("[AstenSDK] El email y la contraseña son obligatorios.");
                callback?.Invoke(false, "Email or password cannot be empty");
                return;
            }

            string payload = $"{{\"email\":\"{email}\", \"password\":\"{password}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/register", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    Debug.Log($"[AstenSDK] Jugador registrado con éxito.");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Inicia sesión de un jugador bajo el contexto del juego y guarda el Player JWT.
        /// </summary>
        public void LoginPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes iniciar sesión sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                Debug.LogError("[AstenSDK] El email no puede estar vacío.");
                callback?.Invoke(false, "Email cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                Debug.LogError("[AstenSDK] La contraseña no puede estar vacía.");
                callback?.Invoke(false, "Password cannot be empty");
                return;
            }

            string payload = $"{{\"provider\":\"email\", \"email\":\"{email}\", \"password\":\"{password}\"}}";
            ExecuteAuthRequest(payload, "email", callback);
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma anónima a un jugador usando el ID de su dispositivo (Device ID).
        /// </summary>
        public void LoginWithDeviceId(Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes iniciar sesión sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }

            // Usamos una variable local si deviceId no era provisto como parámetro en la versión que movimos
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            string payload = $"{{\"provider\":\"device\", \"device_id\":\"{deviceId}\"}}";
            ExecuteAuthRequest(payload, "dispositivo", callback);
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma federada usando un ID Token de Google obtenido en el juego.
        /// </summary>
        public void LoginWithGoogle(string googleIdToken, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes iniciar sesión sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(googleIdToken))
            {
                Debug.LogError("[AstenSDK] El token de Google no puede estar vacío.");
                callback?.Invoke(false, "Google token cannot be empty");
                return;
            }

            string payload = $"{{\"provider\":\"google\", \"token\":\"{googleIdToken}\"}}";
            ExecuteAuthRequest(payload, "Google", callback);
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma federada usando un Access Token de Discord.
        /// </summary>
        public void LoginWithDiscord(string discordAccessToken, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes iniciar sesión sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(discordAccessToken))
            {
                Debug.LogError("[AstenSDK] El token de Discord no puede estar vacío.");
                callback?.Invoke(false, "Discord token cannot be empty");
                return;
            }

            string payload = $"{{\"provider\":\"discord\", \"token\":\"{discordAccessToken}\"}}";
            ExecuteAuthRequest(payload, "Discord", callback);
        }

        private void ExecuteAuthRequest(string payload, string providerName, Action<bool, string> callback)
        {
            StartCoroutine(PostRequestCoroutine("/player/auth", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    // Extraer token de jugador
                    string tokenSearch = "\"token\":\"";
                    int tokenIndex = response.IndexOf(tokenSearch);
                    if (tokenIndex != -1)
                    {
                        int start = tokenIndex + tokenSearch.Length;
                        int end = response.IndexOf("\"", start);
                        if (end != -1)
                        {
                            _playerSessionToken = response.Substring(start, end - start);
                        }
                    }

                    // Extraer ID de jugador
                    ExtractAndSetPlayerId(response);

                    Debug.Log($"[AstenSDK] Jugador autenticado exitosamente por {providerName}. ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
        }

        private void ExtractAndSetPlayerId(string response)
        {
            string[] idKeys = new string[] { "\"id\":\"", "\"_id\":\"" };
            foreach (var key in idKeys)
            {
                int index = response.IndexOf(key);
                if (index != -1)
                {
                    int start = index + key.Length;
                    int end = response.IndexOf("\"", start);
                    if (end != -1)
                    {
                        _activePlayerId = response.Substring(start, end - start);
                        return;
                    }
                }
            }
        }
    }
}
