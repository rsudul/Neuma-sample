# Neuma - code sample

This repository contains selected core systems and infrastructure code from **Neuma**, a commercial detective game I am currently developing.

**Note:** This is a partial repository intended for code review and recruitment purposes. The full game source, assets, and narrative content are in a private repository.

## Tech Stack
* **Engine:** Godot 4.5 (C# / .NET 8.0)
* **Architecture:** Clean Architecture principles, Heavy use of Dependency Injection
* **Tools:** Microsoft.Extensions.DependencyInjection

## Key Systems to Explore

### 1. Architecture & DI
The project avoids tight coupling with the engine by using a strict DI container setup.
* See `Infrastructure/Bootstrap.cs`: Entry point where services are registered using `Microsoft.Extensions.DependencyInjection`.
* See `Infrastructure/GameServices.cs`: Service locator pattern wrapper for Godot nodes.

### 2. Core Logic (Engine-Agnostic)
Most game logic resides in the `Core` namespace and has no dependency on `Godot.*` classes, making it testable.
* `Core/Cases`: Manages investigation progress and logic purely in C#.
* `Core/Logging`: A custom structured logging system with multiple sinks (File, Console).

### 3. Input Handling
A centralized input router that handles context switching (Gameplay vs Menu) to prevent input bleeding.
* `Core/Input/InputEventRouter.cs`: Orchestrates input processing.

## Copyright
Copyright (c) 2025 Radosław Suduł. All rights reserved.
Code provided for evaluation purposes only.
