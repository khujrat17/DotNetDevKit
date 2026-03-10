# DotNetDevKit - Setup & Build Guide

Complete instructions to build and publish the DotNetDevKit NuGet package.

## Project Structure

```
DotNetDevKit/
├── DotNetDevKit.csproj                 # Project file
├── README.md                           # Main documentation
├── LICENSE                             # MIT License
├── EXAMPLES.md                         # Usage examples
├── PUBLISHING.md                       # NuGet publishing guide
├── SETUP.md                            # This file
├── .gitignore                          # Git ignore rules
│
├── DotNetDevKitExtensions.cs           # Main extension methods
│
├── ApiResponse/                        # API Response handling
│   ├── ApiResponseModels.cs            # Response models
│   └── ApiResponseExtensions.cs        # Response extensions & middleware
│
├── DependencyInjection/                # Dependency injection
│   └── AutoServiceRegistrar.cs         # Auto-registration attribute & scanner
│
└── Dashboard/                          # Developer debug dashboard
    └── DevDebugDashboardController.cs  # Dashboard controller & models
```

## Prerequisites

### System Requirements

- **OS**: Windows, macOS, or Linux
- **Memory**: 4GB RAM minimum
- **.NET SDK**: 6.0 or higher
- **Git**: For version control
- **Editor**: Visual Studio, VS Code, or JetBrains Rider

### Install .NET SDK

```bash
# Check if .NET is installed
dotnet --version

# If not installed, download from:
# https://dotnet.microsoft.com/download
```

## Setup Instructions

### 1. Clone or Extract the Project

```bash
# If from git
git clone https://github.com/yourusername/DotNetDevKit.git
cd DotNetDevKit

# Or extract the zip file
unzip DotNetDevKit.zip
cd DotNetDevKit
```

### 2. Restore Dependencies

```bash
# Restore NuGet packages
dotnet restore
```

Expected output:
```
Determining projects to restore...
Restored C:\path\to\DotNetDevKit\DotNetDevKit.csproj (in 5 sec).
```

### 3. Build the Project

```bash
# Debug build
dotnet build

# Release build (for production)
dotnet build -c Release
```

Expected output:
```
Build succeeded.
    Time elapsed: 00:00:15.42
```

### 4. Run Tests (Optional)

```bash
# If you create unit tests
dotnet test
```

## Creating the NuGet Package

### Build Release Package

```bash
# Navigate to project directory
cd DotNetDevKit

# Create NuGet package
dotnet pack -c Release
```

Output:
```
DotNetDevKit.1.0.0.nupkg -> C:\path\to\DotNetDevKit\bin\Release\DotNetDevKit.1.0.0.nupkg
DotNetDevKit.1.0.0.snupkg -> C:\path\to\DotNetDevKit\bin\Release\DotNetDevKit.1.0.0.snupkg
```

### Verify Package Contents

```bash
# List package contents
unzip -l bin/Release/DotNetDevKit.1.0.0.nupkg

# Or on Windows PowerShell
Expand-Archive -Path bin\Release\DotNetDevKit.1.0.0.nupkg -DestinationPath temp-extract
ls temp-extract
```

Expected structure:
```
DotNetDevKit.1.0.0.nupkg
├── [Content_Types].xml
├── .nuspec
├── lib/
│   └── net6.0/
│       ├── DotNetDevKit.dll
│       ├── DotNetDevKit.pdb
│       └── DotNetDevKit.xml
└── package/
    └── services/
        └── metadata/
```

## Development Workflow

### Visual Studio

1. Open `DotNetDevKit.csproj` in Visual Studio
2. Build: Ctrl+Shift+B
3. To create package: Right-click project → Pack
4. Output in `bin/Release/`

### Visual Studio Code

```bash
# Open project in VS Code
code .

# Build with VS Code terminal
dotnet build

# Pack
dotnet pack -c Release
```

### Command Line (Cross-platform)

```bash
# Build
dotnet build -c Release

# Pack
dotnet pack -c Release

# Test locally
dotnet add package DotNetDevKit --source ./bin/Release
```

## Publishing to NuGet

### Step 1: Create NuGet Account

1. Go to https://www.nuget.org
2. Click "Register"
3. Fill in details and create account
4. Verify email

### Step 2: Generate API Key

1. Log in to https://www.nuget.org
2. Click username (top-right) → "API Keys"
3. Click "Create"
4. Configure:
   - Name: "DotNetDevKit"
   - Scope: "Push new packages and package versions"
5. Copy API key (shown only once!)

### Step 3: Push to NuGet

#### Using dotnet CLI (Recommended)

```bash
# Push package
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  --api-key YOUR_API_KEY_HERE \
  --source https://api.nuget.org/v3/index.json

# Push symbol package (optional)
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.snupkg \
  --api-key YOUR_API_KEY_HERE \
  --source https://api.nuget.org/v3/index.json
```

#### Using NuGet CLI

```bash
# Install if needed
dotnet tool install --global NuGet.CommandLine

# Push
nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  -ApiKey YOUR_API_KEY_HERE \
  -Source https://api.nuget.org/v3/index.json
```

### Step 4: Verify Publication

