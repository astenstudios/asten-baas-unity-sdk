using System;
using UnityEngine;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        /// <summary>
        /// Registers a new end-user (player) account.
        /// Sends an email verification request with a 6-digit OTP code.
        /// </summary>
        /// <param name="email">Player email address.</param>
        /// <param name="password">Player password.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void RegisterPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                LogError("You cannot register a player without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                LogError("Email and password are required.");
                callback?.Invoke(false, "Email or password cannot be empty");
                return;
            }

            string payload = $"{{\"email\":\"{email}\", \"password\":\"{password}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/register", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    Log("Player registered successfully. Verification OTP dispatched.");
                }
                callback?.Invoke(success, response);
            }));
        }

        /// <summary>
        /// Logs in an existing player using email and password.
        /// </summary>
        /// <param name="email">Player email address.</param>
        /// <param name="password">Player password.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void LoginPlayer(string email, string password, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                LogError("You cannot log in without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                LogError("Email cannot be empty.");
                callback?.Invoke(false, "Email cannot be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                LogError("Password cannot be empty.");
                callback?.Invoke(false, "Password cannot be empty");
                return;
            }

            string payload = $"{{\"provider\":\"email\", \"email\":\"{email}\", \"password\":\"{password}\"}}";
            ExecuteAuthRequest(payload, "email", callback);
        }

        /// <summary>
        /// Logs in or automatically registers a player anonymously using their unique device hardware ID.
        /// Ideal for zero-friction guest onboarding.
        /// </summary>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void LoginWithDeviceId(Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                LogError("You cannot log in without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string payload = $"{{\"provider\":\"device\", \"device_id\":\"{deviceId}\"}}";
            Log($"Logging in anonymously with Device ID...");
            ExecuteAuthRequest(payload, "device", callback);
        }

        /// <summary>
        /// Verifies the player's email address using the 6-digit OTP code sent to their inbox.
        /// </summary>
        /// <param name="email">Player email address.</param>
        /// <param name="otpCode">6-digit verification code received by the player.</param>
        /// <param name="callback">Callback containing success status and server JSON response.</param>
        public void VerifyPlayerEmail(string email, string otpCode, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                LogError("You cannot verify email without calling Initialize() first.");
                callback?.Invoke(false, "SDK not initialized");
                return;
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                LogError("Email and OTP code are required.");
                callback?.Invoke(false, "Email and OTP cannot be empty");
                return;
            }

            string payload = $"{{\"email\":\"{email}\", \"otp\":\"{otpCode}\"}}";
            StartCoroutine(PostRequestCoroutine("/player/verify-otp", payload, _apiKey, null, (success, response) =>
            {
                if (success)
                {
                    ExtractAndSetSession(response);
                    Log($"Email verified successfully! Session active for Player ID: {_activePlayerId}");
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
                    ExtractAndSetSession(response);
                    Log($"Player authenticated successfully via {providerName}. ID: {_activePlayerId}");
                }
                callback?.Invoke(success, response);
            }));
        }

        private void ExtractAndSetSession(string response)
        {
            if (string.IsNullOrEmpty(response)) return;

            try
            {
                AuthResponse authData = JsonUtility.FromJson<AuthResponse>(response);
                if (authData != null)
                {
                    if (!string.IsNullOrEmpty(authData.token))
                    {
                        _playerSessionToken = authData.token;
                    }
                    if (!string.IsNullOrEmpty(authData.PlayerId))
                    {
                        _activePlayerId = authData.PlayerId;
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to substring parsing if response JSON has custom wrapper
                ExtractTokenFallback(response);
                ExtractPlayerIdFallback(response);
            }

            if (string.IsNullOrEmpty(_playerSessionToken)) ExtractTokenFallback(response);
            if (string.IsNullOrEmpty(_activePlayerId)) ExtractPlayerIdFallback(response);
        }

        private void ExtractTokenFallback(string response)
        {
            string tokenSearch = "\"token\":\"";
            int tokenIndex = response.IndexOf(tokenSearch, StringComparison.Ordinal);
            if (tokenIndex != -1)
            {
                int start = tokenIndex + tokenSearch.Length;
                int end = response.IndexOf("\"", start, StringComparison.Ordinal);
                if (end != -1)
                {
                    _playerSessionToken = response.Substring(start, end - start);
                }
            }
        }

        private void ExtractPlayerIdFallback(string response)
        {
            string[] idKeys = new string[] { "\"id\":\"", "\"_id\":\"" };
            foreach (var key in idKeys)
            {
                int index = response.IndexOf(key, StringComparison.Ordinal);
                if (index != -1)
                {
                    int start = index + key.Length;
                    int end = response.IndexOf("\"", start, StringComparison.Ordinal);
                    if (end != -1)
                    {
                        _activePlayerId = response.Substring(start, end - start);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Closes the active player session, clearing security JWT token and player ID from memory.
        /// </summary>
        public void Logout()
        {
            _activePlayerId = null;
            _playerSessionToken = null;
            Log("Player session closed successfully.");
        }

        /// <summary>
        /// Closes the active player session (Logout alias for naming consistency).
        /// </summary>
        public void LogoutPlayer()
        {
            Logout();
        }
    }
}
