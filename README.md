
## Unity Isometric 2D System (with Job System Support)

<img src="https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Overview.png" width="60%" />

A 2.5D isometric sorting plugin for Unity.

### System Requirements

- **Unity**: 2021.3 or newer

### Quick Start

![Git Guide](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/GitGuide.png)

Open Unity’s Package Manager, click the **+** button, and select `Add package from git URL...`.  
Paste the following URL:

```
https://github.com/MyNameIsDabin/UnityIsometric2D.git?path=/Assets/Isometric2D
```

You can also clone the entire repository and include only the scripts you need.

Check out the included sample project and open the `Sample.unity` scene for a working example.

---

## Features

### Visual Debugging & Job System Support

<img src="https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature1.png" width="60%" />

- Real-time visual debugging of sorting in the Unity Editor.
- High-performance sorting powered by Unity’s Job System.
- Option to switch to a single-threaded mode for compatibility with older Unity versions.

---

### Sorting for `SortingGroup` and `SpriteRenderer`

<img src="https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature2.png" width="60%" />

- Attach `IsometricSortingGroup` or `IsometricSpriteRenderer` to your target `SpriteRenderer` or `SortingGroup`.
- These components depend on `IsometricObject`, which stores the calculated sorting order.
- You can create custom sorting behavior by inheriting from the `IsometricOrderBinder` class.
- To avoid performance issues when many objects are present, culling options are provided to include only visible objects in sorting.

---

### Automatic Handling of `lossyScale` in Isometric Area

![Feature 3](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature3.gif)

- Automatically adjusts the isometric collision bounds based on `lossyScale`.
- No need to manually update the `IsometricObject` size when flipping or scaling objects, even when parent objects are transformed.

---

## License

[CC0 1.0 Universal (Public Domain Dedication)](https://creativecommons.org/publicdomain/zero/1.0/)