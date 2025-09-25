# 🌌 WorldBuilder (Genisis AI)

**Unleash your imagination and build sprawling fictional universes with the help of an AI-powered co-creator.**

WorldBuilder is a desktop application designed for writers, game developers, dungeon masters, and any creator who needs to manage complex fictional worlds. It provides a structured, hierarchical way to organize your ideas, from the grandest universe down to the individual chapters of a story, all while offering intelligent assistance to spark your creativity.

## ✨ Features

*   **Hierarchical Organization:** Structure your world logically with a tree-based view:
    *   **Universes:** The top-level container for your entire world.
    *   **Stories:** Individual narratives within your universe.
    *   **Chapters:** The building blocks of your stories.
    *   **Characters:** The people who inhabit your world.
*   **Rich Content Editing:** Flesh out your creations with dedicated editors for descriptions, loglines, character bios, and chapter content.
*   **AI-Powered Companion:** The "Ask Your Universe" feature (in development) will allow you to query your own world's lore, generate ideas, and overcome writer's block.
*   **Local-First Data:** Your work is saved locally in a private SQLite database. You own your data.
*   **Modern & Extensible:** Built with modern .NET, WPF, and a clean MVVM architecture, making it easy to understand and extend.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or newer)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the ".NET desktop development" workload installed.

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/WorldBuilder.git
    ```
2.  **Open the solution:** Navigate to the cloned directory and open `Genisis.sln` in Visual Studio.
3.  **Run the application:** Press `F5` or click the "Start" button to build and run the project.

The application will automatically create a local SQLite database file (`genisis.db`) and log files in `%APPDATA%\GenisisAI\`.

## 🛠️ Technology Stack

*   **Framework:** .NET 8 & C# 12
*   **UI:** Windows Presentation Foundation (WPF)
*   **Architecture:** Model-View-ViewModel (MVVM)
*   **Database:** Entity Framework Core 8 with SQLite
*   **Logging:** Serilog

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

Please feel free to fork the repo, create a feature branch, and submit a pull request. You can also open an issue with the "bug" or "enhancement" tag.

## License

This project is open source and available under the [MIT License](LICENSE).
