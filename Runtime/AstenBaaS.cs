using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AstenBaaS
{
    /// <summary>
    /// Core singleton class for integrating Unity games with Asten BaaS backend service.
    /// Provides Authentication, Cloud Saves, and Leaderboards.
    /// </summary>
    [AddComponentMenu("Asten BaaS/Asten SDK")]
    [DisallowMultipleComponent]
    public partial class AstenSDK : MonoBehaviour
    {
        private static AstenSDK _instance;

        /// <summary>
        /// Global singleton instance of the Asten BaaS SDK runner.
        /// </summary>
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
            EnableDebugLogs = true;
        }

        /// <summary>
        /// Toggle to enable or disable verbose SDK log output in Unity Console.
        /// </summary>
        public static bool EnableDebugLogs { get; set; } = true;

        private string _backendUrl = "https://api.baas.astenstudios.com";
        private string _gameId;
        private string _apiKey;
        private string _activePlayerId;
        private string _playerSessionToken;

        /// <summary>
        /// Gets the active player's unique backend identifier.
        /// </summary>
        public string ActivePlayerId => _activePlayerId;

        /// <summary>
        /// Gets the active player's JWT session token.
        /// </summary>
        public string PlayerSessionToken => _playerSessionToken;

        /// <summary>
        /// Returns true if a player session (token or ID) is currently active.
        /// </summary>
        public bool IsLoggedIn => !string.IsNullOrEmpty(_playerSessionToken) || !string.IsNullOrEmpty(_activePlayerId);

        /// <summary>
        /// Returns true if the SDK has been initialized with credentials.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        // Save debounce / cooldown configuration
        private float _saveCooldown = 3.0f;
        private float _lastSaveTime = -999f;
        private string _pendingSaveData;
        private Action<bool, string> _pendingSaveCallback;
        private Coroutine _pendingSaveCoroutine;

        private bool _isInitialized = false;

        /// <summary>
        /// Initializes the Asten BaaS SDK with your game credentials from the Asten web console.
        /// </summary>
        /// <param name="gameId">Your Game ID (UUID string).</param>
        /// <param name="apiKey">Your secret API key.</param>
        /// <param name="backendUrl">Optional custom server endpoint URL.</param>
        public void Initialize(string gameId, string apiKey, string backendUrl = null)
        {
            if (_isInitialized)
            {
                LogWarning("The SDK is already initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(gameId))
            {
                LogError("Initialization error: 'gameId' is null or empty.");
                throw new ArgumentException("The gameId is required.", nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                LogError("Initialization error: 'apiKey' is null or empty.");
                throw new ArgumentException("The apiKey is required.", nameof(apiKey));
            }

            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                _backendUrl = backendUrl.TrimEnd('/');
            }

            _gameId = gameId;
            _apiKey = apiKey;
            _isInitialized = true;

            string maskedKey = apiKey.Length > 8 ? apiKey.Substring(0, 8) + "..." : "***";
            Log($"Initialized successfully for Game ID: {_gameId} (Key: {maskedKey}) | Server: {_backendUrl}");
        }

        /// <summary>
        /// Manually sets the active player session token and ID.
        /// </summary>
        /// <param name="playerId">The player's backend identifier.</param>
        /// <param name="playerToken">The player's JWT authentication token.</param>
        public void SetPlayerSession(string playerId, string playerToken = null)
        {
            _activePlayerId = playerId;
            _playerSessionToken = playerToken;
        }

        #region Internal Logging Helpers

        internal static void Log(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[AstenSDK] {message}");
        }

        internal static void LogWarning(string message)
        {
            if (EnableDebugLogs)
                Debug.LogWarning($"[AstenSDK] {message}");
        }

        internal static void LogError(string message)
        {
            if (EnableDebugLogs)
                Debug.LogError($"[AstenSDK] {message}");
        }

        #endregion
    }
}
