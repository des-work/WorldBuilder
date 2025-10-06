# 🌌 WorldBuilder AI

<div align="center">
  <br/>
  <h1>🧙‍♂️ Your Intelligent Partner for Building Living, Breathing Worlds</h1>
  <p><strong>Transform your creative vision into immersive universes with AI-powered world-building</strong></p>
  <br/>
</div>

<div align="center">
  <img src="https://img.shields.io/badge/build-passing-brightgreen" alt="Build Status">
  <img src="https://img.shields.io/badge/.NET-8.0-blue" alt=".NET Version">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
  <img src="https://img.shields.io/badge/database-SQLite-yellow" alt="Database">
  <img src="https://img.shields.io/badge/ai-Ollama-orange" alt="AI Integration">
</div>

---

## ✨ What is WorldBuilder AI?

**WorldBuilder AI** is a revolutionary desktop application that transforms world-building from a solitary note-taking exercise into an **interactive, AI-powered creative partnership**. Whether you're a novelist, game developer, dungeon master, or any creator managing complex fictional universes, WorldBuilder AI helps you:

- **📚 Organize Everything:** Hierarchical structure from universes → stories → chapters → characters
- **🤖 AI Co-Creation:** Talk to your characters, get context-aware suggestions, and explore your world
- **🔒 Complete Privacy:** 100% local AI processing via Ollama - your data never leaves your machine
- **⚡ Modern UX:** Real-time saving, validation, and intuitive editing interfaces

### 🎯 Built for Creators Who Want More

**From Static Notes to Living Worlds:** Don't just document your characters—**converse with them**. The AI embodies your character's voice, drawing from their biography, world lore, and story events they've experienced.

**Your Private AI Studio:** All processing happens locally on your machine. Use any Ollama model you've downloaded, ensuring complete creative ownership and privacy.

---

## 🚀 Key Features

### 📖 **Hierarchical Organization**
```
🌌 Universe
  📚 Stories
    📝 Chapters
  👥 Characters
    🏷️ Character Details
```

### 🤖 **AI-Powered Insights**
- **Character Conversations:** "What would Elara do in this situation?"
- **World Lore Queries:** "How does magic work in Aethelgard?"
- **Context-Aware Prompts:** AI understands your universe's rules and history

### 💾 **Smart Data Management**
- **Auto-Save:** Never lose your creative work
- **Validation:** Real-time feedback on data quality
- **Rich Relationships:** Track character appearances across chapters
- **Sample Worlds:** Pre-loaded with "The Aethelgard Chronicles" for testing

### 🎨 **Modern Interface**
- **TreeView Navigation:** Explore your universe hierarchically
- **Tabbed Editing:** Edit universes, stories, characters, and chapters
- **Real-Time Feedback:** "Saved!" notifications and dirty state tracking
- **Clean Design:** Dark theme optimized for long creative sessions

---

## 🛠️ Technology Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Backend** | .NET 8 & C# 12 | High-performance, modern framework |
| **UI Framework** | Windows Presentation Foundation | Rich, responsive desktop interface |
| **Architecture** | MVVM Pattern | Clean separation of concerns |
| **Database** | Entity Framework Core 8 + SQLite | Local, file-based storage |
| **AI Integration** | Ollama API | Local, private AI processing |
| **Logging** | Serilog | Comprehensive application logging |
| **Testing** | xUnit + Integration Tests | Reliable, tested codebase |

---

## 📋 Current Status: **Production Ready** ✅

### ✅ **Fully Implemented**
- **Complete CRUD Operations:** Create, read, update, delete for all entities
- **Rich Data Models:** Universes, Stories, Characters, Chapters with relationships
- **AI Integration:** Context-aware prompts and character conversations
- **Database Layer:** Entity Framework with migrations and seeding
- **Comprehensive Testing:** 8 passing integration tests covering all workflows
- **Sample Content:** Pre-loaded "Aethelgard Chronicles" fantasy world

### 🎯 **Ready for Users**
- **Intuitive Interface:** Professional WPF application
- **Data Persistence:** SQLite database with automatic migrations
- **Error Handling:** Comprehensive validation and user feedback
- **Performance:** Optimized for large worlds and complex relationships

---

## 🚀 Quick Start

### Prerequisites
- **Windows 10/11** (WPF application)
- **.NET 8 SDK** (automatically configured)
- **Ollama** (optional, for AI features)

### Installation & Launch

