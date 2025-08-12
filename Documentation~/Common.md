# Common

The **`Solution.Common`** assembly is the foundational shared codebase designed to support a broad range of Unity projects, including all Unity Assets published by Solution on the Unity Asset Store.

It provides a comprehensive collection of **core utilities, architectural patterns and Unity-specific helpers** that serve as essential building blocks for gameplay systems, tools and services. Whether you are integrating Solution's assets or using `Solution.Common` as a standalone library in your own projects, this assembly centralizes recurring logic and patterns to keep your higher-level code **clean, consistent and maintainable**.

By consolidating these fundamental components, `Solution.Common` helps you avoid duplicated code, reduce coupling and ensure consistent behavior across your projects.

## What's inside

The assembly is organized into **namespaces**, each with a focused purpose:

* **`Solution.Common.Assets`** Runtime-safe asset retrieval combined with automatic editor-to-build settings synchronization.
* **`Solution.Common.Facade`** A lightweight, payload-driven Facade Pattern implementation for decoupling communication between systems.
* **`Solution.Common.Misc`** General-purpose helpers, utility types and editor-friendly development aids.
* **`Solution.Common.UnityExtension`** Unity-specific patterns and extensions for components, scene objects and rendering.

## Intended Benefits

* **Centralized utilities** Common code maintained in one place to reduce duplication and ease maintenance.
* **Editor-to-runtime safety** Tools and helpers designed to function seamlessly in both Unity Editor and runtime builds.
* **Loosely coupled systems** Encourages modular architecture that minimizes dependencies between unrelated parts of your project.
* **Reusable patterns** Ready-to-use singletons, facades and resolver patterns that can be dropped into any feature without rewriting boilerplate.
* **Inspector-friendly** Serializable types designed for straightforward configuration directly in Unity's Inspector.

## Important note

This documentation **does not cover every class in `Solution.Common`**, focusing on the most commonly used types relevant to general development. Internal helper classes, specialized implementations and low-level abstractions are intentionally omitted to maintain clarity and focus.

---

## Assets

The `Solution.Common.Assets` namespace provides a unified system for accessing, creating and synchronizing **project settings and configuration assets** between the Unity Editor and runtime environments.

Its primary role is to make sure that **editor-only ScriptableObject settings** are seamlessly available when the game runs in a build, without requiring direct asset database access. This is done by automatically exporting them to **JSON** in the `StreamingAssets` folder, where they can be loaded at runtime on any platform.

**Core responsibilities of this assembly:**

* **Centralized asset retrieval:** Developers can load settings with a single method call, without worrying about whether they're in the Editor or a build.
* **Automatic asset creation in the Editor:** Missing ScriptableObjects are generated on demand, reducing setup friction.
* **Editor-to-runtime synchronization:** When settings are saved or imported in the Editor, they're automatically serialized to JSON for use at runtime.
* **Build-safe loading:** Assets are loaded from JSON in the build to avoid relying on Unity's `AssetDatabase`, which is Editor-only.

**Typical usage flow:**

1. Developer requests an asset by name and type (e.g., `LoggingSettings`).
2. In the Editor:
   * If it doesn't exist, it's created and saved in `Assets/Settings`.
   * A JSON copy is exported to `StreamingAssets/Settings`.
3. At runtime:
   * The JSON is loaded and a fresh ScriptableObject instance is populated with the data.

## Facade Pattern in `Solution.Common.Facade`

The `Solution.Common.Facade` namespace provides a lightweight, payload-driven implementation of the **Facade Pattern** for Unity projects.
It's designed to decouple communication between gameplay systems, tools and UI, while still allowing type-safe and editor-friendly data flow.

At its core, the pattern in this assembly enables **services and sub-services** to exchange information via `ScriptablePayload` objects, without requiring direct knowledge of each other's internal implementation. This creates clean boundaries between systems, making them easier to maintain, test and extend.

---

### Key Concepts

#### **1. Services as Facades**

A **Facade** in this context is any system that exposes a clean, minimal API for interacting with potentially complex internal logic.
Instead of having multiple scripts directly reference and call each other's methods, they communicate through a single entry point that handles:

* Payload validation
* Routing to the correct sub-system
* Priority-based subservice selection
* Default/fallback processing when no subservice is eligible
* Error handling and logging

