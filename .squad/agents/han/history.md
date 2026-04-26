# Han's History

**Project:** ElBruno.AspireMonitor
**User:** Bruno Capuano (ElBruno)
**Role:** Frontend Dev (UI/UX)
**Created:** 2026-04-26

## Session Log

### 2026-04-26 — Team Initialization (Session 1)

**Project Overview:**
- Windows system tray monitor for Aspire
- WPF-based notification window with real-time resource monitoring
- Features: Clickable URLs, color-coded status, system tray integration
- Reference: OllamaMonitor (similar WPF system tray app)

**My Responsibilities:**
1. Design and build WPF MainWindow (notification UI)
2. Implement system tray integration (NotifyIcon, tray menu)
3. Create clickable URL controls (open in browser)
4. Display color-coded status (🟢 Green, 🟡 Yellow, 🔴 Red)
5. Build configuration UI (settings dialog)
6. Implement MVVM pattern (INotifyPropertyChanged for real-time updates)

**Architecture Decisions (Locked):**
- ✅ WPF Framework (.NET 10)
- ✅ MVVM pattern for data binding
- ✅ Color thresholds: Green (<70% CPU+MEM), Yellow (70-90%), Red (>90%)
- ✅ System tray: minimize/restore, context menu, status icon

---

## Learnings

### WPF + System Tray Architecture

1. **MainWindow.xaml:**
   - Standard Windows window (minimizable, closable)
   - When closed, stays in tray (don't exit)
   - Resize/drag behavior preserved
   - Binding to view model (DataContext)

2. **System Tray Integration:**
   - NotifyIcon from Windows Forms
   - Context menu: Show/Hide, Settings, Exit
   - Dynamic icon based on status (color)
   - Double-click to restore window

3. **Data Binding:**
   - View Models: INotifyPropertyChanged
   - Properties: HostUrl, ResourceList, CurrentStatus, IsConnected
   - Updates trigger UI refresh without blocking

4. **Interaction:**
   - URLs as Hyperlink or Button with click handler
   - Click → open in default browser (Process.Start)
   - Config button → open settings dialog (separate window)

---

## Session Log

### 2026-04-26 — WPF Structure Design (Session 2)

**Completed:**

1. ✅ Created project structure:
   - src/ElBruno.AspireMonitor/ (main WPF project)
   - Views/, ViewModels/, Models/, Services/, Resources/, Infrastructure/

2. ✅ Created ElBruno.AspireMonitor.csproj:
   - Target: .NET 10 with WPF + Windows Forms
   - OutputType: WinExe (Windows-only)
   - UseWPF=true, UseWindowsForms=true (for NotifyIcon)

3. ✅ Designed MainWindow.xaml:
   - Top: Host URL (clickable), connection status, last updated
   - Middle: Scrollable resource list with status indicators
   - Bottom: Refresh, Settings, Close buttons
   - Data binding: ItemsSource, Commands, Converters

4. ✅ Designed SettingsWindow.xaml:
   - Configuration UI for Aspire endpoint, polling interval, thresholds
   - Input validation with error messages
   - Modal dialog (ShowDialog)

5. ✅ Created ViewModels with INotifyPropertyChanged:
   - MainViewModel: Host URL, Resources collection, Commands
   - ResourceViewModel: Name, Status, CPU/Memory usage, URL
   - ConfigurationViewModel: Settings with validation logic

6. ✅ Created Infrastructure:
   - ViewModelBase (INotifyPropertyChanged base class)
   - RelayCommand (ICommand implementation)
   - BoolToVisibilityConverter (data binding helper)

7. ✅ System Tray Integration Plan:
   - NotifyIcon with context menu (Show, Settings, Exit)
   - Double-click to toggle visibility
   - Minimize to tray (not close)

8. ✅ Status Color Logic:
   - Green: CPU+MEM avg < 70%
   - Yellow: CPU+MEM avg 70-90%
   - Red: CPU+MEM avg > 90%
   - Gray: Disconnected/stopped

9. ✅ Build verification: Project compiles successfully

**Integration Points for Luke:**
- MainViewModel.RefreshData() needs AspireApiService
- ResourceViewModel properties match API data model
- ConfigurationViewModel.AspireEndpoint for base URL

---

## Next Actions

1. ⏳ Wait for Luke's AspireApiService implementation
2. Integrate API service into MainViewModel
3. Add timer-based polling with ConfigurationViewModel.PollingInterval
4. Create icon assets (Resources/icon.ico with color variants)
5. Implement configuration persistence (JSON file or registry)
6. Test end-to-end with real Aspire dashboard
