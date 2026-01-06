# VB.NET Browser with WebView2

A modern web browser application built with VB.NET Windows Forms, powered by Microsoft WebView2 (Chromium-based rendering engine).

## Project Overview

VB.NET Browser is a fully functional web browser that provides a clean, modern interface with features like tabbed browsing, bookmark management, configurable settings, and a sliding sidebar. It leverages the WebView2 control to render web pages using the Chromium engine, ensuring compatibility with modern web standards.

## System Requirements

- **.NET Version**: .NET 6.0 or higher (Windows target)
- **Operating System**: Windows 10 version 1803 or later, Windows 11
- **Development Environment**: Visual Studio 2022 (recommended) or Visual Studio 2019
- **WebView2 Runtime**: Microsoft Edge WebView2 Runtime (see installation guide below)

## WebView2 Installation Guide

The Microsoft WebView2 Runtime is required for this application to function. It provides the Chromium-based rendering engine.

### Download WebView2 Runtime

Official Microsoft download page:
https://developer.microsoft.com/microsoft-edge/webview2/

Direct download links:
- **Stable Channel**: https://go.microsoft.com/fwlink/p/?LinkId=2124703
- **Evergreen Bootstrapper**: https://go.microsoft.com/fwlink/p/?LinkId=2124701

### Installation Instructions

1. **Download the Runtime**:
   - Visit the official Microsoft WebView2 download page
   - Choose the "WebView2 Runtime" (recommended for end users)
   - Download the installer appropriate for your system (x64, x86, or ARM64)

2. **Run the Installer**:
   - Double-click the downloaded `.exe` file
   - Click "Install" when prompted
   - Wait for the installation to complete
   - Click "Finish" when done

3. **Verification Steps**:
   - Open Windows Settings → Apps
   - Search for "Microsoft Edge WebView2 Runtime"
   - Verify it's installed with a version number
   - Alternatively, check in Control Panel → Programs and Features

4. **Alternative Installation via Command Line**:
   ```bash
   # Using the Evergreen Bootstrapper (recommended)
   MicrosoftEdgeWebview2Setup.exe /silent /install

   # Using the Standalone Installer
   MicrosoftEdgeWebView2RuntimeInstallerX64.exe /silent /install
   ```

5. **Troubleshooting**:
   - If the application fails to start, ensure WebView2 Runtime is installed
   - Check for Windows Updates to ensure your system meets minimum requirements
   - Restart your computer after installation if issues persist

### Additional Information

- The WebView2 Runtime is automatically distributed with Microsoft Edge updates
- For development, you can also use the Evergreen Bootstrapper for automatic updates
- More documentation available at: https://docs.microsoft.com/en-us/microsoft-edge/webview2/

## Project Setup

### Clone Repository

```bash
git clone <repository-url>
cd vbnetbrowser
```

### Install NuGet Packages

Using Visual Studio:
1. Open `vbnetbrowser.vbproj` in Visual Studio 2022
2. Right-click the project in Solution Explorer
3. Select "Manage NuGet Packages"
4. Ensure these packages are installed:
   - `Microsoft.Web.WebView2` (v1.0.2277.86 or later)
   - `System.Text.Json` (v8.0.0 or later)

Using .NET CLI:
```bash
dotnet restore
```

Or install packages manually:
```bash
dotnet add package Microsoft.Web.WebView2 --version 1.0.2277.86
dotnet add package System.Text.Json --version 8.0.0
```

### Visual Studio Project Setup

1. **Open Project**:
   - Launch Visual Studio 2022
   - File → Open → Project/Solution
   - Select `vbnetbrowser.vbproj`

2. **Configure Build Settings**:
   - Right-click project → Properties
   - Application → Target framework: .NET 6.0 (or later)
   - Ensure "Use Windows Forms" is checked

3. **Restore Packages**:
   - Build → Restore NuGet Packages
   - Wait for all packages to download

### Build Instructions

**Debug Build**:
```bash
dotnet build --configuration Debug
```

Or in Visual Studio:
- Press `F6` or Build → Build Solution

**Release Build**:
```bash
dotnet build --configuration Release
```

Or in Visual Studio:
- Change configuration dropdown to "Release"
- Press `F6` or Build → Build Solution

### Run Instructions

**From Visual Studio**:
- Press `F5` to run with debugging
- Press `Ctrl+F5` to run without debugging

**From Command Line**:
```bash
# Debug
dotnet run --configuration Debug

# Release
dotnet run --configuration Release
```

**Run Executable Directly**:
After building, navigate to:
- Debug: `bin/Debug/net6.0-windows/vbnetbrowser.exe`
- Release: `bin/Release/net6.0-windows/vbnetbrowser.exe`

