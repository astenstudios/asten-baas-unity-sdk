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

        private string _backendUrl = "https://api.baas.astenstudios.com"; // Default production URL (configurable in Initialize)
        private string _gameId;
        private string _apiKey;
        private string _activePlayerId;
        private string _playerSessionToken; // Stores the verified player's JWT

        public string ActivePlayerId => _activePlayerId;
        public string PlayerSessionToken => _playerSessionToken;
        public bool IsLoggedIn => !string.IsNullOrEmpty(_playerSessionToken) || !string.IsNullOrEmpty(_activePlayerId);

        // Variables for save control (Debounce / Cooldown)
        private float _saveCooldown = 3.0f; // Minimum wait time between save requests (seconds)
        private float _lastSaveTime = -999f;
        private string _pendingSaveData;
        private Action<bool, string> _pendingSaveCallback;
        private Coroutine _pendingSaveCoroutine;

        /// <summary>
        /// Initializes the Asten BaaS SDK with the game credentials.
        /// </summary>
        private bool _isInitialized = false;
        public void Initialize(string gameId, string apiKey, string backendUrl = null)
        {
            Debug.Log("[AstenSDK] Initializing Asten BaaS SDK...");
            Debug.Log($"[AstenSDK] Game ID: {gameId}");
            Debug.Log($"[AstenSDK] API Key: {apiKey}");
            if (_isInitialized)
            {
                Debug.LogWarning("[AstenSDK] The SDK is already initialized.");
                return;
            }

            // 1. Game ID validation
            if (string.IsNullOrWhiteSpace(gameId))
            {
                Debug.LogError("[AstenSDK] Initialization error: 'gameId' is null or empty.");
                throw new ArgumentException("The gameId is required.", nameof(gameId));
            }

            // 2. API Key validation
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError("[AstenSDK] Initialization error: 'apiKey' is null or empty.");
                throw new ArgumentException("The apiKey is required.", nameof(apiKey));
            }

            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                _backendUrl = backendUrl.TrimEnd('/');
            }

            _gameId = gameId;
            _apiKey = apiKey;
            _isInitialized = true;

            Debug.Log($"[AstenSDK] Initialized successfully for Game ID: {_gameId} | Server: {_backendUrl}");
        }


        /// <summary>
        /// Manually sets the active player session (using ID and Token).
        /// </summary>
        public void SetPlayerSession(string playerId, string playerToken = null)
        {
            _activePlayerId = playerId;
            _playerSessionToken = playerToken;
        }

    }
}
