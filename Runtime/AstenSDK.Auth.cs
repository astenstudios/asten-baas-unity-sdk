using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        /// <summary>
        /// Registers a new end user (player) in the game.
        /// </summary>
        public void RegisterPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] You cannot register a player without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.LogError("[AstenSDK] Email and password are required.");
                callback?.Invoke(false, "Email or password cannot be empty");
                return;
            }

            string payload = $"{{\"email\":\"{email}\", \"password\":\"{password}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/register", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    Debug.Log($"[AstenSDK] Player registered successfully.");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Logs in a player under the game's context and saves the Player JWT.
        /// </summary>
        public void LoginPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] You cannot log in without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                Debug.LogError("[AstenSDK] Email cannot be empty.");
                callback?.Invoke(false, "Email cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                Debug.LogError("[AstenSDK] Password cannot be empty.");
                callback?.Invoke(false, "Password cannot be empty");
                return;
            }

            string payload = $"{{\"provider\":\"email\", \"email\":\"{email}\", \"password\":\"{password}\"}}";
            ExecuteAuthRequest(payload, "email", callback);
        }

        /// <summary>
        /// Logs in (or automatically registers) a player anonymously using their Device ID.
        /// </summary>
        public void LoginWithDeviceId(Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] You cannot log in without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }

            // We use a local variable if deviceId was not provided as a parameter in the version we moved
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            string payload = $"{{\"provider\":\"device\", \"device_id\":\"{deviceId}\"}}";
            Debug.Log($" [AstenSDK] Logging in with payload: {payload}");
            ExecuteAuthRequest(payload, "device", callback);
        }

        /// <summary>
        /// Verifies the player's email using the 6-digit OTP code sent to their inbox.
        /// </summary>
        public void VerifyPlayerEmail(string email, string otpCode, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[AstenSDK] You cannot verify the email without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                Debug.LogError("[AstenSDK] Email and OTP code are required.");
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
                    Debug.Log($"[AstenSDK] ✅ Email verified successfully with OTP code! Active session for ID: {_activePlayerId}");
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
                    // Extract player token
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

                    // Extract player ID
                    ExtractAndSetPlayerId(response);

                    Debug.Log($"[AstenSDK] Player authenticated successfully via {providerName}. ID: {_activePlayerId}");
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
        /// Closes the player's active session, clearing the JWT token and ID from memory.
        /// </summary>
        public void Logout()
        {
            _activePlayerId = null;
            _playerSessionToken = null;
            Debug.Log("[AstenSDK] 🚪 Player session closed successfully.");
        }

        /// <summary>
        /// Closes the player's active session (Logout alias for naming consistency).
        /// </summary>
        public void LogoutPlayer()
        {
            Logout();
        }
    }
}
