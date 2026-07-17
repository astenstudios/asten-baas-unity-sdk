using UnityEngine;
using AstenBaaS;

// Define tu estructura de datos del juego
[System.Serializable]
public class PlayerStats
{
    public int level;
    public int gold;
    public int experience;
    public string activeWeapon;
}

public class ExampleIntegration : MonoBehaviour
{
    private void Start()
    {
        // 1. Inicializa el SDK con las credenciales de tu consola de Asten BaaS
        string myGameId = "TU_GAME_ID_AQUÍ";
        string myApiKey = "astn_test_TU_CLAVE_API_AQUÍ";
        
        AstenSDK.Instance.Initialize(myGameId, myApiKey);

        // 2. Registramos a un jugador nuevo (opcional, para registrar por primera vez)
        AstenSDK.Instance.RegisterPlayer("player1@gmail.com", "mipassword123", (registerSuccess, registerResponse) =>
        {
            if (registerSuccess)
            {
                Debug.Log("Jugador registrado con éxito en Asten!");
            }
            
            // 3. Iniciamos sesión para obtener el Player JWT seguro de sesión
            AstenSDK.Instance.LoginPlayer("player1@gmail.com", "mipassword123", (loginSuccess, loginResponse) =>
            {
                if (loginSuccess)
                {
                    Debug.Log("Sesión iniciada con éxito. Token JWT obtenido.");
                    
                    // 4. Cargamos sus estadísticas guardadas en la nube
                    LoadProgress();
                }
                else
                {
                    Debug.LogError("Error al iniciar sesión con el jugador.");
                }
            });
        });
    }

    public void LoadProgress()
    {
        AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
        {
            if (success)
            {
                Debug.Log($"Datos cargados de la nube: {jsonResponse}");
                // Puedes usar JsonUtility para parsearlo a tu clase C#
                // PlayerStats stats = JsonUtility.FromJson<PlayerStats>(jsonResponse);
            }
            else
            {
                Debug.LogError("Error al cargar los datos del jugador.");
            }
        });
    }

    public void SaveProgress()
    {
        // Creamos un objeto de datos de prueba
        PlayerStats stats = new PlayerStats
        {
            level = 5,
            gold = 250,
            experience = 1500,
            activeWeapon = "Silver Sword"
        };

        // 5. Guardamos en la nube (el SDK aplicará Debounce/Cooldown si llamas esta función muy rápido)
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

    public void SubmitLeaderboardScore()
    {
        // 6. Enviar puntaje de 1500 al ranking "global_ranking" con el nombre de usuario "PlayerOne"
        AstenSDK.Instance.SubmitScore("global_ranking", 1500f, "PlayerOne", (success, response) =>
        {
            if (success)
            {
                Debug.Log("Puntuación subida al leaderboard exitosamente!");
            }
            else
            {
                Debug.LogError("Error al subir puntuación al leaderboard.");
            }
        });
    }
}
