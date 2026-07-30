using UnityEngine;
using AstenBaaS;

namespace AstenBaaS.Samples
{
    /// <summary>
    /// Quickstart 100% code-driven demo using OnGUI.
    /// Requires no manual Canvas, TextMeshPro, or UI prefab configuration.
    /// Works out of the box by clicking Play in Unity Editor and covers all authentication flows.
    /// </summary>
    public class AstenQuickstartDemo : MonoBehaviour
    {
        [Header("Asten BaaS Configuration")]
        [Tooltip("Your Game ID from the Asten console (Default Sandbox/Demo)")]
        public string gameId = "";
        
        [Tooltip("Your Sandbox or Production API Key")]
        public string apiKey = "";

        // UI Authentication Mode
        private int _authTab = 0; // 0: Guest (Device ID), 1: Email & OTP
        private int _viewTab = 0; // 0: Visual Profile, 1: Top Leaderboard, 2: Technical JSON
        private string _leaderboardSummary = "💡 <i>Click 'Get Top 5' or 'Submit Score' to download the real-time global ranking.</i>";
        private string _emailInput = "test@email.com";
        private string _passwordInput = "";
        private string _otpInput = "";

        // SDK State
        private string _statusMessage = "🟡 SDK Disconnected. Select an authentication method.";
        private bool _isLoggedIn = false;
        private string _activeProvider = "None";

        // Memory Sample Data
        private int _playerCoins = 1000;
        private int _playerLevel = 1;
        private string _lastServerResponse = "No requests sent yet.";

        void Start()
        {
            // Initialize SDK on startup
            AstenSDK.Instance.Initialize(gameId, apiKey);
            _statusMessage = "🟢 SDK Initialized. Choose how you want to log in:";
        }

        #region Authentication Flows

