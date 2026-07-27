using UnityEngine;
using AstenBaaS;

namespace AstenBaaS.Samples
{
    /// <summary>
    /// Demostración rápida (Quickstart) orientada 100% a código utilizando OnGUI.
    /// No requiere configuración manual de Canvas, TextMeshPro o prefabs de UI.
    /// Funciona con solo presionar Play en Unity Editor y cubre todos los flujos de autenticación.
    /// </summary>
    public class AstenQuickstartDemo : MonoBehaviour
    {
        [Header("Configuración de Asten BaaS")]
        [Tooltip("ID de tu juego en la consola Asten (Sandbox/Demo por defecto)")]
        public string gameId = "";
        
        [Tooltip("API Key de tu entorno Sandbox o Producción")]
        public string apiKey = "";

        // Modo de autenticación en la UI
        private int _authTab = 0; // 0: Invitado (Device ID), 1: Correo & OTP
        private int _viewTab = 0; // 0: Perfil Visual, 1: Top Leaderboard, 2: JSON Técnico
        private string _leaderboardSummary = "💡 <i>Haz clic en 'Consultar Top 5' o 'Publicar Récord' para descargar el ranking global en tiempo real.</i>";
        private string _emailInput = "test@email.com";
        private string _passwordInput = "";
        private string _otpInput = "";

        // Estado del SDK
        private string _statusMessage = "🟡 SDK Desconectado. Selecciona un método de autenticación.";
        private bool _isLoggedIn = false;
        private string _activeProvider = "Ninguno";

        // Datos de ejemplo en memoria
        private int _playerCoins = 1000;
        private int _playerLevel = 1;
        private string _lastServerResponse = "Ninguna petición enviada todavía.";

        void Start()
        {
            // Inicializamos el SDK al arrancar
            AstenSDK.Instance.Initialize(gameId, apiKey);
            _statusMessage = "🟢 SDK Inicializado. Elige cómo deseas entrar:";
        }

        #region Flujos de Autenticación

