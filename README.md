# ChromeDriver Updater

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CodeQL](https://github.com/guberm/GetLatestChromeDriver/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/guberm/GetLatestChromeDriver/actions/workflows/codeql-analysis.yml)

A robust .NET 8 console application that automatically downloads and installs the latest compatible ChromeDriver version for your installed Google Chrome browser. Perfect for Selenium automation projects and CI/CD pipelines.

## 🚀 Features

- **🔍 Automatic Chrome Version Detection**: Detects your installed Chrome version from Windows Registry or file system
- **📦 Compatible ChromeDriver Download**: Uses the official Chrome for Testing API to find the correct ChromeDriver version
- **⚡ Smart Process Management**: Automatically terminates any running ChromeDriver processes that might lock the executable
- **🛡️ Robust Error Handling**: Comprehensive error handling with informative messages and optional verbose output
- **🔄 Modern C# Patterns**: Built with .NET 8, async/await, and modern C# best practices
- **⏱️ Timeout Protection**: HTTP requests include timeout protection to prevent hanging
- **📝 Detailed Logging**: Clear progress information and debugging capabilities

## 📋 Prerequisites

- **Operating System**: Windows (Windows 10/11 recommended)
- **Runtime**: .NET 8.0 Runtime or SDK ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Browser**: Google Chrome installed on your system
- **Permissions**: Administrative privileges may be required for process management

## 🛠️ Installation

### Option 1: Download Release (Recommended)
1. Go to the [Releases](../../releases) page
2. Download the latest `GetLatestChromeDriver.zip`
3. Extract to your desired location
4. Run `GetLatestChromeDriver.exe`

### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/guberm/GetLatestChromeDriver.git

# Navigate to the project directory
cd GetLatestChromeDriver

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project GetLatestChromeDriver
```

### Option 3: Self-Contained Deployment
```bash
# Build a self-contained executable (no .NET runtime required on target machine)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 🎯 Usage

### Basic Usage
```bash
GetLatestChromeDriver.exe
```

### Advanced Usage
```bash
# Run with verbose error output
GetLatestChromeDriver.exe --verbose
```

### Sample Output
```
ChromeDriver Updater - Starting...
Detected Chrome version: 131.0.6778.86
Downloading ChromeDriver version: 131.0.6778.86
Downloading from: https://storage.googleapis.com/chrome-for-testing-public/131.0.6778.86/win32/chromedriver-win32.zip
ChromeDriver extraction completed.
ChromeDriver updated successfully!
```

### Integration in Selenium Projects
```csharp
// Before running your Selenium tests, ensure ChromeDriver is updated
Process.Start("GetLatestChromeDriver.exe")?.WaitForExit();

// Now use ChromeDriver in your Selenium code
var driver = new ChromeDriver("path/to/chromedriver/directory");
```

## 🔧 How It Works

### 1. Chrome Version Detection
The application uses a multi-step approach to detect your Chrome version:
- **Primary**: Windows Registry lookup (`HKEY_CURRENT_USER` and `HKEY_LOCAL_MACHINE`)
- **Fallback**: File version information from Chrome executable in standard installation paths

### 2. ChromeDriver Version Lookup
- Queries the official [Chrome for Testing API](https://googlechromelabs.github.io/chrome-for-testing/)
- Finds the latest compatible ChromeDriver version for your Chrome major version
- Ensures ChromeDriver downloads are available for the target version

### 3. Smart Download and Installation
- Downloads the appropriate Windows 32-bit ChromeDriver package
- Extracts ChromeDriver to the application directory
- Handles file locks by safely terminating blocking processes using Sysinternals Handle tool
- Includes retry logic and proper cleanup

## 📁 File Structure

```
GetLatestChromeDriver/
├── 📄 README.md                           # This documentation
├── 📄 LICENSE                             # MIT License
├── 📄 GetLatestChromeDriver.sln           # Visual Studio Solution
├── 📂 .github/workflows/                  # GitHub Actions CI/CD
│   └── 📄 codeql-analysis.yml
├── 📂 GetLatestChromeDriver/               # Main project
│   ├── 📄 GetLatestChromeDriver.csproj    # Project configuration
│   ├── 📄 Program.cs                      # Main application logic
│   ├── 📦 Handle.zip                      # Sysinternals Handle tool
│   └── 📂 Properties/                     # Assembly properties
└── 📂 bin/Debug/net8.0/                   # Build output (after compilation)
```

## 🛡️ Error Handling

The application includes comprehensive error handling for various scenarios:

| Scenario | Handling |
|----------|----------|
| 🚫 Chrome not installed | Clear error message with installation guidance |
| 🌐 Network connectivity issues | Timeout protection and retry suggestions |
| 🔒 File permission problems | Automatic process termination with Handle tool |
| ❌ Invalid Chrome versions | Fallback version detection methods |
| 🔧 ChromeDriver API unavailable | Graceful degradation with helpful error messages |
| ⚠️ Process conflicts | Smart process management and cleanup |

## 🔄 Why Use This Tool?

### For Developers
- **⏱️ Time Saving**: No manual ChromeDriver version checking and downloading
- **🎯 Always Compatible**: Guarantees the correct ChromeDriver version for your Chrome installation
- **🧪 Testing Ready**: Essential for automated browser testing with Selenium WebDriver
- **🔄 CI/CD Friendly**: Perfect for build pipelines and automated deployment processes

### For Teams
- **📊 Consistency**: Ensures all team members use compatible ChromeDriver versions
- **🚀 Productivity**: Eliminates ChromeDriver version mismatch issues
- **🛡️ Reliability**: Reduces test failures due to driver incompatibility

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **🍴 Fork** the repository
2. **🌿 Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **💻 Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **📤 Push** to the branch (`git push origin feature/amazing-feature`)
5. **🔀 Open** a Pull Request

### Development Guidelines
- Follow existing code style and patterns
- Add unit tests for new functionality
- Update documentation for any changes
- Ensure all tests pass before submitting

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🐛 Troubleshooting

<details>
<summary><strong>🔍 "Could not detect Chrome version"</strong></summary>

**Possible Solutions:**
- Ensure Google Chrome is installed and up to date
- Check if Chrome is installed in a non-standard location
- Run the application as Administrator for registry access
- Verify Chrome is not running in sandboxed mode
</details>

<details>
<summary><strong>🌐 "Could not find compatible ChromeDriver"</strong></summary>

**Possible Solutions:**
- Check your internet connection
- Verify your Chrome version is supported (version 70+)
- Try running again later (Chrome for Testing API might be temporarily unavailable)
- Use `--verbose` flag for detailed error information
</details>

<details>
<summary><strong>🔒 "Access denied" errors</strong></summary>

**Possible Solutions:**
- Run the application as Administrator
- Close all Chrome and ChromeDriver instances
- Check if antivirus software is blocking file operations
- Ensure the target directory is writable
</details>

<details>
<summary><strong>⏱️ Application hangs or times out</strong></summary>

**Possible Solutions:**
- Check your internet connection stability
- Verify firewall/proxy settings allow HTTPS connections
- Try running with `--verbose` for detailed progress information
- Restart the application if it appears frozen
</details>

## 🔗 Related Links

- 📚 [ChromeDriver Documentation](https://chromedriver.chromium.org/)
- 🧪 [Chrome for Testing API](https://googlechromelabs.github.io/chrome-for-testing/)
- 🕷️ [Selenium WebDriver Documentation](https://selenium.dev/documentation/webdriver/)
- 🔧 [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- 🛠️ [Sysinternals Handle Tool](https://docs.microsoft.com/en-us/sysinternals/downloads/handle)

## 📊 Version History

| Version | Release Date | Changes |
|---------|--------------|---------|
| 1.1.0 | 2025-01-12 | Improved error handling, async optimizations, better logging |
| 1.0.0 | 2025-01-01 | Initial release with basic functionality |

---

<div align="center">

**⭐ Star this repository if it helped you!**

Made with ❤️ by [guberm](https://github.com/guberm)

</div>
