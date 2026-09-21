# 🦀 CRAB GAME — Prototipo Unity (estilo Squid Game / Crab Game)

Prototipo 3D low-poly multijugador al estilo **Crab Game** (Dani) con vibes de **Squid Game**.

- Primera persona + avatares cangrejo visibles para otros
- Empujones, agarres, knockback fuerte → caos constante jugador-jugador y jugador-entorno
- 7 modos: Hat King, Bomb Tag, Rey de la Colina, Piso de Cristal, Carrera, Suelo es Lava, Stick Fight
- Funciona **sin setup**: juegas vs 7 bots. **Online** opcional con Photon PUN 2 (gratis, 20 CCU).

## Requisitos

- Unity Hub + **Unity 2022.3 LTS** (probado en 2022.3.21f1)
- Módulos: Windows/Mac/Linux Build, (opcional Android/iOS)

## Abrir el proyecto

1. Unity Hub → Open → selecciona la carpeta `crabgame/` (la que contiene `Assets/`, `Packages/`, `ProjectSettings/`).
2. Deja que Unity importe. Ignora warnings de `FindObjectOfType` (son inofensivos en 2022).
3. Abre `Assets/Scenes/MainMenu.unity` → Play.
   - Gracias a `AutoBoot.cs`, aunque las escenas estén vacías se auto-crea todo (cámara, GameManager, HUD, red).

Si Unity pide Safe Mode por algún script: avísame con el error, está pensado para compilar limpio sin paquetes externos.

## Cómo jugar (offline inmediato)

- En el menú elige modo → **JUGAR**
- `WASD` moverse · Mouse mirar · `Espacio` saltar · `Shift` correr
- `Click izq / E` empujar (robar sombrero, pasar bomba, tirar al vacío)
- `F` agarrar / soltar
- Stick Fight: `Click der / G` golpe con palo
- `Esc` libera el mouse

Rondas cortas → pantalla de ganador → Repetir / Siguiente modo / Menú.

## Activar ONLINE multiplayer (Photon PUN 2, ~10 min)

1. En Unity: `Window → Asset Store` (o Package Manager) → busca **PUN 2 - Free** → Import.
   - Al importar se define `PUN_2_0` y se activa automáticamente el código online en `NetworkBootstrap.cs`.
2. Crea cuenta gratis en https://dashboard.photonengine.com → crea app **PUN Classic (Realtime)** → copia el **AppId**.
3. En Unity: `Window → Photon Unity Networking → PUN Wizard` → pega AppId (o edita `Resources/PhotonServerSettings`).
4. Play en `MainMenu` → JUGAR → crea/une sala `crab-prototipo` (12 jugadores).
   - Sin Photon instalado el botón JUGAR carga `Game` offline con bots (no rompe nada).

> Nota: el spawn online inicial es offline-first. Para sincronización total (posición de cada cangrejo) el siguiente paso es añadir `PhotonView + PhotonTransformView` al prefab del jugador (ver `TODO-ONLINE.md` si lo generas). El prototipo ya deja la arquitectura lista: `GameManager.players`, `CrabPlayer.MoveExternal`, modos por eventos.

## Estructura

```
Assets/Scenes/MainMenu.unity, Game.unity   (vacías a propósito, AutoBoot las rellena)
Assets/Scripts/Core/    GameManager.cs, GameModeBase.cs, GameConfig.cs, AutoBoot.cs, SceneBootstrap.cs
Assets/Scripts/Player/  CrabPlayer.cs (movimiento FPS + push/grab/stick + knockback), CrabAvatarBuilder.cs
Assets/Scripts/Bots/    SimpleBot.cs (IA caótica por modo)
Assets/Scripts/GameModes/ HatKing, BombTag, KingOfHill, GlassFloor, Race, FloorIsLava, StickFight
Assets/Scripts/Maps/    MapBuilder.cs (arenas low-poly por código, sin assets) + GlassTile, Spinner, FinishTrigger
Assets/Scripts/Network/ NetworkBootstrap.cs (Photon con #if PUN_2_0, fallback bots)
Assets/Scripts/UI/      MainMenuUI.cs, GameHUD.cs (IMGUI, sin prefabs)
```

## Modos incluidos (esencia Crab Game)

| Modo | Qué haces | Gana |
|------|-----------|------|
| 👑 Hat King | Roba el sombrero empujando | Más tiempo con sombrero |
| 💣 Bomb Tag | Pasa la bomba tocando | No tenerla al explotar / último vivo |
| ⛰️ Rey Colina | Quédate en zona amarilla | Más puntos en zona |
| 🪟 Cristal | Cruza puente, un vidrio falso por par | Primero en cruzar |
| 🏁 Carrera | Esquiva molinos, empuja | Primero en meta |
| 🌋 Lava | Lava sube, sube a plataformas | Último vivo |
| 🥢 Stick | Todos con palo, knockback x1.8 | Último en arena |

Todos comparten la misma interacción base: **empujar = poder**. Caer bajo `y<-20` = eliminado.

## Siguientes pasos sugeridos

1. PhotonView sync (posición + sombrero/bomba vía RPC ya preparado en `OnPushLanded`).
2. Lobby con lista de salas + nombres + colores elegibles.
3. Sonidos (pop al empujar, boom, fanfarria) + partículas low-poly.
4. Rondas encadenadas estilo torneo Squid Game (puntos acumulados, eliminación definitiva).
5. Skins de cangrejo + sombreros.

Hecho con primitivas Unity, cero assets de pago. ¡A empujar cangrejos! 🦀
