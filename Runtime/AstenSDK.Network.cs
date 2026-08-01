using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AstenBaaS
{
    public partial class AstenSDK
    {
        private const int DefaultTimeoutSeconds = 15;

        private IEnumerator GetRequestCoroutine(string endpoint, string apiKey, string playerToken, Action<bool, string> callback)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(_backendUrl + endpoint);
            webRequest.timeout = DefaultTimeoutSeconds;

            if (!string.IsNullOrEmpty(apiKey))
                webRequest.SetRequestHeader("X-API-Key", apiKey);

            if (!string.IsNullOrEmpty(playerToken))
                webRequest.SetRequestHeader("Authorization", $"Bearer {playerToken}");

            yield return webRequest.SendWebRequest();

            string responseText = webRequest.downloadHandler?.text;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, responseText);
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                string offlineMsg = "Connection Error: No internet access or server is unresponsive.";
                LogWarning($"GET {offlineMsg}");
                callback?.Invoke(false, offlineMsg);
            }
            else
            {
                LogError($"GET Error {webRequest.responseCode}: {webRequest.error}");
                callback?.Invoke(false, !string.IsNullOrEmpty(responseText) ? responseText : webRequest.error);
            }
        }

        private IEnumerator PostRequestCoroutine(string endpoint, string jsonJsonPayload, string apiKey, string playerToken, Action<bool, string> callback)
        {
            using UnityWebRequest webRequest = new UnityWebRequest(_backendUrl + endpoint, "POST");
            webRequest.timeout = DefaultTimeoutSeconds;
            
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonJsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(apiKey))
                webRequest.SetRequestHeader("X-API-Key", apiKey);

            if (!string.IsNullOrEmpty(playerToken))
                webRequest.SetRequestHeader("Authorization", $"Bearer {playerToken}");

            yield return webRequest.SendWebRequest();

            string responseText = webRequest.downloadHandler?.text;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, responseText);
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                string offlineMsg = "Connection Error: No internet access or server is unresponsive.";
                LogWarning($"POST {offlineMsg}");
                callback?.Invoke(false, offlineMsg);
            }
            else
            {
                LogError($"POST Error {webRequest.responseCode}: {webRequest.error}");
                callback?.Invoke(false, !string.IsNullOrEmpty(responseText) ? responseText : webRequest.error);
            }
        }
    }
}
