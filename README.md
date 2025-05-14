
# EasySave - Backup Management Solution

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](https://github.com/Meckagameers85/EasySave/releases)

A powerful and intuitive backup management application built with C# and .NET 8.0. EasySave provides a modern, interactive console interface for creating, managing, and executing backup tasks with real-time progress tracking.

![EasySave Console Interface](https://example.com/screenshots/easysave-console.png)

## 🌟 Key Features

- 🔄 **Multiple Backup Types** - Perform full or differential backups based on your needs
- 🌐 **Multilingual Support** - Available in 7 languages (English, French, Spanish, German, Italian, Portuguese, Russian)
- 📋 **Complete Task Management** - Create, view, edit, and delete backup tasks with ease
- 📊 **Real-time Monitoring** - Track backup progress with detailed state information
- 💾 **Persistent Storage** - All tasks and settings are preserved between sessions
- 🎨 **Rich Console Interface** - Enjoy a modern, colorful, and interactive user experience

## 📋 Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Backup Types](#backup-types)
- [State and Logs](#state-and-logs)
- [Configuration](#configuration)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)
- [Team](#team)

## 🔧 Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later
- Any terminal that supports ANSI color codes
- [Spectre.Console](https://spectreconsole.net/) (automatically installed via NuGet)

### Installation Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/Meckagameers85/EasySave.git
   cd EasySave
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run 
   ```

## 🚀 Usage

### Creating a Backup Task

1. From the main menu, select "Create new backup task"
2. Enter a unique name for your task
3. Specify the source directory (must exist)
4. Specify the target directory (will be created if it doesn't exist)
5. Choose the backup type (Full or Differential)

```
===== EasySave v1.0 =====

[?] What would you like to do?: 
   > Create new backup task
     Show all backup tasks
     Execute backup tasks
     Edit backup task
     Delete backup task
     Settings
     Quit
```

### Running Backups

1. From the main menu, select "Execute backup tasks"
2. Enter the task numbers to execute using one of these formats:
   - Individual tasks: `1` or `3`
   - Range of tasks: `1-3` (executes tasks 1, 2, and 3)
   - Specific tasks: `1;3` (executes only tasks 1 and 3)

```
┌─────────────────────────────────────────┐
│ Available Backup Tasks                  │
├────┬──────────┬──────────┬─────────────┤
│ ID │ Name     │ Type     │ Source      │
├────┼──────────┼──────────┼─────────────┤
│ 1  │ Documents│ Full     │ C:/Documents│
│ 2  │ Pictures │ Full     │ C:/Pictures │
│ 3  │ Projects │ Differ.  │ C:/Dev      │
└────┴──────────┴──────────┴─────────────┘

Enter task number(s) to execute (e.g. 1-3 or 1;3): 
```

## 📂 Project Structure

EasySave follows the MVVM (Model-View-ViewModel) architecture pattern for clear separation of concerns:

```
EasySave/
├── DOC/
|   ├── ClassDiagram.wsd
|   └── UseCaseDiagram.wsd
├── EasySaveProject/
|   ├── EasySaveConsole.csproj
|   ├── Languages/
|   |   ├── en.json
|   |   └── fr.json
|   ├── Models/
|   |   ├── ActionItem.cs
|   |   ├── BackupManager.cs
|   |   ├── LocalizationManager.cs
|   |   ├── SaveState.cs
|   |   ├── SaveTask.cs
|   |   └── SettingsManager.cs
|   ├── Program.cs
|   ├── ViewModels/
|   |   └── MenuViewModel.cs
|   └── Views/
|   |   └── MainMenuView.cs
├── LoggerLib/
|   ├── LogEntry.cs
|   ├── LoggerLib.cs
|   └── LoggerLib.csproj
├── .gitignore
└── README.md
```

## 💾 Backup Types

EasySave supports two types of backups:

### Full Backup

A complete backup that copies all files from the source to the destination, regardless of their modification date or existing state. This ensures you have a complete copy of your data but uses more storage space and time.

### Differential Backup

A smart backup that copies only files that:
- Don't exist in the destination directory
- Have been modified since the last backup (based on last modification date)

This type of backup is faster and uses less storage space, making it ideal for frequent backups of large directories.

## 📊 State and Logs

EasySave generates two types of files to track backup operations:

### State File

A JSON file (`state.json`) containing real-time information about currently running backups:

```json
[
  {
    "Name": "Documents",
    "SourceFilePath": "C:/Users/username/Documents/report.docx",
    "TargetFilePath": "D:/Backups/Documents/report.docx",
    "State": "ACTIVE",
    "TotalFilesToCopy": 143,
    "TotalFilesSize": 2564738,
    "NbFilesLeftToDo": 98,
    "Progression": 31
  }
]
```

### Log Files

Daily JSON log files (format: `Log_YYYY-MM-DD.json`) recording details of each file transferred:

```json
[
  {
    "timestamp": "2023-06-15T14:32:45.1234567+01:00",
    "task_name": "Documents",
    "source_path": "C:/Users/username/Documents/report.docx",
    "destination_path": "D:/Backups/Documents/report.docx",
    "file_size": 1540096,
    "transfer_time": 212
  }
]
```

## ⚙️ Configuration

Application settings are stored in `settings.json`:

```json
{
  "CurrentLanguage": "en"
}
```

Available settings:
- `CurrentLanguage`: Language code for the UI (en, fr, es, de, it, pt, ru)

## 🛠️ Development

### Build and Run

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project EasySave.ConsoleApp

# Run tests
dotnet test
```

### Design Patterns

EasySave implements several design patterns:

- **MVVM Pattern**: Separates UI (View) from business logic (ViewModel) and data (Model)
- **Command Pattern**: Used in menu action execution
- **Strategy Pattern**: For different backup types
- **Singleton Pattern**: For managers like settings and localization
- **Repository Pattern**: For backup task persistence

### Coding Conventions

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Include XML documentation for public members
- Write unit tests for all major functionality
- Use async/await for file operations where appropriate

## 👥 Contributing

We welcome contributions to EasySave! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add some amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

Please ensure your code follows our coding conventions and includes appropriate tests.

### Workflow

- `main` branch: Production-ready code
- `develop` branch: Integration branch for features
- `feature/*` branches: New features and improvements
- `bugfix/*` branches: Bug fixes

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Team

- **Etienne** - Developer
- **Jade** - Developer
- **Mahery** - Developer
- **Thomas V** - Developer
- **Thomas Q** - Developer

---

📧 **Questions or issues?** Open an issue on our [GitHub repository](https://github.com/Meckagameers85/EasySave.git/issues).
```