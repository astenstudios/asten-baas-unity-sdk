using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AstenBaaS
{
    public class AstenSDK : MonoBehaviour
    {
        private static AstenSDK _instance;
        public static AstenSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AstenSDK_Runner");
                    _instance = go.AddComponent<AstenSDK>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private string _backendUrl = "http://localhost:8050"; // Cambiar por tu URL de producción en la nube
        private string _gameId;
        private string _apiKey;
        private string _activePlayerId;
        private string _playerSessionToken; // Almacena el JWT del jugador verificado

        // Variables para el control de guardados (Debounce / Cooldown)
        private float _saveCooldown = 3.0f; // Tiempo mínimo de espera entre peticiones de guardado (segundos)
        private float _lastSaveTime = -999f;
        private string _pendingSaveData;
        private Action<bool, string> _pendingSaveCallback;
        private Coroutine _pendingSaveCoroutine;

        /// <summary>
        /// Inicializa el SDK de Asten BaaS con las credenciales del juego.
        /// </summary>
        public void Initialize(string gameId, string apiKey, string backendUrl = null)
        {
            _gameId = gameId;
            _apiKey = apiKey;
            if (!string.IsNullOrEmpty(backendUrl))
            {
                _backendUrl = backendUrl.TrimEnd('/');
            }
            Debug.Log($"[AstenSDK] Inicializado para el Juego ID: {_gameId}");
        }

        /// <summary>
        /// Establece la sesión activa del jugador manualmente (usando ID y Token).
        /// </summary>
        public void SetPlayerSession(string playerId, string playerToken = null)
        {
            _activePlayerId = playerId;
            _playerSessionToken = playerToken;
        }

        #region AUTENTICACIÓN / REGISTRO DE JUGADORES

        /// <summary>
        /// Registra un nuevo usuario final (jugador) en el juego.
        /// </summary>
        public void RegisterPlayer(string email, string password, Action<bool, string> callback)
        {
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
            string payload = $"{{\"provider\":\"email\", \"email\":\"{email}\", \"password\":\"{password}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/auth", payload, _apiKey, null, (success, response) =>
            {
                if (success)

                {
                    // Extraer token e ID de jugador de la respuesta JSON de forma sencilla
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

                    Debug.Log($"[AstenSDK] Jugador autenticado exitosamente. ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma anónima a un jugador usando el ID de su dispositivo (Device ID).
        /// </summary>
        public void LoginWithDeviceId(string deviceId, Action<bool, string> callback)
        {
            string payload = $"{{\"provider\":\"device\", \"device_id\":\"{deviceId}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/auth", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    // Extraer token e ID de jugador de la respuesta JSON
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

                    Debug.Log($"[AstenSDK] Jugador autenticado por dispositivo exitosamente. ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma federada usando un ID Token de Google obtenido en el juego.
        /// </summary>
        public void LoginWithGoogle(string googleIdToken, Action<bool, string> callback)
        {
            string payload = $"{{\"provider\":\"google\", \"token\":\"{googleIdToken}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/auth", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    // Extraer token e ID de jugador de la respuesta JSON
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

                    Debug.Log($"[AstenSDK] Jugador autenticado por Google exitosamente. ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Inicia sesión (o registra automáticamente) de forma federada usando un Access Token de Discord.
        /// </summary>
        public void LoginWithDiscord(string discordAccessToken, Action<bool, string> callback)
        {
            string payload = $"{{\"provider\":\"discord\", \"token\":\"{discordAccessToken}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/auth", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    // Extraer token e ID de jugador de la respuesta JSON
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

                    string idSearch = "\"_id\":\"";
                    int idIndex = response.IndexOf(idSearch);
                    if (idIndex != -1)
                    {
                        int start = idIndex + idSearch.Length;
                        int end = response.IndexOf("\"", start);
                        if (end != -1)
                        {
                            _activePlayerId = response.Substring(start, end - start);
                        }
                    }

                    ExtractAndSetPlayerId(response);

                    Debug.Log($"[AstenSDK] Jugador autenticado por Discord exitosamente. ID: {_activePlayerId}");
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

        #endregion

        #region PERSISTENCIA DE DATOS (CLOUD SAVES)

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


        #endregion

        #region TABLAS DE CLASIFICACIÓN (LEADERBOARDS)

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

        #endregion

        #region COROUTINES DE RED (HTTP CLIENT)

        private IEnumerator GetRequestCoroutine(string endpoint, string apiKey, string playerToken, Action<bool, string> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(_backendUrl + endpoint))
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    webRequest.SetRequestHeader("X-API-Key", apiKey);
                }
                if (!string.IsNullOrEmpty(playerToken))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {playerToken}");
                }

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[AstenSDK] GET Error {webRequest.responseCode}: {webRequest.error}");
                    callback?.Invoke(false, webRequest.downloadHandler.text ?? webRequest.error);
                }
            }
        }

        private IEnumerator PostRequestCoroutine(string endpoint, string jsonJsonPayload, string apiKey, string playerToken, Action<bool, string> callback)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(_backendUrl + endpoint, "POST"))
            {
                byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonJsonPayload);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    webRequest.SetRequestHeader("X-API-Key", apiKey);
                }
                if (!string.IsNullOrEmpty(playerToken))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {playerToken}");
                }

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[AstenSDK] POST Error {webRequest.responseCode}: {webRequest.error}");
                    callback?.Invoke(false, webRequest.downloadHandler.text ?? webRequest.error);
                }
            }
        }

        #endregion

    }
}
