# 🍎 Catch the Fruit

**Catch the Fruit** es un juego arcade 2D hecho en Unity. Controlas una canasta en la parte inferior de la pantalla y debes atrapar la mayor cantidad de frutas posible en 60 segundos, evitando las bombas y sin dejar que las frutas toquen el suelo. Con el paso del tiempo los objetos caen más rápido y con más frecuencia. ¿Podrás superar tu récord?

---

## 🎮 Controles

| Acción | Teclas |
|---|---|
| Mover a la izquierda | `A` o `←` |
| Mover a la derecha | `D` o `→` |
| Pausar / continuar | `Esc` o `P` |
| Navegar los menús | Ratón (clic en los botones) |

---

## 🍉 Puntos de cada objeto

| Objeto | Puntos | Velocidad base | Frecuencia de aparición |
|---|---|---|---|
| 🍎 Manzana | **10** | 3.0 | Muy común (peso 50) |
| 🍊 Naranja | **15** | 3.5 | Común (peso 25) |
| 🍉 Sandía | **20** | 2.5 | Poco común (peso 15) |
| 🍇 Uvas | **25** | 4.0 | Rara (peso 10) |
| 💣 Bomba | **−1 vida** | 3.5 | 20 % de probabilidad a partir del segundo 15 |

> Si una fruta toca el suelo pierdes una vida. Si una bomba toca el suelo no pasa nada.

---

## ⚙️ Sistemas

- **Vidas:** la partida empieza con **3 vidas**. Se pierde una al atrapar una bomba o al dejar caer una fruta. Al llegar a 0 termina la partida. Cada vida perdida provoca un temblor de cámara, un parpadeo rojo de la canasta y un sonido.
- **Cronómetro de 60 s:** el HUD muestra el tiempo restante (`m:ss`), que se pinta de rojo en los últimos 10 segundos. Al llegar a 0 la partida termina. Se congela durante la pausa (`Time.timeScale = 0`).
- **Dificultad progresiva:** a lo largo de los 60 segundos el intervalo entre objetos baja de **1.5 s a 0.5 s** y la velocidad de caída sube de **×1 a ×2**, siguiendo una `AnimationCurve` configurable. Las bombas empiezan a aparecer a partir del **segundo 15**.
- **Récord con PlayerPrefs:** al terminar la partida, si la puntuación supera el récord se guarda en `PlayerPrefs` (clave `CatchTheFruit.HighScore`) y se muestra el aviso **«¡Nuevo récord!»** con un sonido especial. El récord se muestra en el menú principal y en la pantalla de Game Over.

---

## 🖥️ Pantallas del juego

1. **Menú principal** (`MainMenu`): título, récord actual y botones *Jugar* e *Instrucciones*.
2. **Instrucciones:** panel con los controles y los puntos de cada fruta (leídos directamente de los prefabs, así nunca quedan desactualizados).
3. **Juego** (`Game`): HUD con puntos, cronómetro y vidas.
4. **Pausa:** panel con *Continuar* y *Menú*; la música baja de volumen mientras está en pausa.
5. **Game Over:** motivo del final (*¡Se acabó el tiempo!* / *¡Te quedaste sin vidas!*), puntuación final, récord, aviso de nuevo récord y botones *Reiniciar* y *Menú*.

**Flujo:** Menú → Instrucciones → Jugar → Pausa → Continuar → Game Over → Reiniciar → Menú.

---

## 📂 Estructura de scripts

Todos los scripts están en `Assets/Scripts/`.

| Script | Qué hace |
|---|---|
| `GameManager.cs` | Singleton que controla puntos, vidas, cronómetro, pausa y fin de partida, y emite eventos para la UI y el audio. |
| `Spawner.cs` | Genera frutas (por peso) y bombas en posiciones X aleatorias y aplica la dificultad progresiva con una corrutina. |
| `FallingObject.cs` | Mueve cada fruta o bomba hacia abajo con balanceo, guarda sus puntos y se destruye al salir de la cámara. |
| `BasketController.cs` | Mueve la canasta con el Input System (A/D y flechas) limitándola a los bordes de la cámara. |
| `BasketCatcher.cs` | Trigger de la canasta: suma puntos al atrapar fruta y quita una vida al atrapar bomba. |
| `BasketFeedback.cs` | Efectos de la canasta: rebote de escala al atrapar y parpadeo rojo al perder una vida. |
| `Ground.cs` | Trigger del suelo: quita una vida cuando una fruta cae sin ser atrapada. |
| `CameraShake.cs` | Hace temblar la cámara brevemente al perder una vida. |
| `GameHUD.cs` | Actualiza los textos de puntos, vidas y cronómetro escuchando los eventos del GameManager. |
| `PauseMenu.cs` | Detecta Esc/P para pausar y muestra u oculta el panel de pausa. |
| `GameOverPanel.cs` | Muestra el panel de fin de partida con motivo, puntuación, récord y botones. |
| `MainMenuUI.cs` | Controla el menú principal, el panel de instrucciones y muestra el récord. |
| `AudioManager.cs` | Singleton persistente (`DontDestroyOnLoad`) que reproduce la música y los efectos de sonido. |
| `HighScoreStore.cs` | Clase estática que lee y guarda el récord en `PlayerPrefs`. |
| `SceneLoader.cs` | Carga escenas restableciendo `Time.timeScale = 1`. |

