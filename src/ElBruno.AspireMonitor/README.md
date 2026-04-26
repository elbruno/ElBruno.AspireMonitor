# ElBruno.AspireMonitor - WPF Application

## Overview

WPF-based system tray application for monitoring .NET Aspire resources in real-time.

## Architecture

### MVVM Pattern

- **Views**: XAML UI files (MainWindow.xaml, SettingsWindow.xaml)
- **ViewModels**: Presentation logic with INotifyPropertyChanged
- **Models**: Data structures (ResourceStatus enum)
- **Infrastructure**: Reusable components (ViewModelBase, RelayCommand, Converters)

### Project Structure

```
src/ElBruno.AspireMonitor/
├── Views/
│   ├── MainWindow.xaml          # Main notification window
│   └── SettingsWindow.xaml      # Configuration dialog
├── ViewModels/
│   ├── MainViewModel.cs         # Main window data binding
│   ├── ResourceViewModel.cs     # Individual resource display
│   └── ConfigurationViewModel.cs # Settings management
├── Models/
│   └── ResourceStatus.cs        # Resource state enum
├── Infrastructure/
│   ├── ViewModelBase.cs         # INotifyPropertyChanged base
│   ├── RelayCommand.cs          # ICommand implementation
│   └── BoolToVisibilityConverter.cs # Binding converter
├── Services/                    # (Reserved for Luke's API service)
└── Resources/                   # Icons and assets
```

## Features Implemented

### 1. MainWindow (UI Layout)
- **Top Section**: Host URL display (clickable), connection status, last updated timestamp
- **Middle Section**: Scrollable resource list with:
  - Status color indicator (🟢 Green, 🟡 Yellow, 🔴 Red, ⚪ Gray)
  - Resource name
  - CPU usage percentage
  - Memory usage percentage
  - Clickable URL (if available)
- **Bottom Section**: Refresh, Settings, Close buttons

### 2. System Tray Integration
- NotifyIcon with context menu (Show, Settings, Exit)
- Double-click to toggle window visibility
- Minimize to tray (not close)
- Dynamic icon based on status (future enhancement)

### 3. Settings Dialog
- Aspire Endpoint configuration
- Polling interval (ms)
- CPU threshold (%)
- Memory threshold (%)
- Start with Windows option
- Input validation with error messages

### 4. Data Binding
- All UI elements use binding (no code-behind logic)
- INotifyPropertyChanged pattern for real-time updates
- Commands for user interactions (RefreshCommand, etc.)
- Color-coded status indicators based on resource metrics

### 5. Status Color Logic
- **Green**: CPU + Memory average < 70%
- **Yellow**: CPU + Memory average 70-90%
- **Red**: CPU + Memory average > 90%
- **Gray**: Disconnected or stopped resources

## Integration Points (For Luke)

The ViewModels are ready to consume data from the Aspire API service:

```csharp
// MainViewModel expects:
- void RefreshData() -> Pull latest resource data
- ObservableCollection<ResourceViewModel> -> Update with API response

// ResourceViewModel properties:
- Name (string)
- Status (ResourceStatus enum)
- CpuUsage (double, 0-100)
- MemoryUsage (double, 0-100)
- Url (string, nullable)

// ConfigurationViewModel:
- AspireEndpoint (string) -> Base URL for API calls
- PollingInterval (int) -> Timer interval in ms
```

## Current State

✅ Project structure created
✅ XAML views designed with data binding
✅ ViewModels implemented with INotifyPropertyChanged
✅ System tray integration framework
✅ Settings dialog with validation
✅ Sample data for UI testing

⏳ Pending Luke's API service integration
⏳ Icon assets (Resources/icon.ico)
⏳ Configuration persistence (file/registry)

## Next Steps

1. **Luke**: Implement AspireApiService to poll Aspire endpoint
2. **Han**: Connect MainViewModel.RefreshData() to AspireApiService
3. **Han**: Add timer-based polling using ConfigurationViewModel.PollingInterval
4. **Han**: Create icon variants (green/yellow/red/gray) for system tray
5. **Team**: Test end-to-end with real Aspire dashboard

## Build & Run

```powershell
cd C:\src\ElBruno.AspireMonitor\src\ElBruno.AspireMonitor
dotnet build
dotnet run
```

**Note**: Requires Windows (.NET 10 with WPF support).