This greatly reduces coupling and dependency chains.

#### 2. Facade Settings
Facades can be configured through `FacadeSettings`.
These settings can be shared across multiple facades to ensure consistent behavior in a project.

To avoid writting down the same namespace over and over again for all object using the same facade script, you can predefine settings as shown below:
```csharp
[SerializeReference] private FacadeSettings facadeSettings = new FacadeSettings().WithNamespace("Assets.Scripts.Animation.AnimationSubServices");
```

Make sure to use `[SerializeReference]` and **NOT** `[SerializeField]` if you want to edit the predefined values in the inspector!

If you're using `AbstractFacadeBehaviour` to reduce boilerplate, you have to predefine settings in the constructor as shown below:
```csharp
public AnimationComponentController() {
    facadeSettings = new FacadeSettings().WithNamespace("Assets.Scripts.Animation.AnimationSubServices");
}
```

> **Note for predefined values:**
> Once your scene is loaded with the facade settings in one of your scripts, the values of the settings will stay, even if you modify your script with other predefined values.

#### **3. Payload-Based Communication**

The `ScriptablePayload` is the heart of the communication layer.
It represents **self-contained data packages** that can be passed between services.

* They are `ScriptableObject`s, which means they can be configured in the Editor.
* They can reference **other payloads** through the `Payload` property, enabling indirection and reusability.
* They are **type-safe**, so no fragile string-based lookups are needed.

---

### Why Use This Pattern in Unity?

In Unity, communication between systems often ends up as:

* Direct component references in the Inspector
* Static singletons scattered across the project
* Complex event buses without strong typing

The `Solution.Common.Facade` approach solves these problems by:

* Eliminating hard references between unrelated systems
* Allowing payloads to be configured and reused in multiple contexts
* Making systems easier to unit test without requiring scene dependencies

---

### Example: Enemy Hit Event

#### **Without Facade**

```csharp
// Tight coupling: Enemy references PlayerHealth directly
playerHealth.ApplyDamage(25);
```

If `PlayerHealth` changes, all references must be updated.

#### **With Facade**

```csharp
// Create a payload
[CreateAssetMenu(menuName = "Payloads/DamagePayload")]
public class DamagePayload : ScriptablePayload {
    public int amount;
}

// Send through the facade
combatService.Facade.RequestToSubService(damagePayload);
```

Here, the `combatService` acts as the facade. It knows how to handle `DamagePayload` without the sender needing to know about `PlayerHealth` or any other system.

---

## Implementing Subservices

Subservices are **specialized units of logic** that handle a specific `ScriptablePayload` type inside a Facade's context.
They're the building blocks that make the Facade pattern modular and extensible.

---

### 1. Implementing `ISubService` Directly

You'd implement `ISubService<TContext>` when you want **complete control** over request handling without inheriting from the helper class `AbstractSubService`.

Example: A **simple logger subservice** that writes a string payload to the console.

```csharp
using Solution.Common.Facade;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Payloads/LogMessagePayload")]
public class LogMessagePayload : ScriptablePayload {
    public string message;
}

public class LogMessageSubService : ISubService<MyFacadeContext> {

    public Type AcceptablePayloadType => typeof(LogMessagePayload);

    public void Request(MyFacadeContext context, ScriptablePayload payload, out object result) {
        var casted = (LogMessagePayload)payload;
        Debug.Log($"[{context.name}] says: {casted.message}");
        result = null; // No return value here
    }
}
```

---

### 2. Implementing `AbstractSubService` (Recommended)

`AbstractSubService<TContext, TPayload>` **removes boilerplate** by:

Example: A **healing subservice** for an RPG.

```csharp
using Solution.Common.Facade;
using UnityEngine;

[CreateAssetMenu(menuName = "Payloads/HealPayload")]
public class HealPayload : ScriptablePayload {
    public int healAmount;
}

public class HealSubService : AbstractSubService<PlayerController, HealPayload>, IHandles<HealPayload> {

    protected override object OnRequest(PlayerController context, HealPayload payload) {
        context.Health += payload.healAmount;
        return context.Health; // New health value
    }

    protected override IHandles<TPayload> Self<TSelf, TPayload>() => (IHandles<TPayload>)this;
}
```

