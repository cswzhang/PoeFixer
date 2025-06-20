# PoeFixer

A modified version of [PoeFixer](https://github.com/caltus/PoeFixer), a Path of Exile tool for modifying game assets, particularly UI modifications.

基于[PoeFixer](https://github.com/caltus/PoeFixer)修改.

## 使用说明

1. 选择`Content.ggpk`（官方版本）或`_.index.bin`（Steam版本） `File > Select GGPK`
2. 调整右侧需要的设置，本人目前使用`移除光线阴影`、`改色`、`移除粒子效果`，其中`改色`包括红蓝王祭坛、炸坟、地图反射词缀等，`移除粒子效果`对帧数提升极为明显且不降低原本画质，目前满屏先驱稳定100FPS以上
3. 使用 `File > Patch GGPK` 应用设置

- （可选）`File > Restore Vanilla Assets` 从本地文件中恢复原始内容

## 本项目修改说明

1. 优化使用逻辑：首次Patch GGPK后在本项目运行目录生成`extractedassets`和`modifiedassets`目录，分别对应原始备份（以供恢复）和修改版本
2. 游戏更新后，只有GGG对特效的处理和解包的底层逻辑不改，无需等待本项目更新，只有更新完游戏后删除本项目生成的`extractedassets`目录重新运行即可

## 未来开发内容

1. 针对技能特效修改的支持

## Screenshots

### Main Window
![Main Window](images/main-window.png)

### Color Mod Editor
![Color Mod Editor](images/color-mod-editor.png)

## Features

- GGPK file handling and patching
- Asset extraction and restoration
- Modern, organized UI with grouped settings
- Camera controls (zoom, fog)
- Visual effects customization
- Gameplay modifications

## Recent Updates

### UI Improvements
1. Color Mod Editor
   - Added search functionality for mod descriptions
   - Multi-select support with keyboard shortcuts
   - Context menu for adding/removing items
   - Status notifications for actions
   - Unsaved changes tracking
   - Auto-sizing window

2. Main Window
   - More compact layout
   - Reduced window height
   - Streamlined settings panel
   - Status bar for console output (3 lines)
   - Improved spacing and margins

### Keyboard Shortcuts
- **Color Mod Editor**
  - Enter: Add selected items
  - Ctrl+A: Select all items
  - Delete: Remove selected items
  - Ctrl+S: Save changes

### Technical Changes
- Improved error handling
- Better memory management
- More efficient UI updates
- Enhanced status feedback

## Changes from Original SneedSmoother

### UI Modernization
- Implemented a clean, organized layout
- Grouped settings into logical categories:
  - Camera Settings
  - Visual Effects
  - Gameplay Options
- Improved menu organization with File operations
- Enhanced console output display

### Visual Improvements
- Modern color scheme with blue accents
- Better typography and spacing
- Organized controls in GroupBoxes
- Improved visual hierarchy
- Better button styling and interactions

### Technical Improvements
- Maintained all original functionality
- Improved code organization
- Enhanced user feedback through console
- Better window layout management

## Usage

1. Select your GGPK file using File > Select GGPK
2. Use File > Patch GGPK to apply your settings
3. Extract or restore vanilla assets as needed
4. Adjust settings in the various categories as desired

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

## Requirements

- .NET 8.0 or later
- Path of Exile installation
- packcheck.exe (for asset verification)

## Credits

This is a modified version of [PoeFixer](https://github.com/caltus/PoeFixer). All core functionality remains the same, with improvements focused on user interface and experience.