## Features Overview

### Tabbed Navigation
- **Create Tabs**: Click the `+` button to open a new tab
- **Switch Tabs**: Click on any tab header to switch between tabs
- **Close Tabs**: Click the `×` button on each tab to close it (last tab cannot be closed)
- **URL Display**: Address bar shows the current URL of the active tab
- **Dynamic Titles**: Tab titles update automatically based on page titles

### Sliding Sidepanel
- **Collapsible Sidebar**: Click the `«` or `»` button to toggle sidebar visibility
- **Smooth Animation**: Sidebar slides in and out with a smooth animation effect
- **Content**: Houses bookmarks list, navigation controls, and settings access
- **Persistent State**: Sidebar collapsed state is saved in configuration

### Bookmark System
- **Add Bookmark**: Click the `★` button to save the current page as a bookmark
- **Display Bookmarks**: All bookmarks are listed in the sidebar
- **Navigate to Bookmark**: Double-click any bookmark to navigate to that page
- **Delete Bookmark**: Select a bookmark and click "Delete" to remove it
- **Persistence**: Bookmarks are saved to `data/bookmarks.json` automatically

### Configuration Zone
- **Settings Access**: Click the "Settings" button in the sidebar
- **General Settings**: Configure home page, search engine, and session preferences
- **Appearance Settings**: Adjust sidebar width and theme
- **Feature Toggles**: Enable/disable history and bookmarks
- **Persistence**: Settings are saved to `data/config.json`

### Account Setup (Initial)
- **Login Form**: Appears on application startup
- **Credentials**: Enter username and password (any credentials accepted for now)
- **Session Management**: Session state placeholder (full implementation deferred)
- **Validation**: Basic validation for empty fields

## File Structure

```
/vbnetbrowser
  /src
    /Forms
      - MainForm.vb                 # Main browser window with WebView2, tabs, sidepanel
      - MainForm.Designer.vb        # UI designer code for MainForm
      - LoginForm.vb               # Account setup/login form
      - LoginForm.Designer.vb       # UI designer code for LoginForm
      - SettingsForm.vb            # Configuration/settings form
      - SettingsForm.Designer.vb    # UI designer code for SettingsForm
    /Services
      - BookmarkService.vb         # Bookmark management and persistence
      - ConfigService.vb           # Settings management and persistence
    /Models
      - Bookmark.vb                # Bookmark data model
      - AppConfig.vb               # Application configuration model
    - Program.vb                   # Application entry point
  /data
    - bookmarks.json               # Persistent bookmark storage
    - config.json                 # Persistent settings storage
  - vbnetbrowser.vbproj           # .NET project file
  - README.md                     # This file
```

### Component Descriptions

**Forms**:
- `MainForm`: The primary browser interface containing the WebView2 controls, tab management, address bar, navigation buttons, and sidebar
- `LoginForm`: Modal dialog for user login (shown on startup)
- `SettingsForm`: Configuration dialog for browser preferences

**Services**:
- `BookmarkService`: Handles CRUD operations for bookmarks with JSON persistence
- `ConfigService`: Manages application settings with JSON persistence

**Models**:
- `Bookmark`: Data structure for bookmark entries
- `AppConfig`: Configuration options for the browser

## Configuration

### Accessing Settings

1. Launch the application
2. Click the "Settings" button in the left sidebar
3. Modify desired settings
4. Click "Save" to apply changes

### Configuration Options

**General Settings**:
- **Home Page**: The URL loaded when the browser starts or when clicking "Home"
- **Search Engine**: The search URL used for non-URL text in the address bar
- **Remember Session**: Whether to maintain session data (placeholder for future implementation)

**Appearance Settings**:
- **Sidebar Width**: Width of the left sidebar in pixels (default: 250)
- **Theme**: Color theme selection (Light/Dark)

**Feature Settings**:
- **Enable History**: Toggle history tracking (placeholder for future implementation)
- **Enable Bookmarks**: Toggle bookmark functionality

### Configuration File

Configuration is stored in `data/config.json` in JSON format:

```json
{
  "HomePage": "https://www.bing.com",
  "DefaultSearchEngine": "https://www.bing.com/search?q=",
  "RememberSession": true,
  "SidebarWidth": 250,
  "SidebarCollapsed": false,
  "Theme": "Light",
  "EnableHistory": true,
  "EnableBookmarks": true
}
```

### Resetting Configuration

To reset to default settings:
1. Open Settings dialog
2. Click the "Reset" button
3. Confirm the reset action

Or manually delete `data/config.json` and restart the application.

## Usage Guide

### Basic Navigation

