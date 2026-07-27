# 🚀 Asten BaaS Unity SDK (v1.0.0)

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com/)
[![UPM Compatible](https://img.shields.io/badge/UPM-Compatible-brightgreen.svg)](https://docs.unity3d.com/Manual/Packages.html)
[![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Android%20%7C%20iOS%20%7C%20PC-lightgrey.svg)]()
[![Discord Community](https://img.shields.io/badge/Discord-Únete%20al%20Servidor-7289da.svg)](https://discord.gg/uttmKWfDNU)
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()

El **SDK oficial de C#** para conectar tus videojuegos de **Unity** con la infraestructura de **Asten BaaS** (Backend-as-a-Service). Diseñado para estudios independientes y desarrolladores profesionales que necesitan autenticación segura, persistencia de datos en la nube (Cloud Saves) y tablas de clasificación globales sin administrar servidores ni escribir código backend.

---

## 🌟 Características Principales

* ⚡ **Cero Configuración de Servidores:** Olvídate de configurar bases de datos, APIs de AWS o gestionar tokens; el SDK se encarga de todo el ciclo de vida en la nube.
* 🛡️ **Autenticación Híbrida y Segura:** Soporte para inicio de sesión silencioso/anónimo mediante **Device ID** y registro formal con **Correo Electrónico & Verificación OTP de 6 dígitos**.
* 🧠 **Cloud Saves con Debouncing Inteligente:** Sistema de guardado en MongoDB Atlas con protección de frecuencia integrada (Cooldown de 3 segundos) para proteger los FPS de tu juego y evitar costosos picos de peticiones en la red.
* 🏆 **Leaderboards Globales en Tiempo Real:** Registra puntuaciones, consulta tops competitivos y gestiona medallas (🥇🥈🥉) de forma instantánea.
* 🌐 **100% Compatible con WebGL y Móvil:** Construido sobre `UnityWebRequest` nativo y asíncrono. Funciona de manera impecable en juegos para navegadores (**itch.io, Poki, CrazyGames**), Android, iOS, PC, Mac y Consolas sin bloquear el hilo principal (Main Thread).
* 📦 **Aislamiento Limpio (`.asmdef`):** Cero advertencias amarillas en la consola y total compatibilidad con paquetes populares como TextMeshPro, Cinemachine o Newtonsoft.Json.

---

## 👉 Portal Web Asten BaaS y Obtención de Credenciales

Para comenzar a utilizar este SDK en tu proyecto de Unity, necesitas un **Game ID** y una **API Key**. Puedes generar y administrar todo tu entorno en la nube de forma gratuita desde nuestra consola oficial:

### 🔗 **[Plataforma Web Asten BaaS](https://baas.astenstudios.com)**

### 🔑 Paso a Paso: ¿Cómo obtener tus credenciales en 1 minuto?
1. **Regístrate o Inicia Sesión:** Entra a [baas.astenstudios.com](https://baas.astenstudios.com) y crea tu cuenta de desarrollador o estudio de videojuegos.
2. **Crea un Nuevo Videojuego:** En el panel principal de tu cuenta, haz clic en el botón **`+ Crear`** o selecciona **Nuevo Videojuego**.
3. **Ingresa los Detalles:** Asigna un nombre a tu juego (ej: *My RPG Game*) y una breve descripción. Haz clic en **Crear**.
4. **Guarda tus Credenciales Seguras:** Al crearse el juego exitosamente, la consola te mostrará una ventana emergente con tus dos credenciales de integración:

   ![Credenciales Generadas en la Consola Asten](Documentation~/Images/web-console-credentials.png)

   * **ID DEL JUEGO (GAME_ID)**: Es el identificador público único de tu videojuego (ej: `9bdc7e8f-7e31-...`). Cópialo usando el botón **`Copiar`**.
   * **CLAVE API (BEARER TOKEN / API KEY)**: Es tu clave secreta de acceso para autorizar peticiones desde Unity (ej: `astn_live_df74...`).  
     > ⚠️ **¡ADVERTENCIA DE SEGURIDAD!** Cópiala y guárdala en un gestor de contraseñas o archivo seguro inmediatamente. Por razones de protección, la plataforma **no volverá a mostrar esta clave secreta** una vez que cierres la ventana.
5. ¡Listo! Con estas dos cadenas de texto ya puedes inicializar el SDK en Unity y dar vida a tu videojuego.

---

## 📦 Instalación en Unity (3 Métodos)

### Método 1: Unity Package Manager (URL de Git - Recomendado)
1. En el Editor de Unity, abre **Window** ➔ **Package Manager**.
2. Haz clic en el botón **`+`** (esquina superior izquierda) y selecciona **Add package from git URL...**
3. Pega la siguiente URL del repositorio oficial y haz clic en **Add**:
   ```text
   https://github.com/astenstudios/asten-baas-unity-sdk.git#v1.0.0
   ```
   *(Nota: Puedes omitir `#v1.0.0` para descargar siempre la última versión en desarrollo).*

### Método 2: Archivo `.unitypackage` (Asset Store / Descarga Directa)
1. Descarga el paquete oficial desde la [Tienda de Unity Asset Store]() o desde la sección de **Releases** en GitHub.
2. En Unity, abre **Assets** ➔ **Import Package** ➔ **Custom Package...** y selecciona el archivo `.unitypackage`.

### Método 3: Submódulo de Git (Para Desarrolladores Avanzados)
Si deseas modificar el código fuente dentro de tu propio repositorio de Git:
```bash
git submodule add https://github.com/astenstudios/asten-baas-unity-sdk.git Packages/com.astenstudios.baas
```

---

## 🕹️ Escena de Prueba Rápida (1-Click Quickstart Demo)

El SDK incluye una escena interactiva orientada a código (`AstenQuickstartDemo.cs`) construida con la interfaz nativa `OnGUI` de Unity. **No requiere configurar lienzos (Canvas), ni instalar TextMeshPro, ni arrastrar prefabs.**

> ⚠️ **¡IMPORTANTE: ¿Por qué sólo veo la carpeta `Runtime/` al instalar el paquete?**  
> Al instalar un paquete oficial en Unity Package Manager (UPM), las carpetas de demostración (`Samples~`) se ocultan intencionalmente dentro de `Packages/` para evitar que scripts de prueba compilen en tu proyecto final.  
> **Para acceder a la Demo y probar el SDK en 3 minutos, sigue estos pasos:**
> 
> 1. En el Editor de Unity, ve a **Window** ➔ **Package Manager**.
> 2. Selecciona **Asten BaaS SDK** en tu lista de paquetes instalados (o haz clic en la pestaña **All Samples** en el menú izquierdo).
> 3. Busca **Demo Scene & UI Components** y haz clic en el botón **`Import`**.
> 4. ¡Listo! Unity importará y copiará los archivos de demostración directamente a tu carpeta de Proyecto bajo:  
>    👉 **`Assets/Samples/Asten BaaS SDK/1.0.0/Demo Scene & UI Components/`**
> 5. Abre esa carpeta en tu ventana de Project.

### ⚙️ ¿Cómo configurar tus credenciales en la Demo (Paso a Paso a Prueba de Noobs)?
Para que la escena de demostración se conecte con tus bases de datos y no arroje error, debes asignarle tu **Game ID** y **API Key**:

1. En tu ventana de **Project**, navega a:  
   👉 `Assets/Samples/Asten BaaS SDK/1.0.0/Demo Scene & UI Components/` y abre la escena **`SampleScene.unity`** (o cualquier escena en blanco).
2. En tu ventana de **Hierarchy** (Jerarquía de la escena), haz clic en el objeto que tenga el script demo (o crea un objeto vacío con `Clic Derecho ➔ Create Empty` y llámalo `AstenDemo`).
3. **Arrastra y suelta** el archivo de script **`AstenQuickstartDemo.cs`** desde la ventana de Project hacia el panel **Inspector** de ese objeto (o haz clic en el botón `Add Component` y busca *Asten Quickstart Demo*).
4. En el Inspector del componente, verás los campos para colocar tus credenciales:

   ![Configuración del Inspector en Unity](Documentation~/Images/inspector-config.png)

5. Rellena los dos campos obligatorios:
   * **Game Id**: Pega el identificador de tu videojuego (puedes copiarlo desde tu panel en la [Consola Web Asten BaaS](https://baas.astenstudios.com)).
   * **Api Key**: Pega tu clave API secreta de entorno Sandbox o Producción (ej: `astn_live_...`).
6. ¡Listo! Presiona el botón **Play ▶️** en el Editor de Unity y verás la consola visual conectada con la nube.

### 🎮 Pestañas Interactivas de la Demo
Al presionar **Play ▶️** con tus credenciales configuradas, el panel de control te permitirá interactuar con 3 pestañas en tiempo real:
* **`[ 🎨 Perfil Visual ]`**: Muestra tu ID único de jugador, JWT abreviado, monedas en la nube y nivel actual.
   * **`[ 🏆 Top 5 Ranking ]`**: Consulta en vivo la tabla global con medallas de primero, segundo y tercer lugar.
   * **`[ 💻 JSON Técnico ]`**: Inspecciona las respuestas crudas del servidor para depuración técnica.

---

## 🛠️ Guía de Integración Paso a Paso

### 1. Inicialización del SDK
Antes de llamar a cualquier función de autenticación o guardado, debes inicializar el singleton `AstenSDK.Instance` al arrancar tu juego (por ejemplo, en un script `GameManager.cs` o en tu escena de carga principal):

```csharp
using UnityEngine;
using AstenBaaS;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        string gameId = "TU_GAME_ID_AQUÍ";
        string apiKey = "astn_live_TU_CLAVE_API_AQUÍ";
        
        // Opcional: Especificar servidor custom (por defecto apunta a la nube global de Asten)
        string backendUrl = "https://api.baas.astenstudios.com";

        // Inicializar el motor de Asten BaaS
        AstenSDK.Instance.Initialize(gameId, apiKey, backendUrl);
        Debug.Log("🚀 Asten SDK listo para usar.");
    }
}
```

---

### 2. Autenticación de Jugadores (`AstenSDK.Auth`)
El SDK gestiona de manera transparente el estado de la sesión. Una vez autenticado el usuario, el **Player Session Token (JWT)** se almacena en memoria y se inyecta en todas las cabeceras HTTP de forma automática.

#### 🎮 Inicio de Sesión Anónimo (Device ID - Ideal para Onboarding Rápido)
Registra e inicia sesión de forma silenciosa la primera vez que un jugador abre tu videojuego:
```csharp
AstenSDK.Instance.LoginWithDeviceId((success, response) =>
{
    if (success)
    {
        Debug.Log($"✅ Sesión de invitado activa. Player ID: {AstenSDK.Instance.ActivePlayerId}");
    }
    else
    {
        Debug.LogError($"❌ Error al entrar como invitado: {response}");
    }
});
```

#### 📧 Registro de Cuenta y Verificación OTP de 6 Dígitos
Para evitar cuentas falsas o spam, nuestro backend envía un código de seguridad OTP al correo del usuario al momento de registrarse:
```csharp
// 1. Solicitar registro (envía un correo con código de 6 dígitos al usuario)
AstenSDK.Instance.RegisterPlayer("jugador@studio.com", "miPasswordSeguro123!", (success, response) =>
{
    if (success)
    {
        Debug.Log("📬 Registro creado. Por favor revisa tu correo por el código de verificación.");
    }
});

// 2. Verificar código OTP (activa la cuenta e inicia sesión inmediatamente)
string codigoOtp = "123456"; // Código ingresado por el jugador en un InputField
AstenSDK.Instance.VerifyPlayerEmail("jugador@studio.com", codigoOtp, (success, response) =>
{
    if (success)
    {
        Debug.Log($"✅ ¡Correo verificado! Sesión activa. JWT: {AstenSDK.Instance.PlayerSessionToken}");
    }
});
```

#### 🔑 Inicio de Sesión Clásico con Correo y Contraseña
Para jugadores que regresan desde otro dispositivo o cerraron sesión previamente:
```csharp
AstenSDK.Instance.LoginPlayer("jugador@studio.com", "miPasswordSeguro123!", (success, response) =>
{
    if (success)
    {
        Debug.Log("✅ ¡Bienvenido de vuelta!");
    }
});
```

#### 🚪 Cerrar Sesión (Logout)
```csharp
AstenSDK.Instance.Logout(); // Limpia el ID y Token JWT en memoria
```

---

### 3. Persistencia de Datos en la Nube (`AstenSDK.CloudSaves`)

Asten BaaS te permite serializar y guardar en MongoDB Atlas cualquier clase o estructura de C# utilizando `JsonUtility`.

> **💡 Protección de Debouncing Integrada (Cooldown de 3 Segundos):**  
> Si tu código de juego intenta llamar a `SavePlayerData()` en un bucle rápido o varias veces por segundo (ej. al recoger monedas en un nivel), **el SDK encola inteligentemente la petición** y espera a que transcurran 3 segundos de inactividad antes de enviarla a la red. Esto garantiza un rendimiento máximo a 60+ FPS sin causar tirones (stuttering) ni saturar tu cuota de servidor.

#### 💾 Guardar Progreso del Jugador
```csharp
[System.Serializable]
public class DatosDePartida
{
    public int nivelActual;
    public int monedasNube;
    public string armaEquipada;
    public float[] posicionUltimoCheckpoint;
}

public void GuardarEnNube()
{
    DatosDePartida misDatos = new DatosDePartida
    {
        nivelActual = 15,
        monedasNube = 2500,
        armaEquipada = "Espada Excalibur",
        posicionUltimoCheckpoint = new float[] { 10.5f, 2.0f, -4.3f }
    };

    AstenSDK.Instance.SavePlayerData(misDatos, (success, response) =>
    {
        if (success)
        {
            Debug.Log("☁️ Progreso guardado con éxito en MongoDB Atlas.");
        }
        else
        {
            Debug.LogWarning("⏳ Error o petición en enfriamiento (Debounced): " + response);
        }
    });
}
```

#### 📥 Cargar Progreso desde la Nube
```csharp
public void CargarDesdeNube()
{
    AstenSDK.Instance.LoadPlayerData((success, jsonResponse) =>
    {
        if (success)
        {
            Debug.Log("☁️ JSON descargado: " + jsonResponse);
            
            // Convertir de vuelta a nuestra clase C#
            DatosDePartida datosCargados = JsonUtility.FromJson<DatosDePartida>(jsonResponse);
            Debug.Log($"Monedas recuperadas: {datosCargados.monedasNube}");
        }
        else
        {
            Debug.LogError("❌ No se pudo descargar la partida: " + jsonResponse);
        }
    });
}
```

---

### 4. Tablas de Clasificación Globales (`AstenSDK.Leaderboards`)

Crea competencias, gestiona puntuaciones altas y muestra rankings en vivo en cualquier plataforma.
> **Nota de Backend:** Por defecto, al crear un nuevo videojuego en el portal de Asten BaaS, se genera automáticamente una tabla de clasificación principal con el ID oficial: **`"leaderboard_score"`**. Puedes crear tablas personalizadas (ej. `"ranking_pvp"`, `"carreras_tiempo"`) desde la consola web.

#### 🏆 Publicar un Récord
El servidor cuenta con protección nativa de **High Score**: solo sobrescribirá el puntaje en la base de datos si el nuevo número supera al récord registrado previamente por el jugador.
```csharp
string leaderboardId = "leaderboard_score"; // ID de la tabla en tu consola
int puntosObtenidos = 9850;
string nombreEnPantalla = "ProGamer_2026";

AstenSDK.Instance.SubmitScore(leaderboardId, puntosObtenidos, nombreEnPantalla, (success, response) =>
{
    if (success)
    {
        Debug.Log("🏆 ¡Récord evaluado y publicado en la tabla global!");
    }
});
```

#### 🥇 Consultar el Top Ranking (Mejores Puntuaciones)
Descarga la lista descendente de los mejores jugadores para renderizarla en tu interfaz:
```csharp
int limiteResultados = 10; // Top 10 jugadores

AstenSDK.Instance.GetTopScores("leaderboard_score", limiteResultados, (success, jsonResponse) =>
{
    if (success)
    {
        Debug.Log("📊 Top Ranking recibido: " + jsonResponse);
        // Puedes iterar sobre las puntuaciones y mostrarlas con medallas en tu UI
    }
});
```

---

## 🔍 Inspección de Estado en Tiempo Real (Propiedades Públicas)
En cualquier momento durante la ejecución de tu juego, puedes acceder a las propiedades públicas de sesión desde tu código para verificar el estado de la conexión:
```csharp
// Verificar si hay un jugador conectado
if (AstenSDK.Instance.IsLoggedIn)
{
    string idJugador = AstenSDK.Instance.ActivePlayerId;     // UUID en MongoDB
    string tokenJwt  = AstenSDK.Instance.PlayerSessionToken; // Token de seguridad activo
    Debug.Log($"Jugador logueado con ID: {idJugador}");
}
```

---

## 🏛️ Arquitectura y Buenas Prácticas

1. **Sin Bloqueo del Hilo Principal (Main Thread Unblocking):** Todas las operaciones de red están encapsuladas en corrutinas asíncronas optimizadas bajo `UnityWebRequest`. En pruebas de rendimiento extremas (Stress Testing), el SDK mantiene una estabilidad superior a **300+ FPS** sin generar tirones ni recolectar basura de forma agresiva (Zero-Allocation practices).
2. **Compatibilidad con WebGL:** Al evitar sockets web pesados o librerías que dependen de `System.Net.Sockets` directos, el SDK es ideal para exportaciones en HTML5/WebGL.
3. **Manejo Resiliente de Errores (Offline/No-WiFi):** Si el jugador pierde su conexión a internet o el servidor responde con un código HTTP de error (`400 Bad Request`, `401 Unauthorized`), el SDK captura la excepción limpiamente e invoca tu callback con el mensaje legible para el usuario, sin crashear el juego ni arrojar excepciones en la consola.

---

## 💬 Comunidad, Soporte y Contribuciones

* 💬 **Servidor de Discord Oficial**: Únete a nuestra comunidad activa de desarrolladores, haz preguntas técnicas y comparte tus juegos en:  
  👉 **[https://discord.gg/uttmKWfDNU](https://discord.gg/uttmKWfDNU)**
* 📧 **Soporte Técnico Directo**: Escríbenos a **support@astenstudios.com** si necesitas asistencia personalizada o tienes problemas técnicos con tu integración.
* 🌐 **Plataforma Web**: [https://baas.astenstudios.com](https://baas.astenstudios.com)
* 🐛 **Reporte de Errores e Issues**: Si encuentras algún comportamiento inesperado, abre un [Issue en GitHub](https://github.com/astenstudios/asten-baas-unity-sdk/issues) o contáctanos directamente por Discord.

---

<p align="center">
  <b>Asten Studios © 2026</b><br>
  <i>Infraestructura backend de próxima generación para videojuegos hechos en Unity.</i>
</p>
