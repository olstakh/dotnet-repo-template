# .NET Repository Template

A comprehensive .NET repository template with best practices, custom Roslyn analyzers, and recommended defaults.

## Features

✅ **Central Package Management (CPM)** - Manage all package versions in one place<br>
✅ **Transitive Pinning** - Pin transitive dependencies for reproducible builds<br>
✅ **Custom Roslyn Analyzers** - Template includes analyzer project structure and example<br>
✅ **Nullables Enabled** - Nullable reference types enabled by default<br>
✅ **Latest C# Language Version** - Always use the latest language features<br>
✅ **Global.json** - Pin SDK version for consistent builds<br>
✅ **Nerdbank.GitVersioning** - Automatic semantic versioning from git history<br>
✅ **SourceLink** - Source debugging support (auto-enabled in git repos)<br>
✅ **EditorConfig** - Consistent code style across editors<br>

## Installation

```bash
dotnet new install DotnetRepo-olstakh
```

## Usage

Create a new repository from the template:

```bash
dotnet new repo-template -n MyAwesomeProject
cd MyAwesomeProject
dotnet build
```

### With a custom NuGet feed

```bash
dotnet new repo-template -n MyAwesomeProject --feed https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json
```

### With custom Roslyn analyzers

```bash
dotnet new repo-template -n MyAwesomeProject --include-analyzers
```

## Template Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-n \| --name` | string | `RenameMe` | The name for the generated project, used to rename namespaces, projects, and solution files. |
| `--include-analyzers` | bool | `false` | If specified, includes the CodeAnalysis folder with custom Roslyn analyzer and test projects. |
| `--feed` | string | `https://api.nuget.org/v3/index.json` | The NuGet package source feed URL to use in Nuget.config. |

This creates:
- `MyAwesomeProject.slnx` - Solution file
- `src/MyAwesomeProject/` - Main project
- `CodeAnalysis/MyAwesomeProject.CodeAnalysis/` - Roslyn analyzer project
- `CodeAnalysis/MyAwesomeProject.CodeAnalysis.Tests/` - Analyzer tests
- Configuration files (Directory.Build.props, Directory.Packages.props, global.json, etc.)

## Project Structure

```
MyAwesomeProject/
├── .editorconfig              # Code style settings
├── .gitignore                 # Git ignore rules
├── Directory.Build.props      # Shared MSBuild properties
├── Directory.Packages.props   # Central package versions
├── DependencyRules.json       # Dependency validation rules
├── global.json               # .NET SDK version
├── Nuget.config              # NuGet feed configuration
├── version.json              # Nerdbank.GitVersioning configuration
├── MyAwesomeProject.slnx     # Solution file
├── CodeAnalysis/
│   ├── MyAwesomeProject.CodeAnalysis/          # Custom analyzers
│   └── MyAwesomeProject.CodeAnalysis.Tests/    # Analyzer tests
└── src/
    └── MyAwesomeProject/     # Main project
```

## Building and Testing

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

## Git Workflow

The template includes a custom analyzer that warns if your project is still named "RenameMe". Make sure to use the template with `-n YourProjectName`!

```bash
# Initialize git
git init
git add .
git commit -m "Initial commit from template"

# Create version tag for release
git tag v1.0.0
git push origin main --tags
```

## Contributing

Issues and pull requests welcome!

## License

MIT