---

## 🛠️ Tecnologías

- **Unity 6.6** (6000.6.2f1) con Universal Render Pipeline 2D
- **C#**
- **Input System** (nuevo sistema de entrada de Unity)
- **Rigidbody2D** (cinemáticos) y **Collider2D** (triggers) para el movimiento y las colisiones
- **Prefabs** para cada fruta, la bomba y el AudioManager
- **Canvas / TextMeshPro** para toda la interfaz
- **Coroutines** para el spawner, efectos visuales y sonidos retardados
- **PlayerPrefs** para guardar el récord

---

## 🤖 Desarrollo con MCP for Unity

### ¿Qué es?

[**MCP for Unity**](https://github.com/CoplayDev/unity-mcp) es un paquete de Unity que implementa el **Model Context Protocol (MCP)**, un estándar abierto que permite a asistentes de IA usar herramientas externas. Con él, un asistente como **Claude Code** puede comunicarse directamente con el editor de Unity: crear y modificar escenas, GameObjects, componentes, prefabs y scripts, leer la consola, entrar en Play Mode y ejecutar código dentro del editor.

### ¿Cómo se conectó Claude Code al editor?

1. Se instaló el paquete en Unity desde el **Package Manager** con la URL de Git `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` (queda registrado en `Packages/manifest.json`).
2. Desde la ventana de **MCP for Unity** dentro del editor se inició el servidor local y se configuró **Claude Code** como cliente, lo que registra el servidor MCP en la configuración de Claude Code.
3. Al abrir Claude Code en la carpeta del proyecto, las herramientas de Unity (`manage_scene`, `manage_gameobject`, `manage_script`, `read_console`, `manage_editor`, etc.) quedan disponibles y Claude puede actuar sobre el editor abierto.

### ¿Para qué se usó en cada etapa?

| Etapa | Uso de MCP for Unity |
|---|---|
| Instalación y configuración | Instalar el paquete y verificar la conexión entre Claude Code y el editor. |
| Escena Game y canasta | Crear la escena `Game`, la cámara, la canasta con `Rigidbody2D` + `BoxCollider2D` y el script de movimiento. |
| Prefabs y spawner | Crear los prefabs de las frutas y la bomba con sus sprites, colliders y valores de puntos, y configurar el `Spawner`. |
| Colisiones, puntuación y vidas | Crear el `GameManager`, los triggers de canasta y suelo, y probar las colisiones en Play Mode. |
| Cronómetro y dificultad | Añadir el cronómetro de 60 s y ajustar los parámetros de dificultad desde el Inspector. |
| Menú, HUD, pausa y Game Over | Construir los Canvas con TextMeshPro, los paneles y botones, la escena `MainMenu` y el Build Settings. |
| Sonido y efectos | Importar los audios, crear el prefab `AudioManager`, asignar los clips y añadir animaciones (rebote, parpadeo, temblor de cámara). |
| Revisión final | Revisar la consola, recorrer el flujo completo en Play Mode, limpiar logs innecesarios y verificar Build Settings. |

En todas las etapas se usó `read_console` tras cada cambio de código para detectar errores de compilación y Play Mode para comprobar el comportamiento.

---

## 🎵 Créditos de audio

Todos los efectos de sonido y la música de fondo provienen de **[Pixabay](https://pixabay.com/)** y se usan bajo la [licencia de contenido de Pixabay](https://pixabay.com/service/license-summary/).

- Música de fondo — Pixabay
- Efectos: atrapar fruta, bomba, perder vida, Game Over, nuevo récord y clic de botón — Pixabay

---

## 🚀 Cómo abrir el proyecto

1. Clona el repositorio:
   ```bash
   git clone https://github.com/MilcaTorres/Catch-the-Fruit.git
   ```
2. Abre **Unity Hub** → **Add** → **Add project from disk** y selecciona la carpeta `Catch-the-Fruit`.
3. Ábrelo con **Unity 6.6** (6000.6.2f1 o compatible). La primera vez Unity reimportará los assets y descargará los paquetes.
4. Abre la escena `Assets/Scenes/MainMenu.unity` y pulsa **Play**.

> Para generar un ejecutable: **File → Build Profiles**. Las escenas ya están configuradas: `MainMenu` (índice 0) y `Game` (índice 1).
