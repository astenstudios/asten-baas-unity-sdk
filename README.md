# Asten BaaS Unity SDK

El SDK oficial de C# para integrar **Asten BaaS** en tus videojuegos de Unity. Permite gestionar autenticación de jugadores (correo, dispositivo, Google, Discord), persistencia de datos (Cloud Saves) con protección contra spam/cooldown (Debouncing), y Tablas de Clasificación (Leaderboards) globales.

---

## 🚀 Instalación en Unity

El SDK está configurado como un **paquete oficial de Unity (UPM)**. Puedes instalarlo directamente en tu proyecto usando su URL de Git:

1. En Unity, abre **Window** ➔ **Package Manager**.
2. Haz clic en el botón **"+"** en la esquina superior izquierda del Package Manager.
3. Selecciona **Add package from git URL...**
4. Pega la URL del repositorio público del SDK:
   ```text
   https://github.com/astenstudios/asten-baas-unity-sdk.git
   ```
5. Haz clic en **Add**. ¡Listo! Unity descargará y agregará el SDK automáticamente.

---

## 🛠️ Configuración Inicial

Para empezar a utilizar el SDK, inicialízalo con tu **Game ID** y tu **API Key** (que puedes generar desde la Consola de Desarrollador de Asten BaaS):

```csharp
using UnityEngine;
using AstenBaaS;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        string gameId = "TU_GAME_ID_AQUÍ";
        string apiKey = "astn_test_TU_CLAVE_API_AQUÍ";
        string backendUrl = "https://api.baas.astenstudios.com"; // Tu endpoint de producción

        // Inicializar la instancia del SDK
        AstenSDK.Instance.Initialize(gameId, apiKey, backendUrl);
    }
}
```

---

## 🔑 Autenticación de Jugadores

Asten BaaS soporta múltiples proveedores de identidad. Al autenticar un jugador, el SDK guarda internamente el token JWT de sesión para autorizar todas las peticiones posteriores de forma transparente.

### 📧 Registro e Inicio de Sesión por Correo
```csharp
// 1. Registro de nuevo jugador
AstenSDK.Instance.RegisterPlayer("jugador@ejemplo.com", "miPasswordSeguro123", (success, response) =>
{
    if (success)
    {
        Debug.Log("Jugador registrado con éxito.");
    }
});

// 2. Inicio de sesión
AstenSDK.Instance.LoginPlayer("jugador@ejemplo.com", "miPasswordSeguro123", (success, response) =>
{
    if (success)
    {
        Debug.Log("Sesión iniciada. Token JWT obtenido.");
    }
});
```

### 📱 Inicio de Sesión Anónimo (ID de Dispositivo)
Ideal para registrar de forma silenciosa al jugador la primera vez que abre el juego en su celular:
```csharp
string deviceId = SystemInfo.deviceUniqueIdentifier;

AstenSDK.Instance.LoginWithDeviceId(deviceId, (success, response) =>
{
    if (success)
    {
        Debug.Log("Jugador autenticado de forma anónima con el ID de su dispositivo.");
    }
});
```

### 🌐 Autenticación por Google o Discord
Si obtienes un ID Token de Google (con el SDK oficial de Google Sign-In) o un Token de Discord:
```csharp
// Google Auth
AstenSDK.Instance.LoginWithGoogle(googleIdToken, (success, response) => { /* ... */ });

// Discord Auth
AstenSDK.Instance.LoginWithDiscord(discordAccessToken, (success, response) => { /* ... */ });
```

---

## 💾 Persistencia de Datos (Cloud Saves)

Puedes guardar cualquier estructura u objeto de datos personalizado del jugador (como estadísticas, inventarios, etc.) en formato JSON. El SDK cuenta con protección de **Debouncing** para no saturar el servidor con escrituras excesivas si se llama repetidamente en bucles de juego.

```csharp
[System.Serializable]
public class PlayerStats
{
    public int level;
    public int gold;
    public string activeWeapon;
}

// 1. Guardar datos del jugador en la nube
public void SaveGame()
{
    PlayerStats stats = new PlayerStats { level = 10, gold = 950, activeWeapon = "Espada Dorada" };

    AstenSDK.Instance.SavePlayerData(stats, (success, response) =>
    {
        if (success)
        {
            Debug.Log("Progreso de juego guardado en la nube.");
        }
    });
}

// 2. Cargar datos del jugador desde la nube
public void LoadGame()
{
    AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
    {
        if (success)
        {
            Debug.Log("Datos crudos de la nube: " + jsonResponse);
            // Reconstruir tu objeto C# con JsonUtility
            // PlayerStats stats = JsonUtility.FromJson<PlayerStats>(jsonResponse);
        }
    });
}
```

---

## 🏆 Tablas de Clasificación (Leaderboards)

El SDK te permite registrar puntuaciones del jugador y obtener rankings globales al instante.

### 1. Enviar una Puntuación
Registra o actualiza la puntuación del jugador. El servidor sólo sobrescribirá el récord anterior si el nuevo puntaje es superior:
```csharp
string leaderboardId = "ranking_mundial_nivel_1";
float score = 2504.5f;
string displayUsername = "SuperGamer99";

AstenSDK.Instance.SubmitScore(leaderboardId, score, displayUsername, (success, response) =>
{
    if (success)
    {
        Debug.Log("Puntaje enviado al ranking exitosamente.");
    }
});
```

### 2. Obtener el Ranking Top (Mejores Puntuaciones)
Obtén las puntuaciones ordenadas de forma descendente en una tabla de clasificación para mostrarlas en tu UI de Unity:
```csharp
AstenSDK.Instance.GetTopScores("ranking_mundial_nivel_1", 10, (success, jsonResponse) =>
{
    if (success)
    {
        Debug.Log("Top 10 obtenido: " + jsonResponse);
    }
});
```
