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
        private bool _isInitialized = false;
        public void Initialize(string gameId, string apiKey)
        {
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

            _gameId = gameId;
            _apiKey = apiKey;
            _isInitialized = true;

            Debug.Log($"[AstenSDK] Inicializado correctamente para el Juego ID: {_gameId}");
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