1. **Navigate to a URL**:
   - Type a URL in the address bar (e.g., `https://www.example.com`)
   - Press Enter or click "Go"

2. **Search the Web**:
   - Type a search term in the address bar
   - Press Enter - the browser will search using the configured search engine

3. **Navigation Buttons**:
   - `←` (Back): Go to the previous page
   - `→` (Forward): Go to the next page
   - `↻` (Refresh): Reload the current page
   - `Home`: Navigate to the configured home page

### Tab Management

1. **Open a New Tab**:
   - Click the `+` button in the address bar
   - The new tab loads the configured home page

2. **Switch Between Tabs**:
   - Click on any tab header to activate it

3. **Close a Tab**:
   - Click the `×` button on the tab header
   - Note: The last tab cannot be closed

### Managing Bookmarks

1. **Add a Bookmark**:
   - Navigate to the page you want to bookmark
   - Click the `★` button in the address bar
   - A confirmation message will appear

2. **Navigate to a Bookmark**:
   - Click the sidebar toggle button (if collapsed)
   - Double-click any bookmark in the list

3. **Delete a Bookmark**:
   - Select a bookmark in the sidebar list
   - Click the "Delete" button
   - Confirm the deletion

### Using the Sidebar

1. **Toggle Sidebar**:
   - Click the `«` button (top-right of sidebar) to collapse
   - Click the `»` button to expand

2. **Sidebar Contents**:
   - Bookmarks list
   - Settings button
   - Delete bookmark button (when a bookmark is selected)

### Login

On application startup:
1. Enter a username (any value accepted)
2. Enter a password (any value accepted)
3. Click "Login" or press Enter
4. The main browser window will open

## Future Enhancements

Planned features and improvements:

### Short-Term
- [ ] Implement full authentication system with user accounts
- [ ] Add browsing history with history viewer
- [ ] Implement download manager
- [ ] Add print functionality
- [ ] Create bookmarks export/import feature
- [ ] Add page zoom controls

### Medium-Term
- [ ] Implement incognito/private browsing mode
- [ ] Add password manager integration
- [ ] Create bookmark folders/organizational structure
- [ ] Add favorites/start page with quick links
- [ ] Implement tab pinning
- [ ] Add keyboard shortcuts customization

### Long-Term
- [ ] Browser extensions support
- [ ] Synchronization across devices
- [ ] Advanced developer tools
- [ ] Ad blocking capability
- [ ] Reading mode/distraction-free view
- [ ] Tab groups and workspaces
- [ ] Voice search integration
- [ ] Custom themes and UI skins

## Troubleshooting

### Application Won't Start

**Problem**: Application fails to launch with WebView2 error

**Solution**:
1. Verify WebView2 Runtime is installed (see WebView2 Installation Guide)
2. Check Windows version compatibility (Windows 10 1803+)
3. Ensure .NET 6.0 or later is installed
4. Run the application with administrator privileges

### WebView2 Not Rendering

**Problem**: Pages appear blank or don't load

**Solution**:
1. Check internet connection
2. Verify the URL is correct
3. Try refreshing the page
4. Check Windows Firewall/antivirus settings
5. Clear browser cache if applicable

### Bookmarks Not Saving

**Problem**: Bookmarks disappear after restarting the application

**Solution**:
1. Ensure the application has write permissions to the `data/` folder
2. Check that `data/bookmarks.json` exists and is not corrupted
3. Verify the application is not running from a read-only location

### Settings Not Persisting

**Problem**: Settings revert to defaults after restart

**Solution**:
1. Ensure the application has write permissions to the `data/` folder
2. Check that `data/config.json` exists and is not corrupted
3. Verify the JSON file is properly formatted

## Development Notes

### Code Conventions

- Follow VB.NET naming conventions (PascalCase for public members, camelCase for parameters)
- Use meaningful variable and method names
- Add XML comments for public APIs
- Keep methods focused and concise

### Adding New Features

When adding new features:
1. Update the appropriate Service class (BookmarkService, ConfigService)
2. Create or update Model classes as needed
3. Modify Forms to expose new functionality
4. Update data models if persistence is required
5. Test thoroughly before committing

### Building from Source

```bash
# Clone the repository
git clone <repository-url>
cd vbnetbrowser

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## License

[Specify your license here]

## Contributing

[Specify contribution guidelines here]

## Support

For issues, questions, or suggestions, please [open an issue](<repository-issues-url>) or contact the development team.

## Credits

- Built with [Microsoft WebView2](https://developer.microsoft.com/microsoft-edge/webview2/)
- Powered by the [Chromium](https://www.chromium.org/) engine
- Framework: [.NET 6](https://dotnet.microsoft.com/)