        private void LoginGuest()
        {
            _statusMessage = "🔄 Connecting as Guest (Device ID)...";
            _lastServerResponse = "Sending request to /player/auth...";

            AstenSDK.Instance.LoginWithDeviceId((success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Device ID (Guest)";
                    _statusMessage = "🟢 Authenticated successfully (Guest / Device ID). Syncing cloud...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] Guest authentication completed successfully.");
                }
                else
                {
                    _isLoggedIn = false;
                    _statusMessage = "🔴 Error connecting as guest.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] LoginGuest error: {response}");
                }
            });
        }

        private void RegisterWithEmail()
        {
            _statusMessage = "🔄 Registering player and requesting OTP code...";
            _lastServerResponse = $"Registering {_emailInput} on server...";

            AstenSDK.Instance.RegisterPlayer(_emailInput, _passwordInput, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = "📬 Registration successful! Check your email or use master OTP '123456' in Sandbox.";
                    _lastServerResponse = response;
                    Debug.Log("📬 [AstenQuickstart] Email registration completed. Awaiting OTP verification.");
                }
                else
                {
                    _statusMessage = "🔴 Error registering account with email.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Registration error: {response}");
                }
            });
        }

        private void VerifyEmailOTP()
        {
            _statusMessage = "🔄 Verifying 6-digit OTP code...";
            _lastServerResponse = $"Verifying OTP '{_otpInput}' for {_emailInput}...";

            AstenSDK.Instance.VerifyPlayerEmail(_emailInput, _otpInput, (success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Verified Email";
                    _statusMessage = "🟢 Email verified and session active! Syncing cloud...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] OTP verification successful and session active.");
                }
                else
                {
                    _statusMessage = "🔴 Incorrect or expired OTP code.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] OTP Verification error: {response}");
                }
            });
        }

        private void LoginWithEmail()
        {
            _statusMessage = "🔄 Logging in with email...";
            _lastServerResponse = $"Authenticating {_emailInput}...";

            AstenSDK.Instance.LoginPlayer(_emailInput, _passwordInput, (success, response) =>
            {
                if (success)
                {
                    _isLoggedIn = true;
                    _activeProvider = "Email Address";
                    _statusMessage = "🟢 Login successful. Syncing cloud...";
                    _lastServerResponse = response;
                    _playerCoins = 100;
                    _playerLevel = 1;
                    LoadSampleProgress();
                    Debug.Log("✅ [AstenQuickstart] Email session logged in successfully.");
                }
                else
                {
                    _isLoggedIn = false;
                    _statusMessage = "🔴 Login error: Invalid credentials or unverified email.";
                    _lastServerResponse = response;
                    Debug.LogError($"❌ [AstenQuickstart] Email Login error: {response}");
                }
            });
        }

        private void Logout()
        {
            AstenSDK.Instance.Logout();
            _isLoggedIn = false;
            _activeProvider = "None";
            _playerCoins = 0;
            _playerLevel = 1;
            _leaderboardSummary = "💡 <i>Click 'Get Top 5' or 'Submit Score' to download the real-time global ranking.</i>";
            _statusMessage = "🟡 Logged out. Select a method to enter.";
            _lastServerResponse = "Session logged out locally.";
        }

        #endregion

        #region Progression & Cloud Data

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

            _statusMessage = "🔄 Saving game progress to the cloud...";
            _playerCoins += 250;
            _playerLevel++;

            SampleSaveData progressData = new SampleSaveData
            {
                coins = _playerCoins,
                level = _playerLevel,
                weapon = "Steel Sword",
                timestamp = System.DateTime.UtcNow.ToString("o")
            };

            AstenSDK.Instance.SavePlayerData(progressData, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = $"✅ Progress saved! Level: {_playerLevel} | Coins: {_playerCoins}";
                    _lastServerResponse = response;
                    Debug.Log($"✅ [AstenQuickstart] Save game saved to MongoDB Atlas: {response}");
                }
                else
                {
                    _statusMessage = "🔴 Error saving game to server.";
                    _lastServerResponse = response;
                }
            });
        }

        private void LoadSampleProgress()
        {
            if (!_isLoggedIn) return;

            _statusMessage = "🔄 Loading game progress from cloud...";
            AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
            {
                if (success)
                {
                    _statusMessage = "✅ Data loaded successfully from server!";
                    _lastServerResponse = jsonResponse;
                    Debug.Log($"✅ [AstenQuickstart] Save data loaded: {jsonResponse}");

                    // Extract actual coins and level from player JSON to sync UI
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
                        Debug.LogWarning($"[AstenQuickstart] Failed to parse coins/level from JSON: {ex.Message}");
                    }
                }
                else
                {
                    _statusMessage = "🔴 Error retrieving cloud data.";
                    _lastServerResponse = jsonResponse;
                }
            });
        }

        private void SubmitSampleScore()
        {
            if (!_isLoggedIn) return;

            int randomScore = Random.Range(1000, 9999);
            _statusMessage = $"🔄 Submitting score ({randomScore}) to global leaderboard...";

            AstenSDK.Instance.SubmitScore("leaderboard_score", randomScore, "Player_" + randomScore, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = $"🏆 High score of {randomScore} pts posted! Downloading Top 5 leaderboard...";
                    _lastServerResponse = response;
                    Debug.Log($"🏆 [AstenQuickstart] Score submitted to Leaderboard: {response}");
                    GetSampleLeaderboard(); // Sync and switch to Top 5 tab
                }
                else
                {
                    _statusMessage = "🔴 Error submitting to Leaderboard.";
                    _lastServerResponse = response;
                }
            });
        }

        private void GetSampleLeaderboard()
        {
            if (!_isLoggedIn) return;

            _statusMessage = "🔄 Querying Top 5 Leaderboard...";
            _leaderboardSummary = "🔄 <i>Downloading ranking from MongoDB Atlas...</i>";
            AstenSDK.Instance.GetTopScores("leaderboard_score", 5, (success, response) =>
            {
                if (success)
                {
                    _statusMessage = "🏆 Top 5 Ranking downloaded successfully from cloud!";
                    _lastServerResponse = response;
                    ParseAndFormatLeaderboard(response);
                    _viewTab = 1; // Switch automatically to Top 5 tab
                    Debug.Log($"🏆 [AstenQuickstart] Ranking retrieved: {response}");
                }
                else
                {
                    _statusMessage = "🔴 Error querying Leaderboard.";
                    _lastServerResponse = response;
                }
            });
        }

        private void ParseAndFormatLeaderboard(string json)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<b>🏆 TOP 5 GLOBAL RANKING (Live):</b>");
                
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
                    string usernameStr = "Anonymous Player";
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
                    _leaderboardSummary = "🏆 <i>The leaderboard has no scores recorded in the cloud yet.</i>";
                }
                else
                {
                    _leaderboardSummary = sb.ToString();
                }
            }
            catch
            {
                _leaderboardSummary = "🏆 <i>Ranking updated (Check Technical JSON for raw format).</i>";
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

            // Status and Provider
            GUILayout.Label($"<b>Status:</b> {_statusMessage}", labelStyle);
            if (_isLoggedIn)
            {
                GUILayout.Label($"<b>Active Session:</b> <color=green>{_activeProvider}</color>", labelStyle);
            }
            GUILayout.Space(8);

            if (!_isLoggedIn)
            {
                // Auth Tabs
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(_authTab == 0, " 🎮 Guest (Device ID) ", "Button", GUILayout.Height(30))) _authTab = 0;
                if (GUILayout.Toggle(_authTab == 1, " 📧 Email & OTP ", "Button", GUILayout.Height(30))) _authTab = 1;
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (_authTab == 0)
                {
                    GUILayout.Label("Log in instantly without a password using this device or editor's unique ID:", labelStyle);
                    GUILayout.Space(5);
                    if (GUILayout.Button("Connect as Guest (Guest Login)", buttonStyle, GUILayout.Height(35)))
                    {
                        LoginGuest();
                    }
                }
                else if (_authTab == 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Email:", GUILayout.Width(80));
                    _emailInput = GUILayout.TextField(_emailInput, textFieldStyle);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Password:", GUILayout.Width(80));
                    _passwordInput = GUILayout.PasswordField(_passwordInput, '*', textFieldStyle);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("1. Register (Send OTP)", buttonStyle, GUILayout.Height(30)))
                    {
                        RegisterWithEmail();
                    }
                    if (GUILayout.Button("3. Login (Verified)", buttonStyle, GUILayout.Height(30)))
                    {
                        LoginWithEmail();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    GUILayout.Label("OTP Code received via email (or '123456' in Sandbox):", labelStyle);
                    GUILayout.BeginHorizontal();
                    _otpInput = GUILayout.TextField(_otpInput, textFieldStyle, GUILayout.Width(100));
                    if (GUILayout.Button("2. Verify OTP & Log In", buttonStyle, GUILayout.Height(30)))
                    {
                        VerifyEmailOTP();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                // Progression options once logged in
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save to Cloud (+250 Coins)", buttonStyle, GUILayout.Height(35)))
                {
                    SaveSampleProgress();
                }
                if (GUILayout.Button("Load from Cloud", buttonStyle, GUILayout.Height(35)))
                {
                    LoadSampleProgress();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Submit High Score (+Pts)", buttonStyle, GUILayout.Height(35)))
                {
                    SubmitSampleScore();
                }
                if (GUILayout.Button("🏆 Get Top 5 Cloud", buttonStyle, GUILayout.Height(35)))
                {
                    GetSampleLeaderboard();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("🚪 Log Out (Logout)", buttonStyle, GUILayout.Height(30)))
                {
                    Logout();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_viewTab == 0, " 🎨 Visual Profile ", "Button", GUILayout.Height(25))) _viewTab = 0;
            if (GUILayout.Toggle(_viewTab == 1, " 🏆 Top 5 Ranking ", "Button", GUILayout.Height(25))) { if (_viewTab != 1 && _leaderboardSummary.StartsWith("💡")) GetSampleLeaderboard(); _viewTab = 1; }
            if (GUILayout.Toggle(_viewTab == 2, " 💻 Technical JSON ", "Button", GUILayout.Height(25))) _viewTab = 2;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (_viewTab == 0)
            {
                // Visual & Friendly Dashboard
                GUIStyle cardStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };
                GUILayout.BeginVertical(cardStyle, GUILayout.Height(125));
                
                if (_isLoggedIn)
                {
                    string shortToken = !string.IsNullOrEmpty(AstenSDK.Instance.PlayerSessionToken) 
                        ? (AstenSDK.Instance.PlayerSessionToken.Length > 20 ? AstenSDK.Instance.PlayerSessionToken.Substring(0, 20) + "..." : AstenSDK.Instance.PlayerSessionToken) 
                        : "N/A";
                    
                    GUILayout.Label($"🆔 <b>Player ID:</b> {AstenSDK.Instance.ActivePlayerId ?? "Local Session"}", labelStyle);
                    GUILayout.Label($"🔑 <b>JWT Token:</b> <color=yellow>{shortToken}</color> (Verified)", labelStyle);
                    GUILayout.Space(4);
                    GUILayout.Label($"💰 <b>Cloud Coins:</b> <color=#55FF55>{_playerCoins}</color> | ⭐ <b>Level:</b> <color=#55FFFF>{_playerLevel}</color>", labelStyle);
                    GUILayout.Label($"⚔️ <b>Equipped Weapon:</b> Steel Sword", labelStyle);
                }
                else
                {
                    GUILayout.Label("💡 <i>Log in to view your unique Player ID, security JWT Token, and real-time state of your cloud variables here.</i>", labelStyle);
                    GUILayout.Space(5);
                    GUILayout.Label($"<b>Latest Event:</b> {_statusMessage}", labelStyle);
                }
                
                GUILayout.EndVertical();
            }
            else if (_viewTab == 1)
            {
                // Live Top 5 Ranking
                GUIStyle cardStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };
                GUILayout.BeginVertical(cardStyle, GUILayout.Height(125));
                GUILayout.Label(_leaderboardSummary, labelStyle);
                GUILayout.EndVertical();
            }
            else
            {
                // Response console (Raw JSON)
                GUILayout.Label("<b>Raw Backend Response / Payload:</b>", labelStyle);
                _lastServerResponse = GUILayout.TextArea(_lastServerResponse, GUILayout.Height(95));
            }

            GUILayout.EndArea();
        }
    }
}