---

### 3. Creating Subservice Categories

Sometimes you want **different types of subservices for the same context** without mixing them all into one giant list.
For example: `AnimationSubService`, `CombatSubService`, `InventorySubService`.

You do this by:

* Creating an abstract category type that inherits from `AbstractSubService`.
* Loading only that category type into your Facade's `SubServices` list.

Example: An **Inventory Category**.

```csharp
public abstract class InventorySubService 
    : AbstractSubService<InventoryController, InventoryPayload> { }
```

Now your `InventoryController` only loads types inheriting from `InventorySubService`.

```csharp
InventoryController() {
    Facade.InitializeFacade();
}
```

---

### 4. Practical Tips

* **Namespace separation:** Keep each category's subservices in its own namespace for cleaner reflection loading.
* **Payload type checking:** Let the Facade's `TryRequest` method do the heavy lifting for you, it will ensure type safety before calling the subservice.
* **Avoid storing heavy state** in subservices, they should be stateless or lightweight, with context state stored in the Facade's context object.

---

### Copies of Payloads

The `Payload` property in `ScriptablePayload` allows to copy a payload by referencing it in editor:

```csharp
public class DamageReferencePayload : ScriptablePayload {
    [SerializeField] private ScriptablePayload targetPayload;
    public override ScriptablePayload Payload => targetPayload;
}
```

## Misc `Solution.Common.Misc`

The `Solution.Common.Misc` namespace contains utility and helper classes that don't belong to a specific gameplay or subsystem category but are broadly useful across the project. Couple of the more important/usefull features are shown below:

### Filter Chain Pattern

The **Filter Chain** pattern in `Solution.Common.Misc` provides a reusable, thread-safe way to process a value through a series of filters that may modify, reject or pass it along unchanged.
It is implemented via the `IFilter<T>` interface and the `FilterChain<T>` class, which together support building modular, easily composable pipelines.

**Key features:**

* **Interface-based filters** Works with `MonoBehaviour` and non-`MonoBehaviour` classes alike, since filters implement an interface instead of inheriting from a base class.
* **Thread-safe** The filter chain is immutable after construction, allowing safe concurrent use.
* **Immutable filters list** Filters are stored in a `readonly` collection to avoid accidental modification after setup.
* **Short-circuiting** Any filter can stop the chain by returning a `FilterResult` that is marked as failed or rejected.
* **Rich result type** Processing produces a `FilterResult<T>` that contains:

    * The processed value (if successful)
    * A success flag
    * An optional message for diagnostics or logging

**Use cases:**

* Processing and validating player input or network messages before they reach the core gameplay logic.
* Chaining image, audio or data transformations in a controlled order.
* Applying gameplay modifiers, buffs or effects to an entity in sequence.
* Security checks or content sanitization before storage or display.

**Example usage:**

```csharp
var filters = new Filter<string>[] {
    new TrimFilter(),
    new LowercaseFilter(),
    new ProfanityCheckFilter()
};

var chain = new FilterChain<string>(filters);

var result = chain.Process("   Hello WORLD!   ");

if (result.Success) {
    Debug.Log($"Processed value: {result.Value}");
} else {
    Debug.LogWarning($"Processing failed: {result.Message}");
}
```

---

### GenericSubject<T>

A generic implementation of the **Observer pattern** combining `IObservable<T>` and `IObserver<T>`.
It allows multiple observers to subscribe and receive value, error and completion notifications.

**Key features:**

* Maintains an internal observer list.
* Handles error and completion propagation, with customizable behavior via `StopOnError` and `AllowCompletion`.
* Provides `Unsubscriber` to remove observers automatically when disposed.

**Use cases:**

* Broadcasting events or data changes to multiple subscribers without tight coupling.
* Creating reactive pipelines without external libraries.

---

### ISimpleObserver<T>

A minimal observer interface that inherits from `IObserver<T>` but provides **no-op** implementations for `OnCompleted` and `OnError`.

**Key features:**

* Implement only `OnNext` when error/completion handling is unnecessary.
* Reduces boilerplate for simple observation cases.

**Use cases:**

* Quick listeners for value changes where error handling is irrelevant.

---

### Optional<T>