1. **Clone & Build:**
   ```bash
   git clone <your-repo>
   cd WorldBuilder
   dotnet build
   ```

2. **Launch the Application:**
   ```bash
   dotnet run --project Genisis.Presentation.Wpf
   ```

3. **Start Creating:**
   - The app auto-creates a SQLite database
   - Sample "Aethelgard Chronicles" world is pre-loaded
   - Begin editing or create your own universe!

### 🎮 Using the Application

1. **Explore Sample Data:** Click through "The Aethelgard Chronicles"
2. **Edit Content:** Select any item in the tree to edit its details
3. **Add New Worlds:** Use the "+ New Universe" button
4. **AI Conversations:** Select a character or story and ask questions in the AI panel
5. **Manage Relationships:** Link characters to chapters for tracking

---

## 📚 Sample World: "The Aethelgard Chronicles"

Your installation includes a complete fantasy world featuring:

### 🌍 **Universe:** Aethelgard Chronicles
> "A universe of high fantasy, ancient magic, and forgotten empires where dragons soar and wizards weave spells that can reshape reality itself."

### 👥 **Main Characters:**
- **Elara Stormweaver** (Main Hero) - Young wizard with storm magic
- **Archmage Eldric** (Mentor) - Ancient wizard guardian of forbidden knowledge
- **Lord Vesper Darkmoor** (Antagonist) - Fallen noble seeking immortality through dark magic

### 📖 **Story:** The Shadow of the Sunstone
> "When an ancient artifact awakens, a young wizard must master her powers to prevent a catastrophe that could engulf her world in eternal darkness."

### 📝 **Chapters:**
1. **The Storm's Awakening** - Elara discovers her powers
2. **The Ancient Mentor** - Archmage Eldric appears
3. **Shadows in the Storm** - Lord Vesper's dark influence grows

---

## 🔧 Development

### Project Structure
```
WorldBuilder/
├── Genisis.Core/          # Domain models and business logic
├── Genisis.Infrastructure/ # Data access and external services
├── Genisis.Application/   # Application services and handlers
├── Genisis.Presentation.Wpf/ # WPF UI and view models
└── Genisis.Tests/         # Comprehensive test suite
```

### Key Architecture Decisions
- **Clean Architecture:** Separation of concerns with dependency injection
- **MVVM Pattern:** Testable UI with data binding
- **Repository Pattern:** Abstracted data access
- **Entity Framework:** Code-first migrations with SQLite
- **Local AI:** Ollama integration for privacy

### Running Tests
```bash
dotnet test                    # Run all tests
dotnet test --filter IntegrationTests  # Run integration tests only
```

---

## 🗺️ Roadmap & Future Features

### 🎯 **Immediate Priorities**
- **Enhanced UI Polish:** Inline renaming, drag-and-drop, keyboard shortcuts
- **Character-Chapter Management:** Visual interface for linking characters to chapters
- **Timeline Visualization:** Chronological view of story events and character arcs

### 🚀 **Advanced Features**
- **World Timeline:** Visual timeline connecting all stories and character events
- **Advanced AI Features:** Conversation memory, character personality learning
- **Import/Export:** JSON/YAML export for wikis and game engines
- **Collaboration:** Shared universe editing (future)

### 🎨 **UI Enhancements**
- **Themes:** Light/dark mode toggle
- **Accessibility:** Screen reader support, keyboard navigation
- **Performance:** Virtual scrolling for large worlds

---

## 🤝 Contributing

We welcome contributions! Here's how to get involved:

### 🐛 **Bug Reports & Feature Requests**
- Use GitHub Issues with appropriate labels
- Include steps to reproduce bugs
- Describe feature requests with use cases

### 🔧 **Development Setup**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Add tests for new functionality
5. Run the full test suite: `dotnet test`
6. Commit changes: `git commit -m 'Add amazing feature'`
7. Push to branch: `git push origin feature/amazing-feature`
8. Open a Pull Request

### 📝 **Code Style Guidelines**
- Follow C# coding standards and naming conventions
- Add XML documentation for public APIs
- Include unit tests for new functionality
- Update README for significant changes

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Ollama Team** for enabling private, local AI processing
- **Entity Framework Team** for excellent ORM capabilities
- **WPF Community** for rich desktop application framework
- **Open Source Contributors** who make amazing tools possible

---

<div align="center">
  <p><strong>Ready to build worlds? 🌟</strong></p>
  <p>Launch the application and start creating your next great story!</p>
</div>


