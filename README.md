# Digital Watchman 🎮

> **Un videojuego de concienciación sobre el ciberacoso basado en Estructuras de Datos**

**Universidad del Norte 
**Estructuras de Datos I — 2026-10**

---

## 👥 Equipo — Anthropic Dev Studio

| Rol | Integrante |
|-----|-----------|
| Director de Programación y Gerente de Proyecto | Jose Fernando Van Strahlen Garcia |
| Directora de UI/UX | Daniela Idarraga |
| Director de Documentación | Jairo Hernandez |
| Director de Pruebas | Jeus Esguerra |

**Profesor:** Daniel Romero Martínez  
**Fecha:** Barranquilla, abril de 2026

---

## 📋 Resumen

Digital Watchman es un videojuego de acción en 3D desarrollado en **Unity 6** con **C#**, cuyo objetivo es generar conciencia sobre el ciberacoso a través de una narrativa inmersiva ambientada en una ciudad futurista donde robots hackeados acosan digitalmente a víctimas inocentes.

La propuesta integra **cuatro estructuras de datos** con roles funcionales reales dentro del gameplay:

- `Queue` (FIFO) — spawn dinámico de enemigos por misión
- `Stack` (LIFO) — registro de acciones del jugador visible en Game Over
- `LinkedList` — gestión encadenada de misiones e inventario
- `File I/O` — persistencia del progreso en formato JSON

Adicionalmente se implementaron tres TADs: **Item**, **Misión** y **DatosGuardado**.

---

## 🎯 Introducción

El ciberacoso afecta a más del 40% de los jóvenes entre 12 y 17 años. Los videojuegos emergen como herramienta pedagógica de alto potencial para abordar esta problemática de forma accesible.

En Digital Watchman el jugador asume el rol de un **vigilante urbano** que interviene para proteger víctimas, rastrear el origen de los ataques y neutralizar a los agresores (robots de mantenimiento de red hackeados y reprogramados para acosar digitalmente a ciudadanos vulnerables).

---

## 🕹️ Descripción del juego

### Misiones

| # | Título | Objetivo | Enemigos | Recompensa |
|---|--------|----------|----------|------------|
| 1 | ¿Que esta pasando Rony? | Derrota a los bots que acosan a Rony | 4 | +100 pts / +30 HP |
| 2 | Guarida | Encuentra y desactiva los bots agresores en la guarida | 8 | +200 pts / +20 HP |
| 3 | Confrontación final | Derrota al boss principal — VOID_K3RN3L | 1 boss | +500 pts / +50 HP |

### Controles
- **WASD** — Movimiento
- **Clic izquierdo** — Atacar
- **E** — Usar ítem del inventario

---

## 🛠️ Plataforma tecnológica

- **Motor:** Unity 6 (6000.0.42f1)
- **Lenguaje:** C#
- **Estructuras de datos:** `System.Collections.Generic` — `Queue<T>`, `Stack<T>`, `LinkedList<T>`
- **Serialización:** `JsonUtility` (Unity)
- **Modelos y animaciones:** Mixamo
- **UI:** Canvas + TextMeshPro
- **IA utilizada:** Claude (Anthropic) — revisión de código, detección de bugs, documentación técnica

---

## 📐 Arquitecturas de datos implementadas

### Queue (Cola) — FIFO
**Archivo:** `MissionManager.cs`, `EventQueue.cs`

Controla el spawn dinámico de enemigos por misión. Al iniciar cada misión se cargan N tokens en la cola (uno por enemigo requerido). Cada vez que un enemigo muere se ejecuta `Dequeue()` y se instancia el siguiente enemigo en escena.

```csharp
// Carga de la Queue al iniciar misión
colaDeSpawn.Clear();
for (int i = 0; i < mision.enemigosRequeridos; i++)
{
    colaDeSpawn.Enqueue(i + 1); // el número identifica al enemigo
}

// Dequeue al spawnear
int numeroEnemigo = colaDeSpawn.Dequeue();
Vector3 posicion = CalcularPosicionSpawn();
Instantiate(enemigoPrefab, posicion, Quaternion.identity);
enemigosVivosEnEscena++;
```

---