A robust, immutable container representing an optional reference value of type `<typeparamref name="T"/>`.
This class provides a safer and more expressive alternative to using null references directly.

**Key features:**

* Explicitly indicates presence or absence of a value through the `HasValue` property.
* Enforces non-null wrapped values, preventing accidental null usage.
* Provides static factory methods `Some(T value)` and `None()` for clear construction semantics.
* Supports implicit conversion from `T` to `Optional<T>` for convenient usage.
* Offers `Match` methods for safe and expressive handling of both cases (value present or absent).
* Provides `GetValueOrDefault` to retrieve the wrapped value or a specified fallback.
* Overrides `Equals`, `GetHashCode` and `ToString` for proper value semantics and debugging friendliness.
* Designed as a `sealed` class to ensure immutability and prevent misuse via inheritance.

**Use cases:**

* Avoiding null reference exceptions by making optional values explicit.
* Improving API clarity by signaling optional parameters and return values explicitly.
* Simplifying conditional logic for presence/absence scenarios with pattern-like matching methods.
* Enhancing maintainability and readability by eliminating scattered null checks.

---


### PrototypeObject

A Unity `MonoBehaviour` for **rapid prototyping** of in-scene objects.
It can display identifying text in the Scene view and optionally adjust transparency while in the editor.

**Key features:**

* `objectRepresents`: a descriptive label displayed via scene gizmos.
* `transparentInEditorMode`: toggles alpha in the Unity editor for quick visual differentiation.
* Automatically positions the label at the object's center or the `SpriteRenderer` bounds.

**Use cases:**

* Marking placeholder objects in prototype levels.
* Providing quick, visual, in-editor notes about object purpose.

---

### StaticObjectCache

A global static cache that uses `ConditionalWeakTable` to store values associated with object keys.
Cached values are automatically removed when their keys are garbage-collected, preventing memory leaks.

**Key features:**

* `Cache(key, value)` and `Cache(key, factory)` methods for storing/retrieving values.
* `Remove(key)` to explicitly clear entries.
* `TryGet(key, out value)` to retrieve without creating new entries.
* Fully garbage-collection-aware.

**Use cases:**

* Attaching metadata or computed values to objects without modifying their structure.
* Memoizing expensive computations tied to an object's lifecycle.

---

### `CooldownTimer`

A lightweight helper for tracking cooldown durations.
Ideal for skills, actions or mechanics that need a delay before reuse.

**Key features:**

* `Duration`: total cooldown length.
* `Elapsed()`, `Remaining()`, `IsActive()` methods for status checks.
* Simple API for starting/resetting timers.

**Use cases:**

* Enforcing delays on player abilities.
* Controlling timed interactions in gameplay loops.

## UnityExtension `Solution.Common.UnityExtension`

The `Solution.Common.UnityExtension` namespace contains helper classes and extensions that simplify working with Unity components in the context of the *Solution* project.
These utilities are designed to reduce boilerplate code and encourage consistent architecture patterns across the codebase.

### ReferenceResolver<T>
`ReferenceResolver<T>` is a generic, serializable utility designed for **runtime lookup of scene objects by ID**.
It helps decouple object references from direct inspector assignments, allowing for more flexible scene setups and dynamic resolution at runtime.
The resolver can search for the target component in **multiple fallback steps** until it finds a match, caching the result for future calls.

**Key Features:**

* **Generic Component Resolution:** Works with any type derived from `UnityEngine.Component`.
* **Multiple Resolution Strategies:** Attempts resolution in the following order:
  1. **Cached Reference:** Returns previously found or assigned reference.
  2. **Relative Path Search:** Uses `id` as a hierarchy path relative to a given context.
  3. **Ancestor Search:** Searches parents and their children for a matching component.
  4. **Scene Search:** Scans all loaded objects in the scene, optionally matching by name.
* **Cached Results:** Once found, the reference is cached for future calls to improve performance.
* **Integrated Logging:** Automatically logs lookups, warnings and potential configuration issues via the shared `ICommonLogger`.

**Typical Use Case:**

* Linking UI elements, gameplay objects or controllers **without hardcoding references**.
* Resolving references in **prefabs** that need to adapt to different scene hierarchies.

**Usage Example:**

