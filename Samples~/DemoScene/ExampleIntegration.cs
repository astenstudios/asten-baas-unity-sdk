using UnityEngine;
using AstenBaaS;

// Define tu estructura de datos del juego
[System.Serializable]
public class PlayerStats
{
    public int quantity;
    public string itemName;
}

public class ExampleIntegration : MonoBehaviour
{

    public void SaveProgress()
    {
        string myGameId = "dc458654-ced2-43dd-bef7-21a54f163211";
        string myApiKey = "astn_live_7ce5ba3d6c2df3ff27852f94ea8b1deb4132ebb858fea4ba";

        AstenSDK.Instance.Initialize(myGameId, myApiKey);
        AstenSDK.Instance.LoginPlayer("player1@gmail.com", "mipassword123", (loginSuccess, loginResponse) =>
             {
                 if (loginSuccess)
                 {
                     AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
                     {
                         if (success)
                         {
                             Debug.Log("Sesión iniciada con éxito. Token JWT obtenido.");

                             PlayerStats stats = new PlayerStats
                             {
                                 quantity = 250,
                                 itemName = "Gemas de Cristal"
                             };

                             AstenSDK.Instance.SavePlayerData(stats, (success, response) =>
                             {
                                 if (success)
                                 {
                                     Debug.Log("Progreso guardado exitosamente en Asten BaaS!");
                                 }
                                 else
                                 {
                                     Debug.LogError("Error de red al intentar guardar progreso.");
                                 }
                             });
                         }
                         else
                         {
                             Debug.LogWarning("[SaveProgress] No se pudo obtener datos del servidor.");
                         }
                     });

                 }
                 else
                 {
                     Debug.LogError("Error al iniciar sesión con el jugador.");
                 }
             });

    }

}