### Stack (Pila) — LIFO
**Archivo:** `ActionStack.cs`

Registra las acciones del jugador durante la partida mediante `Push()`. Al morir, la pantalla de Game Over llama `ObtenerUltimas(6)` que realiza `Pop()` sobre una copia del Stack, mostrando las 6 acciones más recientes (la última acción aparece primero).

```csharp
// Declaración
private Stack<string> acciones = new Stack<string>();

// Push desde PlayerAttack cuando golpea
if (ActionStack.Instance != null)
    ActionStack.Instance.Registrar("Atacaste a " + hit.name + " (-" + attackDamage + " HP)");

// Pop en Game Over — ObtenerUltimas()
public List<string> ObtenerUltimas(int cantidad)
{
    List<string> resultado = new List<string>();
    Stack<string> copia = new Stack<string>(new Stack<string>(acciones));
    int count = 0;
    while (copia.Count > 0 && count < cantidad)
    {
        resultado.Add(copia.Pop());
        count++;
    }
    return resultado;
}
```

---

### LinkedList (Lista Enlazada)
**Archivos:** `MissionManager.cs`, `Inventario.cs`

Para **misiones**: cada nodo contiene un objeto `Mision`; al completar una misión se avanza con `misionActualNodo.Next` en O(1). Para el **inventario**: `AddLast()` agrega ítems al recogerlos y `Remove(nodo)` los elimina al usarlos, también en O(1).

```csharp
// Declaración
private LinkedList<Mision> misiones = new LinkedList<Mision>();
private LinkedListNode<Mision> misionActualNodo;

// Avanzar al completar misión
misionActualNodo = misionActualNodo.Next;

// Inventario — agregar ítem
items.AddLast(item);

// Inventario — usar y eliminar ítem (recorrido con .Next)
LinkedListNode<Item> nodo = items.First;
while (nodo != null)
{
    if (nodo.Value.nombre == nombreItem)
    {
        AplicarEfecto(nodo.Value);
        items.Remove(nodo); // O(1) — ventaja clave de LinkedList
        return true;
    }
    nodo = nodo.Next;
}
```

---

### File I/O — Persistencia JSON
**Archivo:** `FileManager.cs`

Al completar cada misión, `JsonUtility.ToJson()` serializa el objeto `DatosGuardado` y `File.WriteAllText()` lo escribe en `Application.persistentDataPath`.

Datos persistidos: nombre del jugador, puntaje, vida actual, misión activa, últimos 10 eventos de la Queue, fecha de guardado.

```csharp
// Guardar
DatosGuardado datos = new DatosGuardado();
datos.nombreJugador = nombreJugador;
datos.puntaje = puntaje;
datos.vidaActual = vidaActual;
datos.misionActual = misionActual;
datos.fechaGuardado = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
string json = JsonUtility.ToJson(datos, true);
File.WriteAllText(rutaGuardado, json);

// Cargar
if (!File.Exists(rutaGuardado)) return null;
string json = File.ReadAllText(rutaGuardado);
DatosGuardado datos = JsonUtility.FromJson<DatosGuardado>(json);
```

---

## 🏗️ Arquitectura del sistema

El sistema está compuesto por **22 scripts** que se comunican mediante el patrón **Singleton**, permitiendo acceso global sin dependencias directas entre scripts de gameplay.

```
Jugador                    MissionManager              Enemigo / BYTE-X
├── PlayerMovement         ├── LinkedList misiones      ├── EnemyMovement + Attack
├── Camera                 └── Queue spawn enemigos     └── EnemyHealth + ItemDropper
└── PlayerAttack + Health

Estructuras de datos
├── Queue (FIFO)  →  Spawn de enemigos
├── Stack (LIFO)  →  Últimas acciones
├── LinkedList    →  Inventario + misiones
└── File I/O      →  Guardar partida JSON

Sistemas de UI
├── HUDManager      →  Vida · puntaje · misión
├── InventarioUI    →  Items · efectos
├── DialogueSystem  →  NPC Carlos · narrativa
└── GameOverScreen  →  Stack · últimas acciones

FileManager  →  persistentDataPath · partida.json
```

---

## 📊 Estado de implementación

