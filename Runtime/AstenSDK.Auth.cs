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
            Debug.Log($" [AstenSDK] Iniciando sesión con payload: {payload}");
            ExecuteAuthRequest(payload, "dispositivo", callback);
        }

        /// <summary>
        /// Verifica el correo electrónico del jugador utilizando el código OTP de 6 dígitos enviado a su bandeja de entrada.
        /// </summary>
        public void VerifyPlayerEmail(string email, string otpCode, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] No puedes verificar el correo sin haber llamado a Initialize() primero.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                Debug.LogError("[AstenSDK] El email y el código OTP son obligatorios.");
                callback?.Invoke(false, "Email and OTP cannot be empty");
                return;
            }

            string payload = $"{{\"email\":\"{email}\", \"otp\":\"{otpCode}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/verify-otp", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
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

                    ExtractAndSetPlayerId(response);
                    Debug.Log($"[AstenSDK] ✅ ¡Correo verificado exitosamente con código OTP! Sesión activa para ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
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

        /// <summary>
        /// Cierra la sesión activa del jugador, limpiando el token JWT y el ID en memoria.
        /// </summary>
        public void Logout()
        {
            _activePlayerId = null;
            _playerSessionToken = null;
            Debug.Log("[AstenSDK] 🚪 Sesión del jugador cerrada exitosamente.");
        }

        /// <summary>
        /// Cierra la sesión activa del jugador (alias de Logout para consistencia de nomenclatura).
        /// </summary>
        public void LogoutPlayer()
        {
            Logout();
        }
    }
}
