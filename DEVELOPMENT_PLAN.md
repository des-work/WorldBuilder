# WorldBuilder AI — Development Plan

## Overview

This document outlines the project’s phased roadmap and the concrete tasks we will execute next. Phases build on the current Clean Architecture foundation and aim to integrate configuration, improve resilience/performance, and introduce advanced AI and UX features.

## Completed (Highlights)

- Clean Architecture across Core/Application/Infrastructure/Presentation
- EF Core + SQLite with migrations and seeding
- WPF shell with theme system and bootscreen; startup service and metrics
- Baseline AI integration via local Ollama service (model list + streaming)
- Unified DI for the WPF app; Docker (Windows) build/test/publish; GitHub Actions CI
- Test infrastructure with scenarios, data builders, performance placeholders

## Phase 1 — Integration & Optimization (Current)

Goals
- Unify configuration across layers and remove ad‑hoc wiring
- Improve startup and DI coherence in the Presentation layer
- Prepare the ground for advanced AI components by fixing seams

Scope & Tasks
- Configuration management
  - Add `appsettings.json` in WPF project and ensure it’s copied to output
  - Bind `WorldBuilderConfiguration` and start consuming values via options where appropriate
  - Use configured values for database and AI (timeouts, base URLs, flags)
- DI and startup coherence
  - Inject `IThemeService` into `MainWindowV3` instead of constructing it directly
  - Ensure startup resolves the DI-managed `MainWindowV3` and services
- AI client consistency
  - Refactor `OllamaAiService` to accept `HttpClient` from `IHttpClientFactory`
  - Register a typed client with base address/timeout from configuration
- Testing & stability
  - Keep tests green; ensure infra’s DI extension supports typed client or mocks
  - Add (or plan) a minimal integration test to validate config sourcing for AI base URL

Expected Outcomes
- Single source of truth for configuration
- Cleaner startup with fewer hidden singletons
- AI calls configured and resilient to environment differences

## Phase 2 — Advanced Features (Next)

Goals
- Introduce advanced AI services and non-linear story UX

Scope & Tasks
- AI Integration Enhancements
  - Implement `ISmartQuerySystem` (intent detection + routing)
  - Implement `IDynamicPromptGenerator` (context-aware prompts)
  - Implement `IStoryContextDatabase` and `IContextAwareAIService`
  - Implement `IResponseCacheSystem`, `IModelPerformanceProfiler`, and `IModelSelectionOptimizer`
- UI/UX Enhancements
  - Wire Non-Linear Story creation
  - Theme transitions polish and saved preferences
- Testing & Telemetry
  - Add targeted unit/integration tests for each AI component

## Phase 3 — User Experience

Goals
- Improve productivity and navigation; add timeline views and I/O

Scope & Tasks
- Advanced UI Features
  - Drag-and-drop, keyboard shortcuts, inline editing, global search/filter
- Timeline Visualization
  - Chronological events and character arcs; link to chapters/locations
- Import/Export
  - JSON/YAML export; wiki/game engine compatibility; backup/restore

## Phase 4 — Advanced AI

Goals
- Memory, consistency checking, and analytics

Scope & Tasks
- Conversation memory; character personality learning
- World consistency checks with suggestions
- Writing analytics and insights dashboards

---

## Phase 1 — Implementation Changelog (running)

We will maintain a brief running list of Phase 1 changes below as they land.

- Added `appsettings.json` to WPF with AI/DB defaults; ensured copy to output
- Injected `IThemeService` into `MainWindowV3` via DI (removed direct construction)
- Refactored `OllamaAiService` to use `HttpClient` from `IHttpClientFactory` and bound to configuration
- Updated DI registrations for typed AI client in both app and tests infra

