# ChromeDriver Updater

A .NET 8 console application that automatically downloads and installs the latest compatible ChromeDriver version for your installed Google Chrome browser.

## 🚀 Features

- **Automatic Chrome Version Detection**: Detects your installed Chrome version from Windows Registry or file system
- **Compatible ChromeDriver Download**: Uses the official Chrome for Testing API to find the correct ChromeDriver version
- **Smart Process Management**: Automatically terminates any running ChromeDriver processes that might lock the executable
- **Error Handling**: Comprehensive error handling with informative messages
- **Modern C# Patterns**: Built with .NET 8, async/await, and modern C# best practices

## 📋 Prerequisites

- Windows operating system
- .NET 8.0 Runtime or SDK
- Google Chrome installed

## 🛠️ Installation

### Option 1: Download Release
1. Download the latest release from the [Releases](../../releases) page
2. Extract the files to your desired location
3. Run `GetLatestChromeDriver.exe`

### Option 2: Build from Source
1. Clone this repository:
   ```bash
   git clone https://github.com/guberm/GetLatestChromeDriver.git
   ```
2. Navigate to the project directory:
   ```bash
   cd GetLatestChromeDriver
   ```
3. Build the project:
   ```bash
   dotnet build --configuration Release
   ```
4. Run the application:
   ```bash
   dotnet run --project GetLatestChromeDriver
   ```

## 🎯 Usage

Simply run the executable, and it will:

1. Detect your installed Chrome version
2. Find the compatible ChromeDriver version
3. Download and extract the ChromeDriver to the application directory
4. Clean up temporary files

```bash
GetLatestChromeDriver.exe
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

## 🔧 How It Works

1. **Chrome Version Detection**: 
   - First tries Windows Registry (`HKEY_CURRENT_USER` and `HKEY_LOCAL_MACHINE`)
   - Falls back to reading version from Chrome executable files

2. **ChromeDriver Version Lookup**:
   - Uses the official [Chrome for Testing API](https://googlechromelabs.github.io/chrome-for-testing/)
   - Finds the latest ChromeDriver version compatible with your Chrome version

3. **Download and Installation**:
   - Downloads the appropriate Windows 32-bit ChromeDriver
   - Extracts to the application directory
   - Handles file locks by terminating blocking processes

## 📁 File Structure

```
GetLatestChromeDriver/
├── GetLatestChromeDriver.sln          # Solution file
├── GetLatestChromeDriver/
│   ├── GetLatestChromeDriver.csproj   # Project file
│   ├── Program.cs                     # Main application logic
│   ├── Handle.zip                     # Sysinternals Handle tool (for process management)
│   └── Properties/
└── README.md                          # This file
```

## 🛡️ Error Handling

The application includes comprehensive error handling for common scenarios:

- Chrome not installed or not detectable
- Network connectivity issues
- File permission problems
- Invalid or unsupported Chrome versions
- ChromeDriver API unavailability

## 🔄 Why Use This Tool?

- **Automation**: No manual ChromeDriver version checking and downloading
- **Compatibility**: Always gets the correct ChromeDriver version for your Chrome
- **Selenium Testing**: Essential for automated browser testing with Selenium WebDriver
- **CI/CD Integration**: Can be integrated into build pipelines to ensure compatible drivers

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Troubleshooting

### Common Issues

**"Could not detect Chrome version"**
- Ensure Google Chrome is installed
- Check if Chrome is installed in a non-standard location
- Run as Administrator if registry access is denied

**"Could not find compatible ChromeDriver"**
- Check your internet connection
- Verify that your Chrome version is supported
- The Chrome for Testing API might be temporarily unavailable

**"Access denied" errors**
- Run the application as Administrator
- Ensure no other applications are using ChromeDriver
- Check antivirus software isn't blocking the download

## 🔗 Related Links

- [ChromeDriver Documentation](https://chromedriver.chromium.org/)
- [Chrome for Testing API](https://googlechromelabs.github.io/chrome-for-testing/)
- [Selenium WebDriver](https://selenium.dev/documentation/webdriver/)

---

**Note**: This tool is specifically designed for Windows environments. For cross-platform ChromeDriver management, consider using package managers like npm's `chromedriver` package or similar tools for your specific platform.
