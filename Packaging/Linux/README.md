# Linux Packaging

This folder contains resources for packaging ProjectTreeViewer on Linux.

## Files

- `projecttreeviewer.desktop` - Desktop entry file for application launchers
- Icons are located in `Assets/AppIcon/Linux/`

## Manual Installation

### 1. Build the application

```bash
dotnet publish Apps/Avalonia/ProjectTreeViewer.Avalonia/ProjectTreeViewer.Avalonia.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -o ./publish/linux
```

### 2. Install the executable

```bash
# System-wide
sudo cp ./publish/linux/ProjectTreeViewer.Avalonia /usr/local/bin/projecttreeviewer
sudo chmod +x /usr/local/bin/projecttreeviewer

# Or user-only
mkdir -p ~/.local/bin
cp ./publish/linux/ProjectTreeViewer.Avalonia ~/.local/bin/projecttreeviewer
chmod +x ~/.local/bin/projecttreeviewer
```

### 3. Install the desktop entry

```bash
# System-wide
sudo cp Packaging/Linux/projecttreeviewer.desktop /usr/share/applications/

# Or user-only
mkdir -p ~/.local/share/applications
cp Packaging/Linux/projecttreeviewer.desktop ~/.local/share/applications/
```

### 4. Install icons

```bash
# System-wide installation
sudo mkdir -p /usr/share/icons/hicolor/{128x128,256x256,512x512,scalable}/apps

sudo cp Assets/AppIcon/Linux/png/128.png /usr/share/icons/hicolor/128x128/apps/projecttreeviewer.png
sudo cp Assets/AppIcon/Linux/png/256.png /usr/share/icons/hicolor/256x256/apps/projecttreeviewer.png
sudo cp Assets/AppIcon/Linux/png/512.png /usr/share/icons/hicolor/512x512/apps/projecttreeviewer.png
sudo cp Assets/AppIcon/Linux/appicon-master.svg /usr/share/icons/hicolor/scalable/apps/projecttreeviewer.svg

# Update icon cache
sudo gtk-update-icon-cache /usr/share/icons/hicolor
```

## Window Icon

The application window icon is set automatically via Avalonia's `Window.Icon` property using the embedded PNG resource. This works independently of the desktop entry and hicolor icons.

## Future: DEB/RPM Packages

For proper distribution packages (`.deb`, `.rpm`, Flatpak, Snap), additional configuration files will be needed. The icon assets and desktop entry in this folder serve as the foundation for those packages.
