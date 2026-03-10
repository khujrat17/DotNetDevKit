# Publishing DotNetDevKit to NuGet

Complete guide to publish your DotNetDevKit package to NuGet.org

## Prerequisites

1. **.NET 6 SDK or higher** - Download from https://dotnet.microsoft.com/download
2. **NuGet Account** - Create free account at https://www.nuget.org/users/account/Register
3. **API Key** - Generate from https://www.nuget.org/account/ApiKeys

## Step 1: Set Up Your NuGet Account

1. Go to https://www.nuget.org
2. Click "Register" and create your account
3. Verify your email address
4. Log in to your account

## Step 2: Generate API Key

1. Click on your username (top-right)
2. Select "API Keys"
3. Click "Create"
4. Enter a key name (e.g., "DotNetDevKit")
5. Select scope: "Push new packages and package versions"
6. Copy the API key (you'll only see it once!)

## Step 3: Configure Your Project

### Update the .csproj file

Make sure your `DotNetDevKit.csproj` has:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Framework -->
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    
    <!-- Documentation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    
    <!-- NuGet Package Properties -->
    <PackageId>DotNetDevKit</PackageId>
    <Version>1.0.0</Version>
    <Title>DotNetDevKit</Title>
    <Authors>Your Name</Authors>
    <Description>Automatic dependency injection registration, standardized API responses, and developer debug dashboard for ASP.NET Core</Description>
    
    <!-- Repository Information -->
    <PackageProjectUrl>https://github.com/yourusername/DotNetDevKit</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourusername/DotNetDevKit</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    
    <!-- Licensing -->
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    
    <!-- Additional Metadata -->
    <PackageVersion>1.0.0</PackageVersion>
    <PackageReleaseNotes>Initial release with AutoServiceRegistrar, ApiResponseKit, and DevDebugDashboard</PackageReleaseNotes>
    <PackageTags>aspnetcore;dependency-injection;api-response;debug-dashboard;devtools</PackageTags>
    
    <!-- Symbols Package -->
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.App" Version="6.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="6.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="6.0.0" />
  </ItemGroup>

</Project>
```

## Step 4: Build the Package

### Command Line

```bash
# Navigate to your project directory
cd DotNetDevKit

# Build in Release mode
dotnet build -c Release

# Create NuGet package
dotnet pack -c Release
```

This creates a `.nupkg` file in `bin/Release/`

### Expected Output

```
bin/Release/
├── DotNetDevKit.1.0.0.nupkg
└── DotNetDevKit.1.0.0.snupkg
```

## Step 5: Verify Package Contents

Before publishing, verify your package:

```bash
# List contents of the package
nuget list -Source bin/Release

# Or unzip and inspect
unzip bin/Release/DotNetDevKit.1.0.0.nupkg -d temp-extract
ls temp-extract
```

Expected structure:
```
DotNetDevKit.1.0.0.nupkg
├── lib/
│   └── net6.0/
│       ├── DotNetDevKit.dll
│       └── DotNetDevKit.xml (documentation)
├── .nuspec
└── [Content_Types].xml
```

## Step 6: Publish to NuGet.org

### Option A: Using NuGet CLI

```bash
# Install NuGet CLI if needed
dotnet tool install --global NuGet.CommandLine

# Push package
nuget push bin/Release/DotNetDevKit.1.0.0.nupkg -ApiKey YOUR_API_KEY -Source https://api.nuget.org/v3/index.json

# Push symbol package (optional but recommended)
nuget push bin/Release/DotNetDevKit.1.0.0.snupkg -ApiKey YOUR_API_KEY -Source https://api.nuget.org/v3/index.json
```

### Option B: Using dotnet CLI

```bash
# Push package
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

# Push symbol package
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.snupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Option C: Using Visual Studio

1. Right-click on project → "Pack"
2. Go to `bin/Release/`
3. Right-click `.nupkg` → "Push"
4. Enter API key and upload

## Step 7: Verify Publication

### Check Online

1. Go to https://www.nuget.org/packages/DotNetDevKit
2. Wait 5-10 minutes for indexing
3. Verify all details are correct
4. Check documentation appears correctly

### Test Installation

```bash
# Create test project
dotnet new console -n TestDotNetDevKit
cd TestDotNetDevKit

# Install package
dotnet add package DotNetDevKit --version 1.0.0

# Verify installation
dotnet list package

# Test in code
using DotNetDevKit;
var builder = WebApplicationBuilder.CreateBuilder(args);
builder.Services.AddDotNetDevKit();
```

## Step 8: Semantic Versioning

For future releases, follow Semantic Versioning:

```
Major.Minor.Patch-PreRelease+Build
1.0.0-alpha.1
1.0.0-beta
1.0.0-rc.1
1.0.0
1.1.0 (minor feature)
2.0.0 (breaking changes)
```

### Update version in .csproj:

```xml
<Version>1.1.0</Version>
<PackageVersion>1.1.0</PackageVersion>
```

### Update package release notes:

```xml
<PackageReleaseNotes>
Version 1.1.0:
- Added support for .NET 7.0
- Improved performance in AutoServiceRegistrar
- Added PaginatedApiResponse support
- Fixed bug in exception middleware
</PackageReleaseNotes>
```

## Troubleshooting

### Issue: "The API key is not valid"

**Solution:**
- Verify API key is correct (copy-paste from NuGet website)
- Ensure API key has "Push new packages" permission
- Try regenerating a new API key

### Issue: "Package with ID already exists"

**Solution:**
- The package name is already taken on NuGet.org
- Choose a different package name
- Or contact NuGet support to request transfer

### Issue: "Package contents invalid"

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet build -c Release

# Verify .csproj has correct properties
# Check that assemblies are in lib/net6.0/ folder
```

### Issue: "Symbol package failed"

**Solution:**
```bash
# Skip symbol package if issues persist
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
# Don't push .snupkg file
```

## Continuous Integration / CD

### GitHub Actions Example

Create `.github/workflows/publish.yml`:

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '6.0.x'
      
      - name: Build
        run: dotnet build -c Release
      
      - name: Pack
        run: dotnet pack -c Release
      
      - name: Publish to NuGet
        run: |
          dotnet nuget push "bin/Release/DotNetDevKit.*.nupkg" \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
```

### Usage:

1. Create a release tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. GitHub Actions automatically:
   - Builds the project
   - Creates NuGet package
   - Publishes to NuGet.org

## Best Practices

✅ **Do:**
- Keep version numbers meaningful
- Update release notes for each version
- Include comprehensive documentation
- Test package locally before publishing
- Use meaningful package metadata
- Include examples and usage docs
- Keep dependencies minimal

❌ **Don't:**
- Publish unfinished code
- Use unclear version numbers
- Skip documentation
- Publish over existing versions
- Include large/unnecessary files
- Use popular package names that already exist

## Quick Command Checklist

```bash
# 1. Build and pack
dotnet build -c Release
dotnet pack -c Release

# 2. Verify locally
dotnet add package DotNetDevKit --source bin/Release

# 3. Publish
nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  -ApiKey YOUR_API_KEY \
  -Source https://api.nuget.org/v3/index.json

# 4. Verify
# Visit https://www.nuget.org/packages/DotNetDevKit
# Wait 5-10 minutes for indexing
```

## Resources

- 📖 [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- 🔗 [NuGet.org](https://www.nuget.org)
- 📦 [Create .NET Package](https://docs.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package-using-visual-studio)
- 🏷️ [Semantic Versioning](https://semver.org/)
- ⚙️ [.csproj Reference](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props)

## Support

- 📧 NuGet Support: https://www.nuget.org/policies/Contact
- 💬 NuGet Issues: https://github.com/NuGet/Home/issues
- 🤝 Community: https://stackoverflow.com/questions/tagged/nuget

---

**You're ready to publish!** 🚀