        private void LoginGuest()
        {
            _statusMessage = "🔄 Conectando como Invitado (Device ID)...";
            _lastServerResponse = "Enviando petición a /player/auth...";

            AstenSDK.Instance.LoginWithDeviceId((success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Device ID (Guest)";
                    _statusMessage = "🟢 Autenticado con éxito (Guest / Device ID). Sincronizando nube...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] Autenticación de invitado completada con éxito.");
                }
                else
                {
                    _isLoggedIn = false;
                    _statusMessage = "🔴 Error al conectar como invitado.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Error en LoginGuest: {response}");
                }
            });
        }

        private void RegisterWithEmail()
        {
            _statusMessage = "🔄 Registrando jugador y solicitando código OTP...";
            _lastServerResponse = $"Registrando {_emailInput} en el servidor...";

            AstenSDK.Instance.RegisterPlayer(_emailInput, _passwordInput, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = "📬 ¡Registro exitoso! Revisa tu correo o usa el OTP maestro '123456' en Sandbox.";
                    _lastServerResponse = response;
                    Debug.Log("📬 [AstenQuickstart] Registro de correo completado. Esperando validación OTP.");
                }
                else
                {
                    _statusMessage = "🔴 Error al registrar cuenta por correo.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Error en Registro: {response}");
                }
            });
        }

        private void VerifyEmailOTP()
        {
            _statusMessage = "🔄 Verificando código OTP de 6 dígitos...";
            _lastServerResponse = $"Verificando OTP '{_otpInput}' para {_emailInput}...";

            AstenSDK.Instance.VerifyPlayerEmail(_emailInput, _otpInput, (success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Correo Verificado";
                    _statusMessage = "🟢 ¡Correo verificado y sesión activa! Sincronizando nube...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] Verificación OTP exitosa y sesión activa.");
                }
                else
                {
                    _statusMessage = "🔴 Código OTP incorrecto o expirado.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Error en Verificación OTP: {response}");
                }
            });
        }

        private void LoginWithEmail()
        {
            _statusMessage = "🔄 Iniciando sesión con correo...";
            _lastServerResponse = $"Autenticando {_emailInput}...";

            AstenSDK.Instance.LoginPlayer(_emailInput, _passwordInput, (success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Correo Electrónico";
                    _statusMessage = "🟢 Inicio de sesión exitoso. Sincronizando nube...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] Sesión de correo iniciada exitosamente.");
                }
                else
                {
                    _isLoggedIn = false;
                    _statusMessage = "🔴 Error en login: Credenciales inválidas o correo no verificado.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Error en Login de correo: {response}");
                }
            });
        }

        private void Logout()
        {
            AstenSDK.Instance.Logout();
            _isLoggedIn = false;
            _activeProvider = "Ninguno";
            _playerCoins = 0;
            _playerLevel = 1;
            _leaderboardSummary = "💡 <i>Haz clic en 'Consultar Top 5' o 'Publicar Récord' para descargar el ranking global en tiempo real.</i>";
            _statusMessage = "🟡 Sesión cerrada. Selecciona un método para entrar.";
            _lastServerResponse = "Sesión cerrada localmente.";
        }

        #endregion

        #region Progresión y Datos en Nube

        [System.Serializable]
        public class SampleSaveData
        {
            public int coins;
            public int level;
            public string weapon;
            public string timestamp;
        }

        private void SaveSampleProgress()
        {
            if (!_isLoggedIn) return;

            _statusMessage = "🔄 Guardando partida en la nube...";
            _playerCoins += 250;
            _playerLevel++;

            SampleSaveData progressData = new SampleSaveData
            {
                coins = _playerCoins,
                level = _playerLevel,
                weapon = "Espada de Acero",
                timestamp = System.DateTime.UtcNow.ToString("o")
            };

            AstenSDK.Instance.SavePlayerData(progressData, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = $"✅ Progreso guardado! Nivel: {_playerLevel} | Monedas: {_playerCoins}";
                    _lastServerResponse = response;
                    Debug.Log($"✅ [AstenQuickstart] Partida guardada en MongoDB Atlas: {response}");
                }
                else
                {
                    _statusMessage = "🔴 Error al guardar partida en el servidor.";
                    _lastServerResponse = response;
                }
            });
        }

        private void LoadSampleProgress()
        {
            if (!_isLoggedIn) return;

            _statusMessage = "🔄 Descargando partida desde la nube...";
            AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
            {
                if (success)
                {
                    _statusMessage = "✅ Datos descargados correctly desde el servidor!";
                    _lastServerResponse = jsonResponse;
                    Debug.Log($"✅ [AstenQuickstart] Partida obtenida: {jsonResponse}");

                    // Extraer monedas y nivel reales del jugador para sincronizar la interfaz
                    try
                    {
                        if (jsonResponse.Contains("\"coins\":"))
                        {
                            int idx = jsonResponse.IndexOf("\"coins\":") + 8;
                            string numStr = "";
                            while (idx < jsonResponse.Length && (char.IsDigit(jsonResponse[idx]) || jsonResponse[idx] == '-' || jsonResponse[idx] == ' '))
                            {
                                if (char.IsDigit(jsonResponse[idx]) || jsonResponse[idx] == '-') numStr += jsonResponse[idx];
                                idx++;
                            }
                            if (int.TryParse(numStr, out int parsedCoins)) _playerCoins = parsedCoins;
                        }
                        if (jsonResponse.Contains("\"level\":"))
                        {
                            int idx = jsonResponse.IndexOf("\"level\":") + 8;
                            string numStr = "";
                            while (idx < jsonResponse.Length && (char.IsDigit(jsonResponse[idx]) || jsonResponse[idx] == '-' || jsonResponse[idx] == ' '))
                            {
                                if (char.IsDigit(jsonResponse[idx]) || jsonResponse[idx] == '-') numStr += jsonResponse[idx];
                                idx++;
                            }
                            if (int.TryParse(numStr, out int parsedLevel)) _playerLevel = parsedLevel;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[AstenQuickstart] No se pudo parsear saldo de monedas/nivel del JSON: {ex.Message}");
                    }
                }
                else
                {
                    _statusMessage = "🔴 Error al obtener datos de la nube.";
                    _lastServerResponse = jsonResponse;
                }
            });
        }

        private void SubmitSampleScore()
        {
            if (!_isLoggedIn) return;

            int randomScore = Random.Range(1000, 9999);
            _statusMessage = $"🔄 Enviando puntaje ({randomScore}) a la tabla global...";

            AstenSDK.Instance.SubmitScore("leaderboard_score", randomScore, "Player_" + randomScore, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = $"🏆 Récord de {randomScore} puntos publicado! Descargando tabla Top 5...";
                    _lastServerResponse = response;
                    Debug.Log($"🏆 [AstenQuickstart] Récord publicado en Leaderboards: {response}");
                    GetSampleLeaderboard(); // Sincronizar y cambiar a pestaña de Top 5
                }
                else
                {
                    _statusMessage = "🔴 Error al publicar en Leaderboard.";
                    _lastServerResponse = response;
                }
            });
        }

        private void GetSampleLeaderboard()
        {
            if (!_isLoggedIn) return;

            _statusMessage = "🔄 Consultando Top 5 del Leaderboard...";
            _leaderboardSummary = "🔄 <i>Descargando ranking desde MongoDB Atlas...</i>";
            AstenSDK.Instance.GetTopScores("leaderboard_score", 5, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = "🏆 ¡Ranking Top 5 descargado con éxito desde la nube!";
                    _lastServerResponse = response;
                    ParseAndFormatLeaderboard(response);
                    _viewTab = 1; // Cambiar automáticamente a la pestaña del Top 5
                    Debug.Log($"🏆 [AstenQuickstart] Ranking obtenido: {response}");
                }
                else
                {
                    _statusMessage = "🔴 Error al consultar Leaderboard.";
                    _lastServerResponse = response;
                }
            });
        }

        private void ParseAndFormatLeaderboard(string json)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<b>🏆 TOP 5 RANKING GLOBAL (En Vivo):</b>");
                
                int count = 0;
                int searchIndex = 0;
                while (count < 5)
                {
                    int scoreIdx = json.IndexOf("\"score\":", searchIndex);
                    if (scoreIdx == -1) break;
                    
                    int scoreEnd = json.IndexOf(",", scoreIdx + 8);
                    if (scoreEnd == -1) scoreEnd = json.IndexOf("}", scoreIdx + 8);
                    string scoreStr = json.Substring(scoreIdx + 8, scoreEnd - (scoreIdx + 8)).Trim();
                    
                    int userIdx = json.IndexOf("\"username\":\"", searchIndex);
                    string usernameStr = "Jugador Anónimo";
                    if (userIdx != -1 && userIdx < scoreIdx + 80)
                    {
                        int userEnd = json.IndexOf("\"", userIdx + 12);
                        usernameStr = json.Substring(userIdx + 12, userEnd - (userIdx + 12));
                    }
                    
                    count++;
                    string medal = count == 1 ? "🥇 1." : (count == 2 ? "🥈 2." : (count == 3 ? "🥉 3." : $"  {count}."));
                    sb.AppendLine($"{medal} <b>{usernameStr}</b> — <color=#55FF55>{scoreStr} pts</color>");
                    
                    searchIndex = scoreEnd;
                }
                
                if (count == 0)
                {
                    _leaderboardSummary = "🏆 <i>La tabla aún no tiene puntuaciones registradas en la nube.</i>";
                }
                else
                {
                    _leaderboardSummary = sb.ToString();
                }
            }
            catch
            {
                _leaderboardSummary = "🏆 <i>Ranking actualizado (Revisa el JSON Técnico para ver el formato bruto).</i>";
            }
        }

        #endregion

        void OnGUI()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.UpperCenter, fontStyle = FontStyle.Bold };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 13 };

            Rect panelRect = new Rect(20, 20, 480, 610);
            GUI.Box(panelRect, "Asten BaaS - Quickstart Code-First Demo", boxStyle);

            GUILayout.BeginArea(new Rect(35, 50, 450, 550));

            // Estado Actual y Proveedor
            GUILayout.Label($"<b>Estado:</b> {_statusMessage}", labelStyle);
            if (_isLoggedIn)
            {
                GUILayout.Label($"<b>Sesión Activa:</b> <color=green>{_activeProvider}</color>", labelStyle);
            }
            GUILayout.Space(8);

            if (!_isLoggedIn)
            {
                // Pestañas de autenticación
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(_authTab == 0, " 🎮 Invitado (Device ID) ", "Button", GUILayout.Height(30))) _authTab = 0;
                if (GUILayout.Toggle(_authTab == 1, " 📧 Correo & OTP ", "Button", GUILayout.Height(30))) _authTab = 1;
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (_authTab == 0)
                {
                    GUILayout.Label("Entra instantáneamente sin contraseña usando el ID único de este dispositivo o editor:", labelStyle);
                    GUILayout.Space(5);
                    if (GUILayout.Button("Conectar como Invitado (Guest Login)", buttonStyle, GUILayout.Height(35)))
                    {
                        LoginGuest();
                    }
                }
                else if (_authTab == 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Correo:", GUILayout.Width(80));
                    _emailInput = GUILayout.TextField(_emailInput, textFieldStyle);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Contraseña:", GUILayout.Width(80));
                    _passwordInput = GUILayout.PasswordField(_passwordInput, '*', textFieldStyle);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("1. Registrar (Enviar OTP)", buttonStyle, GUILayout.Height(30)))
                    {
                        RegisterWithEmail();
                    }
                    if (GUILayout.Button("3. Login (Ya Verificado)", buttonStyle, GUILayout.Height(30)))
                    {
                        LoginWithEmail();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    GUILayout.Label("Código OTP recibido en correo (o '123456' en Sandbox):", labelStyle);
                    GUILayout.BeginHorizontal();
                    _otpInput = GUILayout.TextField(_otpInput, textFieldStyle, GUILayout.Width(100));
                    if (GUILayout.Button("2. Verificar OTP & Entrar", buttonStyle, GUILayout.Height(30)))
                    {
                        VerifyEmailOTP();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                // Opciones de Progresión una vez logueado
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Guardar en Nube (+250 Monedas)", buttonStyle, GUILayout.Height(35)))
                {
                    SaveSampleProgress();
                }
                if (GUILayout.Button("Cargar desde Nube", buttonStyle, GUILayout.Height(35)))
                {
                    LoadSampleProgress();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Publicar Récord (+Puntos)", buttonStyle, GUILayout.Height(35)))
                {
                    SubmitSampleScore();
                }
                if (GUILayout.Button("🏆 Consultar Top 5 Nube", buttonStyle, GUILayout.Height(35)))
                {
                    GetSampleLeaderboard();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("🚪 Cerrar Sesión (Logout)", buttonStyle, GUILayout.Height(30)))
                {
                    Logout();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_viewTab == 0, " 🎨 Perfil Visual ", "Button", GUILayout.Height(25))) _viewTab = 0;
            if (GUILayout.Toggle(_viewTab == 1, " 🏆 Top 5 Ranking ", "Button", GUILayout.Height(25))) { if (_viewTab != 1 && _leaderboardSummary.StartsWith("💡")) GetSampleLeaderboard(); _viewTab = 1; }
            if (GUILayout.Toggle(_viewTab == 2, " 💻 JSON Técnico ", "Button", GUILayout.Height(25))) _viewTab = 2;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (_viewTab == 0)
            {
                // Dashboard amigable y visual
                GUIStyle cardStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };
                GUILayout.BeginVertical(cardStyle, GUILayout.Height(125));
                
                if (_isLoggedIn)
                {
                    string shortToken = !string.IsNullOrEmpty(AstenSDK.Instance.PlayerSessionToken) 
                        ? (AstenSDK.Instance.PlayerSessionToken.Length > 20 ? AstenSDK.Instance.PlayerSessionToken.Substring(0, 20) + "..." : AstenSDK.Instance.PlayerSessionToken) 
                        : "N/A";
                    
                    GUILayout.Label($"🆔 <b>Player ID:</b> {AstenSDK.Instance.ActivePlayerId ?? "Sesión Local"}", labelStyle);
                    GUILayout.Label($"🔑 <b>Token JWT:</b> <color=yellow>{shortToken}</color> (Verificado)", labelStyle);
                    GUILayout.Space(4);
                    GUILayout.Label($"💰 <b>Monedas Nube:</b> <color=#55FF55>{_playerCoins}</color> | ⭐ <b>Nivel:</b> <color=#55FFFF>{_playerLevel}</color>", labelStyle);
                    GUILayout.Label($"⚔️ <b>Arma Equipada:</b> Espada de Acero", labelStyle);
                }
                else
                {
                    GUILayout.Label("💡 <i>Inicia sesión para visualizar aquí tu ID único, Token JWT de seguridad y el estado en tiempo real de tus variables en la nube.</i>", labelStyle);
                    GUILayout.Space(5);
                    GUILayout.Label($"<b>Último Evento:</b> {_statusMessage}", labelStyle);
                }
                
                GUILayout.EndVertical();
            }
            else if (_viewTab == 1)
            {
                // Tabla Top 5 en vivo
                GUIStyle cardStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };
                GUILayout.BeginVertical(cardStyle, GUILayout.Height(125));
                GUILayout.Label(_leaderboardSummary, labelStyle);
                GUILayout.EndVertical();
            }
            else
            {
                // Consola de respuesta en pantalla (Raw JSON)
                GUILayout.Label("<b>Respuesta Cruda del Backend / Payload:</b>", labelStyle);
                _lastServerResponse = GUILayout.TextArea(_lastServerResponse, GUILayout.Height(95));
            }

            GUILayout.EndArea();
        }
    }
}