> Estado al **27 de abril de 2026** (primera entrega — >70% funcionalidad requerida)

| Componente | Estructura asociada | Estado |
|-----------|---------------------|--------|
| Movimiento WASD + física + animaciones | — | ✅ Completo |
| Sistema de combate con OverlapSphere | — | ✅ Completo |
| IA de enemigos (persecución y ataque) | — | ✅ Completo |
| Queue — spawn dinámico de enemigos | `Queue<int>` | ✅ Completo |
| Stack — historial en Game Over | `Stack<string>` | ✅ Completo |
| LinkedList — misiones e inventario | `LinkedList<Item>` | ✅ Completo |
| File I/O — guardado en JSON | `System.IO` | ✅ Completo |
| TADs: Item, Misión, DatosGuardado | `[Serializable]` | ✅ Completo |
| Sistema de drops con probabilidades | `Random.Range` | ✅ Completo |
| HUD con vida, misión y puntaje | `TMP_Text` | ✅ Completo |
| Pantalla de Game Over | `Stack LIFO` | ✅ Completo |
| Inventario con grilla dinámica | `LinkedList` | ✅ Completo |
| Sistema de diálogo con NPC víctimas | `Coroutines` | ✅ Completo |
| Menú de pausa | — | ⏳ Pendiente |
| Componente inclusivo (subtítulos) | — | 🔜 Trabajo futuro |
| Prueba de usabilidad | — | ⏳ Pendiente |
| Menú de inicio | — | ⏳ Pendiente |
| Sonido y música | — | ⏳ Pendiente |
| Leaderboard | — | ⏳ Pendiente |

---

## 📋 Requisitos no funcionales (FURPS)

| Atributo | Descripción |
|----------|-------------|
| **Functionality** | Sin errores en compilación ni runtime. Cada estructura de datos tiene un rol verificable en el gameplay. |
| **Usability** | Controles intuitivos (WASD + clic). HUD con información clara. Sistema de ayuda accesible desde el menú. |
| **Reliability** | Sistema de guardado robusto ante fallos. Inputs inválidos no provocan crashes. IA de enemigos predecible. |
| **Performance** | Mínimo 30 FPS en hardware de gama media. Escrituras a disco asíncronas para evitar micro-lag. |
| **Supportability** | Código modular con patrón Singleton. Scripts documentados. Arquitectura extensible para nuevas misiones. |

---

## 🗺️ Trabajo futuro (entrega final)

- [ ] Componente inclusivo de accesibilidad (subtítulos)
- [ ] Menú principal con selección de nombre del jugador
- [ ] Leaderboard con LinkedList ordenada
- [ ] Mensajes motivadores al completar misiones
- [ ] Prueba de usabilidad con 5 usuarios
- [ ] Sonido y música
- [ ] Menú de pausa

---

## 📚 Referencias

1. UNESCO, *Behind the numbers: Ending school violence and bullying*, UNESCO Publishing, Paris, 2019.
2. J. McGonigal, *Reality Is Broken: Why Games Make Us Better and How They Can Change the World*, Penguin Press, New York, 2011.
3. M. Pallavicini, A. Ferrari y F. Mantovani, "Video games for well-being," *Frontiers in Psychology*, vol. 9, p. 2127, 2018.
4. CONICET, "Videojuegos para prevenir el grooming y el bullying," 2022.
5. Universidad Complutense de Madrid, "Un videojuego pone al usuario en la piel de víctimas de cyberbullying," OTRI-UCM, 2021.
6. SENA Colombia, "Videojuegos educativos para la prevención del acoso escolar," 2020.
7. I. Millington y J. Funge, *Artificial Intelligence for Games*, 2nd ed., Morgan Kaufmann, 2009.
8. J. Gregory, *Game Engine Architecture*, 3rd ed., CRC Press, 2018.
9. K. Salen y E. Zimmerman, *Rules of Play: Game Design Fundamentals*, MIT Press, 2004.
10. Unity Technologies, "Unity Scripting API: Application.persistentDataPath," Unity Documentation, 2024.

---

> *Digital Watchman — Estructuras de Datos I 2026-10 — Universidad del Norte*  
> *Anthropic Dev Studio*
