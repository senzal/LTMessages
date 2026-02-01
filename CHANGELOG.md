# Changelog

All notable changes to the LT Messages Blish HUD module will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [0.9.0] - 2026-01-31 (Major Feature Update)

### Added
- **Custom List Names**: Rename any of the 6 message lists with custom names (up to 30 characters)
  - "Rename" button in message editor
  - List 0 defaults to "Default", others are "List 1-5"
  - Names persist across sessions
- **Refactored Chat System** with three new settings:
  - **Chat Focus**: Choose how to open chat (Shift+Enter for squad chat, or Enter for last used chat)
  - **Chat Action**: Send (auto-type) or Paste Only (clipboard)
  - **Chat Command**: 20 channel options including:
    - Default (use current channel)
    - /squad, /subgroup
    - /1 through /5 (party member whispers)
    - /guild, /g1 through /g6 (guild channels)
    - /say, /map, /party, /team
- **Typing Delay**: New dropdown with 16 optimized delay options (5-150ms, default 40ms)
  - Finer control at lower delays where precision matters
  - Migration support for old settings
- **Help Dialog**: Comprehensive in-game help system
  - "Help" button in message editor
  - "Show Help" toggle in settings panel
  - Covers quick start, settings explanations, common configurations, and troubleshooting
  - Scrollable content with fixed title bar
  - Thank you message to GW2 and Blish HUD communities

### Changed
- Maximum message length updated to 199 characters (GW2's actual limit, was 200)
- Default typing delay reduced from 200ms to 40ms for faster message sending
- Enhanced manifest description with chat_shorts plugin recommendation

### Removed
- "Max Message Length" setting (now hardcoded to 199 as it's a GW2 limitation)
- "Active Message List" and "List Names Data" from settings UI (now stored internally)

### Technical
- Internal data (active list, custom names) now stored in `module_data.txt` instead of settings
- Settings panel is cleaner with only user-relevant controls visible
- Added automatic migration for invalid delay values from previous versions

---

## [0.8.6] - 2026-01-28 (Complete Commander Toolkit)

### Added
- "Restore Defaults" button in message editor
  - Shows confirmation dialog before restoring
  - Automatically saves to file after restoring
- 13 new critical commander messages for comprehensive coverage:
  - **Movement**: Wait, Stop, Portal, Unlock-WP
  - **Combat**: Focus, Spread, Rez, Stealth, Blast, Safe
  - **Objectives**: POI-Marker
  - **Squad Management**: Buffs, Loot
- Now includes 30 total default messages covering all essential commander needs

### Changed
- Updated default messages to use authentic GW2 commander language
- Reorganized messages in logical commanding flow order:
  - Pre-Movement & Positioning: 5 messages (Stack, Wait, Buffs, Stealth, Blast)
  - Movement Commands: 7 messages (Moving, Stop, Port, Portal, Unlock-WP, Take-WP, Link-WP)
  - Combat - Priority Actions: 8 messages (Focus, Kill-Adds, Spread, Dodge, Rez, Mount-CC, Need-CC, Safe)
  - Objectives: 5 messages (HP-Combat, HP-Commune, F-Vista, POI-Tag, POI-Marker)
  - Squad Management: 5 messages (Guard, Loot, Help, No-Drop, Break)
- Improved waypoint messages for map completion trains:
  - Added "Unlock-WP" for activating waypoints
  - Separated "Take-WP" (teleport) from "Link-WP" (ping in chat)
  - All three cover different waypoint scenarios
- Changed "Ping-WP" to "Link-WP" (more common GW2 terminology)
- Improved message phrasing for authenticity and clarity
- Fixed spelling: "Springer" (was "Spriger"), "dropped" (was "droped")

### Milestone
- v1.0.0 represents a complete, production-ready commander toolkit with all essential messages

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
