# 🌌 WorldBuilder AI

<div align="center">
  <br/>
  <strong>Your intelligent partner for building living, breathing worlds.</strong>
  <br/>
</div>

<div align="center">
  <!-- Placeholder for badges -->
  <img src="https://img.shields.io/badge/build-passing-green" alt="Build Status">
  <img src="https://img.shields.io/badge/license-MIT-blue" alt="License">
  <img src="https://img.shields.io/badge/.NET-8.0-purple" alt=".NET Version">
</div>

WorldBuilder AI is a desktop application designed for writers, game developers, dungeon masters, and any creator who needs to manage complex fictional worlds. It provides a structured, hierarchical way to organize your ideas, from the grandest universe down to the individual chapters of a story, all while offering intelligent assistance to spark your creativity.

## What is WorldBuilder AI?

WorldBuilder AI transforms the daunting task of world-building from a static, note-taking exercise into a dynamic, interactive conversation. It's a desktop application for writers, game developers, and dungeon masters who want to go deeper than a wiki. WorldBuilder AI is your co-creator, helping you not only to document your world but to truly *understand* it.

At its heart, it combines a powerful organizational framework with a **100% local, private, and context-aware AI** that brings your universe to life.

<!-- Placeholder for a screenshot or GIF of the app in action -->
`[Image: A screenshot showing the TreeView, a character editor, and the AI chat pane.]`

## Why WorldBuilder AI?

*   **From World Bible to Living World:** Don't just write *about* your characters—**talk to them**. Genisis AI's core feature is its ability to let you have real, in-character conversations. The AI embodies your character's persona, drawing from their bio, their world's lore, and even events from chapters they've appeared in.

*   **Your Private AI Co-Creator:** All AI processing happens on your machine via Ollama. Your prompts, your world's data, and your creative sparks **never leave your computer**. You can use any model you've downloaded, ensuring complete privacy and ownership.

*   **A Structure That Sparks Creativity:** The hierarchical `TreeView` provides the perfect balance of organization and discovery. See the grand sweep of your universe, then drill down into the details of a single chapter or character with a click.

*   **Built for the Modern Creator:** A clean, intuitive interface with modern UX considerations like "dirty" state tracking and "Saved!" notifications means you spend less time fighting the tool and more time creating.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

*   **.NET 8 SDK** (or newer)
*   **Visual Studio 2022** with the ".NET desktop development" workload installed.
*   **Ollama:** To use the AI features, you must have Ollama installed and running on your machine with at least one model downloaded (e.g., `ollama run llama3`).

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/WorldBuilderAI.git
    ```
2.  **Open the solution:** Navigate to the cloned directory and open `Genisis.sln` in Visual Studio.
3.  **Run the application:** Press `F5` or click the "Start" button to build and run the project.

The application will automatically create a local SQLite database file (`worldbuilder.db`) and log files in `%APPDATA%\WorldBuilderAI\`.

## 🚀 Next Steps & Roadmap

*   **Universe Timeline:** A visual tool to navigate the connections and chronology of your stories and characters.
*   **Character-to-Chapter Linking UI:** A dedicated interface for easily managing which characters appear in which chapters.
*   **Inline Renaming:** The ability to rename items directly from the `TreeView`.

## 🛠️ Technology Stack

*   **Framework:** .NET 8 & C# 12
*   **UI:** Windows Presentation Foundation (WPF)
*   **Architecture:** Model-View-ViewModel (MVVM)
*   **Database:** Entity Framework Core 8 with SQLite
*   **Logging:** Serilog
*   **AI Integration:** Ollama

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

Please feel free to fork the repo, create a feature branch, and submit a pull request. You can also open an issue with the "bug" or "enhancement" tag.

## License

This project is open source and available under the MIT License.


