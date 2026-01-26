# LT Messages

A [Blish HUD](https://blishhud.com/) module for Guild Wars 2 that enables Commanders and Lieutenant Tags to quickly send pre-configured messages to squad or subgroup chat with a single click.

[![Version](https://img.shields.io/badge/version-0.7.0-blue.svg)](https://github.com/senzal/LTMessages/releases)
[![Blish HUD](https://img.shields.io/badge/Blish%20HUD-1.2.0%2B-orange.svg)](https://blishhud.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## Features

### 🎯 Quick Message Sending
- **One-click messaging**: Press Home key (configurable) to show popup menu, click message to send
- **Auto-send mode**: Automatically types and sends messages to squad/subgroup chat
- **Clipboard mode**: Copies message to clipboard for manual sending
- **Two chat methods**:
  - Direct squad chat (Shift+Enter)
  - Chat commands (/squad or /subgroup)

### ✏️ In-Game Message Editor
- **Full message management** without leaving the game
- Add, edit, and delete messages through intuitive UI
- **TextBox controls** with character counters
- Save changes to file on demand
- Changes reflected immediately in popup menu

### 🎨 User Interface
- **Popup menu** at cursor with GW2-styled appearance
- **Corner icon** in Blish HUD menu bar
- **Context menu** on right-click
- Close popup with ESC key or click outside
- Hover effects and visual feedback

### 🛡️ Safety Features
- **LT Mode toggle**: Prevents accidental sends when not acting as LT/Commander
- **Character limits**: Enforces GW2 chat limits (200 characters default)
- **Warning notifications**: Alerts when LT Mode is disabled

### ⚙️ Highly Configurable
- Customizable keybind for popup
- Adjustable typing speed (50-500ms delays)
- Choice of chat method and command
- Configurable message length limits
- Optional corner icon display

## Installation

### From Blish HUD Module Repo (Recommended)
1. Open Blish HUD
2. Navigate to **Modules** → **Manage Modules**
3. Search for "LT Messages"
4. Click **Install**
5. Enable the module

### Manual Installation
1. Download the latest `LTMessages.bhm` from [Releases](https://github.com/senzal/LTMessages/releases)
2. Place it in: `Documents\Guild Wars 2\addons\blishhud\modules\`
3. Restart Blish HUD
4. Enable the module in Blish HUD settings

## Usage

### Quick Start
1. **Join or create a squad** in Guild Wars 2
2. **Enable LT Mode** in module settings (if you're LT or Commander)
3. **Press Home key** (or your configured keybind) to show popup
4. **Click a message** to send it to squad chat

### Managing Messages

#### In-Game Editor (Recommended)
1. Open Blish HUD settings
2. Navigate to **LT Messages** module
3. Toggle **"Open Message Editor"** on
4. Use the editor window to:
   - View all current messages
   - Click **Edit** to modify a message
   - Click **Delete** to remove a message
   - Click **Add New Message** to create one
   - Click **Save to File** to persist changes

#### External Editing
Alternatively, edit the messages file directly:
1. Navigate to: `Documents\Guild Wars 2\addons\blishhud\ltmessages\messages.txt`
2. Open in any text editor (Notepad, VS Code, etc.)
3. Edit and save (changes auto-reload)

**File Format:**
```
# Lines starting with # are comments
Title,Message
Moving,Tag is moving
Stack,Stack on Tag
HP-Combat,Please let Tag start the combat hp!
HP-Commune,Commune with HP and then stack on tag!
```
- **Title**: Max 16 characters (shown in menu)
- **Message**: Max 200 characters (configurable)

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **LT Mode Enabled** | Enable to allow sending messages | On |
| **Message File Path** | Location of messages.txt | `Documents\Guild Wars 2\addons\blishhud\ltmessages\messages.txt` |
| **Popup Keybind** | Key to show message popup | Home |
| **Show Corner Icon** | Display icon in Blish menu | On |
| **Auto-send messages** | Automatically send vs clipboard only | Off |
| **Send delay (ms)** | Delay between keystrokes | 200ms |
| **Chat Method** | Shift+Enter or Shift+/ | Shift+Enter |
| **Chat Command** | /squad or /subgroup | /squad |
| **Max Message Length** | Character limit per message | 200 |

## Default Messages

The module comes with 17 pre-configured messages:
- Tag is moving
- Stack on Tag
- Please let Tag start the combat hp!
- Commune with HP and then stack on tag!
- Port is on the marker
- F the Vista and then Stack on Tag
- Point of Interest on Tag!
- Bunny up for CC
- Take the Waypoint.
- Woosh the Waypoint
- If it is red make it dead!
- Don't stand in the red circles
- Watch for the bounty mechanics
- If you get lost ask for help!
- We need 1-2 people to guard this spot
- Special Squad can come get their loot
- Please don't drop EMPs or other items. Let Commander setup stations.

## How It Works

### Technical Details
- Uses **Windows SendInput API** for keyboard input injection
- **Character-by-character typing** to bypass GW2 chat paste restrictions
- **Focus management** to ensure GW2 window receives input
- **STA threading** for clipboard operations
- **FileSystemWatcher** for automatic file reload

### Message Sending Flow
1. User selects message from popup
2. Module checks if LT Mode is enabled
3. Module focuses GW2 window
4. Opens chat (Shift+Enter or Shift+/)
5. Types message character-by-character
6. Sends with Enter key
7. Hides popup

## Building from Source

### Prerequisites
- .NET Framework 4.8 SDK
- Visual Studio 2019+ or VS Code with C# extension
- Blish HUD 1.2.0+

### Build Steps
```bash
# Clone the repository
git clone https://github.com/senzal/LTMessages.git
cd LTMessages

# Restore dependencies
dotnet restore

# Build
dotnet build

# Output will be in: bin/Debug/net48/LTMessages.bhm
```

### Development Setup
1. Edit `Properties/launchSettings.json` with your Blish HUD path
2. Build the project
3. Copy `.bhm` file to Blish HUD modules folder
4. Restart Blish HUD

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Guidelines
- Follow existing code style
- Update version number in `manifest.json`
- Test thoroughly in-game before submitting
- Update README with new features
- Add comments for complex logic

## Known Limitations

- **No automatic LT detection**: GW2 MumbleLink API doesn't expose lieutenant status
- **Manual LT Mode toggle required**: Users must enable/disable manually
- **Windows only**: Uses Windows-specific APIs for keyboard input
- **Requires window focus**: GW2 window must be focused for sending

## Troubleshooting

### Messages not sending
- Verify **LT Mode** is enabled in settings
- Check **Auto-send** is enabled (or use clipboard mode)
- Adjust **Send delay** if typing too fast/slow
- Ensure GW2 window has focus

### Module not loading
- Check Blish HUD version is 1.2.0+
- Verify `.bhm` file is in correct modules folder
- Check Blish HUD logs in `Documents\Guild Wars 2\addons\blishhud\logs\`

### Popup not showing
- Verify keybind isn't conflicting with GW2 keybinds
- Check module is enabled in Blish HUD settings
- Try resetting keybind to default (Home key)

## Support

- **Issues**: [GitHub Issues](https://github.com/senzal/LTMessages/issues)
- **Discord**: [Blish HUD Discord](https://discord.gg/FYKN3qh)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- **Author**: Senzall
- **Blish HUD**: [blish-hud.com](https://blishhud.com/)
- **Icons**: Custom GW2-styled "LT" icon

## Changelog

### v0.7.0 (Current)
- Updated default messages with 17 comprehensive commander/LT messages
- Added Hero Point (HP) combat and commune messages
- Added waypoint, vista, and POI navigation messages
- Added tactical messages (bunny, guard, mechanics)
- Added special squad and EMP drop warnings

### v0.6.0
- Added LT Mode toggle for safety
- Corner icon shows LT Mode status in tooltip
- Warning notification when trying to send with LT Mode disabled

### v0.5.0
- Full in-game message editor
- Add/Edit/Delete messages without leaving game
- Character counter in edit dialog

### v0.4.0
- Changed to Blish standard file location
- Embedded default messages
- Module works immediately without file

### v0.3.0
- Added ESC key and click-outside to close popup
- Configurable max message length
- Improved default messages file

### v0.2.0
- Dual chat method support (Shift+Enter and Shift+/)
- /squad and /subgroup command options

### v0.1.0
- Improved popup UI
- Fixed text rendering issues

### v0.0.9
- Character-by-character typing implementation
- Fixed clipboard paste issues with GW2 chat

---

**Made with ❤️ for the Guild Wars 2 community**
