# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-01-12

### Added
- Verbose command line flag (`--verbose`) for detailed error output
- HTTP timeout protection (5 minutes) to prevent hanging
- Better resource management with proper disposal of HttpClient
- Enhanced process management with async operations
- More specific exception handling for file operations
- Assembly metadata including version, description, and repository information
- Documentation file generation for the project
- Comprehensive README with badges, detailed usage instructions, and troubleshooting

### Changed
- Improved async/await patterns throughout the codebase
- Replaced Thread.Sleep with Task.Delay for better async performance
- Enhanced error handling with more specific exception types
- Better file path handling using Path.Combine consistently
- Process killing operations now use async methods
- Updated project structure with better metadata

### Fixed
- Resource disposal issues with Process objects
- Potential deadlocks with synchronous operations in async context
- File path concatenation issues on different Windows configurations
- Memory leaks from undisposed resources

### Improved
- Code documentation and inline comments
- Error messages are more descriptive and actionable
- Better separation of concerns in the codebase
- More robust process management when handling locked files

## [1.0.0] - 2025-01-01

### Added
- Initial release of ChromeDriver Updater
- Automatic Chrome version detection from Windows Registry and file system
- Compatible ChromeDriver download using Chrome for Testing API
- Smart process management for locked ChromeDriver files
- Support for .NET 8.0
- Basic error handling and logging
- Integration with Sysinternals Handle tool for process management
- MIT License
- Basic README documentation
- GitHub Actions CodeQL analysis workflow