```csharp
public class PlayerHUD : MonoBehaviour {
    [SerializeField] private ReferenceResolver<HealthBar> healthBarRef;

    private void Start() {
        if (healthBarRef.TryGet(transform, out var healthBar)) {
            healthBar.SetValue(100);
        } else {
            Debug.LogWarning("HealthBar could not be resolved!");
        }
    }
}
```

---

### SingletonMonoBehaviour<T>

**Key Features:**

* **Automatic Singleton:** Automatically ensures only one instance of a component exists.
* **Generic Type Parameter:** Pass the component type to enable type-safe access through `Instance`.
* **Optional Persistence:** Control persistence across scene loads with `dontDestroyOnLoad`.
* **Logging Access:** Provides a `Log` property for unified logging via project settings.

**Usage Example:**

```csharp
public class GameManager : SingletonMonoBehaviour<GameManager> {
    private void Start() {
        Log.LogInfo("Game Manager started!");
    }
}
```

### SortingLayerMask
`SortingLayerMask` is a compact, serializable value type for working with Unity sorting layers as a bitmask. It wraps Unity's `SortingLayer.layers` into a small API that makes it easy to build, combine, query and serialize sets of sorting layers inside your code or inspector-friendly objects.

**Why this exists:**

* Represent multiple sorting layers with a single integer (cheap bitwise ops).
* Make checks like "does this mask include layer X" readable and type-safe.
* Be serializable and inspector-friendly so designers can store layer sets on components and ScriptableObjects.
* Provide convenience helpers for common operations: create-from-name/index, union, intersection, inversion, enumeration and comparisons.

**Quick API summary:**

* Static: `None`, `All`.
* Creation: `FromIndex(int)`, `FromName(string)`, constructor `SortingLayerMask(int)`.
* Queries: `IncludesLayer(int index)`, `IncludesLayer(string name)`, `IncludesLayerID(int id)`, `IsEmpty`, `IsSingleLayer`, `SortingLayersContained`, `EnumerateLayerNames()`.
* Comparison: `ContainsAll`, `IsSupersetOf`, `IsSubsetOf`, `Overlaps`, `Equals` and value-type overrides.
* Mask ops: `WithAddedLayer`, `WithoutLayer`, `Intersect`.
* Operators: `|`, `&`, `~`, `==`, `!=`.
* Conversions: implicit `int` -> raw mask, explicit `SortingLayerMask(int)` -> mask.
* Debugging: nicely formatted `ToString()` listing layer names; `DebuggerDisplay` attribute.

#### Examples

Create masks

```csharp
// single layer by name
var foreground = SortingLayerMask.FromName("Foreground");

// combine layers
var combined = foreground | SortingLayerMask.FromName("UI");

// add/remove a layer by index
combined = combined.WithAddedLayer(3);
combined = combined.WithoutLayer(2);
```

Check a renderer's layer

```csharp
int sortingId = spriteRenderer.sortingLayerID;
if (combined.IncludesLayerID(sortingId)) {
    Debug.Log("Renderer is in the mask");
}
```

Iterate contained layer names

```csharp
foreach (var name in combined.EnumerateLayerNames()) {
    Debug.Log("Mask contains: " + name);
}
```

Performance tip for hot paths

```csharp
// avoid repeated string/index lookups in tight loops:
// use raw int and bit operations instead of high-level helpers
int rawMask = (int)combined;
if ((rawMask & (1 << index)) != 0) {
    // included
}
```

#### Inspector and serialization

* The struct is `[Serializable]` and stores `mask` with `[SerializeField]`, so it can be exposed on MonoBehaviours and ScriptableObjects.
* `FromName` relies on `SortingLayer.layers` lookup, so if layers change in the project the mask semantics can change; prefer storing masks as persistent fields in assets or validating in `OnValidate`.

#### Notes and gotchas

* Layer names map to indices at runtime; `FromName` returns an empty mask when the name is not found. Validate names or provide fallbacks in editor code.
* `SortingLayer.layers` lookup is linear in number of layers. Not a problem normally, but avoid re-running name-based lookups every frame. Cache masks or raw ints when needed.
* The struct is value-type friendly. Use `==`/`!=` and the provided operators for expressive, zero-allocation code.