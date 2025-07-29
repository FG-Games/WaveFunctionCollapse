Wave Function Collapse

This repository contains a flexible and modular Wave Function Collapse (WFC) algorithm template designed to automatically assemble tileable modules according to preset adjacency rules. 
The setup is agnostic to the tessellation type, supporting various 2D and 3D grids (square, hexagonal, cubic, etc.).
It is intended as a subrepository within your Unity 3D project to provide procedural content generation based on constraint solving.

The project consists primarily of a bottom-level framework built on abstract classes and interfaces.
No example projects are provided at this stage; the focus is on a clean, extensible foundation for your own implementations.
Modules represent congruent tessellation elements, each defined by user-provided feature IDs:
Users specify a series of digits describing module characteristics (e.g., corners).
This ID-based system does not analyze geometry or auto-generate feature data.

Key Points
- Data-oriented design: Extensive use of structs for performance; groundwork laid for future optimization with Unity Native Arrays and multithreading.
- Constraint logic: Boolean operations on CellConstraints and Superpositions allow flexible yet controlled tile adjacency.
- Rotation-aware: Supports multiple orientations per Module.
- Debuggable: Core collapse process is cleanly encapsulated for ease of debugging and extension.

Current Limitations
- The feature ID system requires manual input of module characteristics, which may limit tile diversity and flexibility.
- Performance optimizations via multithreading and native arrays are planned but not yet implemented.
- Implementation or example scenes are not included.

Usage
- Add this repository as a Git submodule in your Unity project.
- Inherit from the base abstract classes to define your own Module, ModuleSet, and CSPFieldCollapse implementations.
- Your CSPFieldCollapse subclass must provide a CSP field that implements ICSPField<A>, where A represents your address type (e.g., grid coordinates).
- This abstraction layer is the cost of supporting arbitrary tessellation types — from square grids to 3D lattices.

Contributing
- Contributions, bug reports, and feature requests are welcome!
