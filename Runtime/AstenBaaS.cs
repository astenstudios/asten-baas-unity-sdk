using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AstenBaaS
{
    public partial class AstenSDK : MonoBehaviour
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

        private string _backendUrl = "https://api.baas.astenstudios.com"; // URL de producción por defecto (configurable en Initialize)
        private string _gameId;
        private string _apiKey;
        private string _activePlayerId;
        private string _playerSessionToken; // Almacena el JWT del jugador verificado

        public string ActivePlayerId => _activePlayerId;
        public string PlayerSessionToken => _playerSessionToken;
        public bool IsLoggedIn => !string.IsNullOrEmpty(_playerSessionToken) || !string.IsNullOrEmpty(_activePlayerId);

        // Variables para el control de guardados (Debounce / Cooldown)
        private float _saveCooldown = 3.0f; // Tiempo mínimo de espera entre peticiones de guardado (segundos)
        private float _lastSaveTime = -999f;
        private string _pendingSaveData;
        private Action<bool, string> _pendingSaveCallback;
        private Coroutine _pendingSaveCoroutine;

        /// <summary>
        /// Inicializa el SDK de Asten BaaS con las credenciales del juego.
        /// </summary>
        private bool _isInitialized = false;
        public void Initialize(string gameId, string apiKey, string backendUrl = null)
        {
            Debug.Log("[AstenSDK] Inicializando Asten BaaS SDK...");
            Debug.Log($"[AstenSDK] Game ID: {gameId}");
            Debug.Log($"[AstenSDK] API Key: {apiKey}");
            if (_isInitialized)
            {
                Debug.LogWarning("[AstenSDK] El SDK ya se encuentra inicializado.");
                return;
            }

            // 1. Validación de Game ID
            if (string.IsNullOrWhiteSpace(gameId))
            {
                Debug.LogError("[AstenSDK] Error de inicialización: 'gameId' es nulo o está vacío.");
                throw new ArgumentException("El gameId es obligatorio.", nameof(gameId));
            }

            // 2. Validación de API Key
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError("[AstenSDK] Error de inicialización: 'apiKey' es nula o está vacía.");
                throw new ArgumentException("El apiKey es obligatorio.", nameof(apiKey));
            }

            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                _backendUrl = backendUrl.TrimEnd('/');
            }

            _gameId = gameId;
            _apiKey = apiKey;
            _isInitialized = true;

            Debug.Log($"[AstenSDK] Inicializado correctamente para el Juego ID: {_gameId} | Servidor: {_backendUrl}");
        }


        /// <summary>
        /// Establece la sesión activa del jugador manualmente (usando ID y Token).
        /// </summary>
        public void SetPlayerSession(string playerId, string playerToken = null)
        {
            _activePlayerId = playerId;
            _playerSessionToken = playerToken;
        }

    }
}
