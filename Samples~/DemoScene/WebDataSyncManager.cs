using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si usas TextMeshPro (Recomendado)
using AstenBaaS;

public class WebDataSyncManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI quantityText; // O usa UnityEngine.UI.Text si no usas TMP
    public TextMeshProUGUI statusText;

    [Header("Configuración de Polling (Sondeo)")]
    public bool autoSync = true;
    public float syncIntervalSeconds = 3.0f; // Cada cuántos segundos consulta el servidor

    private Coroutine _syncCoroutine;

    private void Start()
    {
        // 1. Asegúrate de haber inicializado el SDK e iniciado sesión previamente
        StartAutoSync();
    }

    public void StartAutoSync()
    {
        if (_syncCoroutine != null) StopCoroutine(_syncCoroutine);
        _syncCoroutine = StartCoroutine(AutoSyncCoroutine());

    }

    public void StopAutoSync()
    {
        if (_syncCoroutine != null)
        {
            StopCoroutine(_syncCoroutine);
            _syncCoroutine = null;
        }
    }

    private IEnumerator AutoSyncCoroutine()
    {
        while (autoSync)
        {
            FetchDataFromConsole();
            yield return new WaitForSeconds(syncIntervalSeconds);
        }
    }

    /// <summary>
    /// Consulta los datos actualizados de la nube y refresca la UI en Unity.
    /// </summary>
    public void FetchDataFromConsole()
    {

        string myGameId = "f0c01ba8-d510-4ad3-8d6d-cde283a039ab";
        string myApiKey = "astn_live_38c7062c3a3fbae5b7b2d73d27c1a952671edae60ed0e402";

        AstenSDK.Instance.Initialize(myGameId, myApiKey);
        AstenSDK.Instance.LoginPlayer("player1@gmail.com", "mipassword123", (loginSuccess, loginResponse) =>
       {
           if (loginSuccess)
           {
               Debug.Log("Sesión iniciada con éxito. Token JWT obtenido.");
               AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
                {
                    if (success)
                    {
                        ParseAndUpdateUI(jsonResponse);
                    }
                    else
                    {
                        if (statusText != null) statusText.text = "Error al sincronizar datos";
                        Debug.LogWarning("[WebDataSyncManager] No se pudo obtener datos del servidor.");
                    }
                });
           }
           else
           {
               Debug.LogError("Error al iniciar sesión con el jugador.");
           }
       });

    }

    private void ParseAndUpdateUI(string rawJson)
    {
        try
        {
            // El backend retorna un formato {"customData": { "quantity": 100, ... }}
            // Extraemos la parte interna de customData
            PlayerInventoryData data = JsonUtility.FromJson<PlayerInventoryData>(ExtractCustomDataJson(rawJson));

            if (data != null)
            {
                if (quantityText != null)
                {
                    quantityText.text = $"Cantidad: {data.quantity}";
                }
                if (statusText != null)
                {
                    statusText.text = "¡Actualizado desde Consola Web!";
                }
                Debug.Log($"[WebDataSyncManager] UI actualizada con Cantidad = {data.quantity}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WebDataSyncManager] Error al parsear JSON: {ex.Message}");
        }
    }


    private string ExtractCustomDataJson(string rawJsonResponse)
    {
        string searchKey = "\"custom_data\":";
        int startIndex = rawJsonResponse.IndexOf(searchKey);

        if (startIndex == -1)
            return rawJsonResponse;

        // Buscamos la llave de apertura justo después de la clave "custom_data"
        int objectStart = rawJsonResponse.IndexOf("{", startIndex + searchKey.Length);
        if (objectStart == -1)
            return rawJsonResponse; // Por si custom_data es null o no es un objeto

        int openBraces = 0;
        int objectEnd = -1;

        // Iteramos desde la primera llave para encontrar su par de cierre exacto
        for (int i = objectStart; i < rawJsonResponse.Length; i++)
        {
            if (rawJsonResponse[i] == '{')
            {
                openBraces++;
            }
            else if (rawJsonResponse[i] == '}')
            {
                openBraces--;
                // Cuando volvemos a 0, encontramos el final del objeto custom_data
                if (openBraces == 0)
                {
                    objectEnd = i;
                    break;
                }
            }
        }

        if (objectEnd != -1)
        {
            string extracted = rawJsonResponse.Substring(objectStart, (objectEnd - objectStart) + 1);
            Debug.Log($"[ExtractCustomDataJson] Extracted JSON: {extracted}");
            return extracted;
        }

        return rawJsonResponse;
    }
}