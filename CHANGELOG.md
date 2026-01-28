# Changelog

All notable changes to the LT Messages Blish HUD module will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [0.8.5] - 2026-01-27 (First Public Release)

### Changed
- Corner icon click behavior: Left-click shows message popup, Right-click opens editor
- Updated tooltip to reflect correct click actions

## [0.8.4] - 2026-01-27

### Changed
- Right-click on corner icon now opens the message editor (previously showed context menu)
- Updated corner icon tooltip to show left-click and right-click actions
- Removed context menu from corner icon

## [0.8.3] - 2026-01-27

### Added
- "Messages" header label to popup window for better clarity

## [0.8.2] - 2026-01-27

### Fixed
- Close button now properly positioned and visible on popup window when resized

## [0.8.1] - 2026-01-27

### Changed
- Updated module description text
- Fixed grammar and spelling in description

## [0.8.0] - 2026-01-27

### Added
- X close button in top-right corner of popup window for easier mouse-based closing
- Optional keybind for toggling LT Mode on/off (no default binding)
- Optional keybind for opening the message editor window (no default binding)

### Changed
- Removed default Home key binding for popup keybind - users must now explicitly bind a key if they want keyboard access to the popup
- Updated popup window layout to accommodate close button
- Improved keybind descriptions to clarify optional nature

### Fixed
- Popup window now properly sizes with close button area

---

## Core Features (v0.8.5)

This first public release includes:

### Message Management
- Send preset messages to squad/subgroup chat with one click
- 17 default commander/LT messages included
- In-game message editor for adding, editing, and deleting messages
- File-based configuration with auto-reload
- Support for unlimited custom messages

### User Interface
- Corner icon with left-click (show messages) and right-click (open editor)
- Popup message menu at cursor with GW2-styled appearance
- X button, ESC key, or click-outside to close popup
- "Messages" header for clarity

### Sending Options
- **Auto-send mode**: Automatically types and sends messages to chat
- **Clipboard mode**: Copies message to clipboard for manual sending
- Support for `/squad` and `/subgroup` chat commands
- Adjustable typing speed (50-500ms)

### Safety & Control
- **LT Mode toggle**: Prevent accidental sends when not commanding
- Warning notifications when LT Mode is disabled
- Character limits enforce GW2 chat restrictions

### Optional Keybinds
- Popup keybind: Open message popup (no default)
- Toggle LT Mode keybind: Quick on/off toggle (no default)
- Open Editor keybind: Quick editor access (no default)

### Configuration
- Highly configurable settings for all features
- Message file location customizable
- Corner icon can be hidden if desired