```bash
# Wait 5-10 minutes for NuGet indexing

# Check online
# Visit: https://www.nuget.org/packages/DotNetDevKit

# Or verify via CLI
nuget search DotNetDevKit
```

## Version Updates

### Update for New Release

1. **Update version** in `DotNetDevKit.csproj`:
   ```xml
   <Version>1.1.0</Version>
   <PackageVersion>1.1.0</PackageVersion>
   ```

2. **Update release notes**:
   ```xml
   <PackageReleaseNotes>
   v1.1.0:
   - New feature X
   - Bug fix Y
   - Improvement Z
   </PackageReleaseNotes>
   ```

3. **Rebuild and pack**:
   ```bash
   dotnet clean
   dotnet build -c Release
   dotnet pack -c Release
   ```

4. **Push new version**:
   ```bash
   dotnet nuget push bin/Release/DotNetDevKit.1.1.0.nupkg \
     --api-key YOUR_API_KEY \
     --source https://api.nuget.org/v3/index.json
   ```

## Troubleshooting

### Issue: "dotnet command not found"

**Solution:**
- Install .NET SDK from https://dotnet.microsoft.com/download
- Restart terminal/IDE
- Verify: `dotnet --version`

### Issue: "Package restore failed"

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

### Issue: "Build errors with dependencies"

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet build -c Release

# If still failing, check .csproj dependencies:
# - Microsoft.AspNetCore.App >= 6.0.0
# - Microsoft.Extensions.DependencyInjection >= 6.0.0
```

### Issue: "Package validation failed on NuGet"

**Solution:**
- Verify .csproj is complete
- Check that DLL is in `lib/net6.0/`
- Ensure XML documentation file exists
- Check package metadata (Title, Author, etc.)

### Issue: "API key is invalid"

**Solution:**
- Regenerate API key on https://www.nuget.org/account/ApiKeys
- Ensure scope is "Push new packages"
- Use full API key (no truncation)
- Copy-paste to avoid typos

## Build Checklist

Before publishing, verify:

- [ ] .NET SDK version 6.0+
- [ ] All files in place
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build -c Release` succeeds
- [ ] No build warnings
- [ ] `dotnet pack -c Release` creates .nupkg
- [ ] Package contents verified
- [ ] Version number updated
- [ ] Release notes updated
- [ ] NuGet account created
- [ ] API key generated
- [ ] API key not expired
- [ ] README is complete
- [ ] LICENSE file present
- [ ] All dependencies in .csproj

## CI/CD Setup (GitHub Actions)

Create `.github/workflows/publish.yml`:

```yaml
name: Build & Publish

on:
  push:
    tags:
      - 'v*'

jobs:
  build-publish:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release
    
    - name: Pack
      run: dotnet pack -c Release
    
    - name: Publish NuGet
      run: |
        dotnet nuget push "bin/Release/*.nupkg" \
          -k ${{ secrets.NUGET_API_KEY }} \
          -s https://api.nuget.org/v3/index.json \
          --skip-duplicate
```

### Usage:

```bash
# Tag a release
git tag v1.0.0
git push origin v1.0.0

# GitHub Actions automatically:
# 1. Builds the project
# 2. Creates NuGet package
# 3. Publishes to NuGet.org
```

## File Structure Summary

### Core Files

| File | Purpose |
|------|---------|
| `DotNetDevKit.csproj` | Project configuration |
| `DotNetDevKitExtensions.cs` | Main extension methods |

### API Response Module

| File | Purpose |
|------|---------|
| `ApiResponse/ApiResponseModels.cs` | Response classes |
| `ApiResponse/ApiResponseExtensions.cs` | Response helpers & middleware |

### DI Module

| File | Purpose |
|------|---------|
| `DependencyInjection/AutoServiceRegistrar.cs` | Auto-registration attribute & scanner |

### Dashboard Module

| File | Purpose |
|------|---------|
| `Dashboard/DevDebugDashboardController.cs` | Dashboard endpoints & models |

### Documentation

| File | Purpose |
|------|---------|
| `README.md` | Main documentation |
| `EXAMPLES.md` | Usage examples |
| `PUBLISHING.md` | Publishing guide |
| `SETUP.md` | This file |
| `LICENSE` | MIT License |

## Performance Notes

- **Build time**: ~5-10 seconds
- **Pack time**: ~2-3 seconds  
- **Publish time**: ~30-60 seconds (includes NuGet indexing)
- **Package size**: ~50-100 KB

## Next Steps

1. ✅ Clone/extract this project
2. ✅ Run `dotnet restore`
3. ✅ Run `dotnet build -c Release`
4. ✅ Run `dotnet pack -c Release`
5. ✅ Verify package in `bin/Release/`
6. ✅ Create NuGet account
7. ✅ Generate API key
8. ✅ Push to NuGet using `dotnet nuget push`
9. ✅ Verify on https://www.nuget.org/packages/DotNetDevKit

## Support

- 📖 [NuGet Docs](https://docs.microsoft.com/en-us/nuget/)
- 🔧 [.NET CLI Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- 💬 [NuGet Issues](https://github.com/NuGet/Home/issues)
- 🤝 [Stack Overflow](https://stackoverflow.com/questions/tagged/nuget)

---

**Ready to publish?** Follow the Publishing to NuGet section! 🚀
