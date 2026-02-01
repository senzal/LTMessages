# LT Messages

**Send preset squad messages with one click!**

Allows LTs and Commanders to send preset squad broadcast messages. Helping squads and LTs literally get the message out!

[![Version](https://img.shields.io/badge/version-0.9.0-blue.svg)](https://github.com/senzal/LTMessages/releases)
[![Blish HUD](https://img.shields.io/badge/Blish%20HUD-1.2.0%2B-orange.svg)](https://blishhud.com/)

---

## What Does It Do?

As a Lieutenant or Commander in Guild Wars 2, you often need to send the same messages to your squad:
- "Tag is moving"
- "Stack on Tag"
- "Take the Waypoint"
- "Don't stand in the red circles"

This module lets you send these messages with a single click, either automatically typed into squad chat or copied to your clipboard.

---

## Quick Start

### Installation
1. Download `bh.lt.messages_0.9.0.bhm` from [Releases](https://github.com/senzal/LTMessages/releases)
2. Place it in: `Documents\Guild Wars 2\addons\blishhud\modules\`
3. Restart Blish HUD
4. Enable the module

### Basic Usage
1. Look for the **[LT] icon** in your Blish HUD corner menu
2. **Left-click** to see your messages
3. Click any message to send it to squad chat

That's it!

---

## Features

### 📨 Message Sending
- **Click to send**: One click sends your message to any chat channel
- **Flexible chat targeting**: Send to squad, map, party, guild, whispers, and more
- **Chat Focus**: Choose Shift+Enter (squad) or Enter (last used chat)
- **Chat Action**: Auto-send (types message) or Paste Only (clipboard)
- **20+ chat channels**: /squad, /map, /party, /guild, /say, /team, and more

### 📋 Multiple Message Lists
- **6 customizable lists**: Organize messages for different events
- **Custom list names**: Rename lists to "WvW", "Meta Events", "HP Trains", etc.
- **Quick switching**: Dropdown selector in editor to switch between lists

### ✏️ Message Editor
- **Right-click the [LT] icon** to open the editor
- Add, edit, or delete messages without leaving the game
- Rename your message lists with custom names
- Changes save automatically
- **Built-in Help button** for quick reference

### 📖 In-Game Help
- Comprehensive help dialog accessible from editor or settings
- Explains all settings and features
- Common configuration examples
- Troubleshooting guide
- No need to leave the game to find answers

### 🎨 Easy to Use
- Clean popup menu at your cursor
- Customizable typing delay (5-150ms)
- X button to close
- Optional keyboard shortcuts if you want them

### 🛡️ Safety First
- **LT Mode toggle**: Turn off when you're not commanding
- Won't accidentally send messages when disabled

---

## How to Use Messages

### Sending Messages
1. **Left-click the [LT] icon** in the corner
2. A popup appears with your message list
3. Click any message to send it
4. Close the popup by clicking X, pressing ESC, or clicking outside

### Editing Messages
1. **Right-click the [LT] icon** in the corner
2. The message editor opens
3. Click **Edit** to change a message, **Delete** to remove one, or **Add New Message** to create one
4. Click **Save to File** when done
5. Click **Help** for detailed instructions and configuration examples

### Default Messages Included
The module comes with 30 comprehensive messages organized in logical commander flow:

**Pre-Movement & Positioning** (5 messages)
- Stack, Wait, Buffs, Stealth, Blast

**Movement Commands** (7 messages)
- Moving, Stop, Port, Portal, Unlock-WP, Take-WP, Link-WP

**Combat - Priority Actions** (8 messages)
- Focus, Kill-Adds, Spread, Dodge, Rez, Mount-CC, Need-CC, Safe

**Objectives** (5 messages)
- HP-Combat, HP-Commune, F-Vista, POI-Tag, POI-Marker

**Squad Management** (5 messages)
- Guard, Loot, Help, No-Drop, Break

---

## Settings

Open Blish HUD settings → LT Messages module

### Essential Settings
- **LT Mode Enabled**: Turn on when you're LT/Commander (prevents accidental sends)
- **Chat Focus**: Shift+Enter (squad chat) or Enter (last used chat)
- **Chat Action**: Send (auto-type) or Paste Only (clipboard)
- **Chat Command**: Choose which channel to send to (Default, /squad, /map, /party, /guild, etc.)
- **Show Corner Icon**: Display the [LT] icon in Blish menu

### Optional Settings
- **Popup Keybind**: Set a hotkey to open the message popup (optional)
- **Toggle LT Mode Keybind**: Hotkey to toggle LT Mode on/off (optional)
- **Open Editor Keybind**: Hotkey to open the message editor (optional)
- **Typing Delay (ms)**: Adjust typing speed - 16 options from 5-150ms (default: 40ms)
- **Show Help**: Toggle to open the in-game help dialog

---

## Advanced: Editing Messages Files

You can also edit messages with a text editor:

1. Navigate to: `Documents\Guild Wars 2\addons\blishhud\ltmessages\`
2. Edit the message file for your list:
   - `messages.txt` (List 0 - "Default")
   - `messages_1.txt` through `messages_5.txt` (Lists 1-5)
3. Open in Notepad or any text editor
4. Edit and save (changes load automatically)

**Format:**
```
Title,Message
Moving,Tag is moving
Stack,Stack on Tag
```
- Title: Max 16 characters (shown in the popup)
- Message: Max 199 characters (GW2 chat limit)
- Lines starting with # are comments

---

## Troubleshooting

### Messages aren't sending
- Make sure **LT Mode** is enabled in settings
- Set **Chat Action** to "Send" (or use "Paste Only" and manually paste)
- Try increasing **Typing Delay** in settings
- Check that **Chat Command** is set correctly for your target channel

### Popup isn't showing
- Left-click the **[LT] icon** in the corner
- Or set a **Popup Keybind** in settings

### Module not loading
- Check you have **Blish HUD 1.2.0 or newer**
- Make sure the `.bhm` file is in `Documents\Guild Wars 2\addons\blishhud\modules\`
- Check Blish HUD logs: `Documents\Guild Wars 2\addons\blishhud\logs\`

---

## FAQ

**Q: Does it work with other chat channels besides squad?**
A: Yes! Set **Chat Command** to any of 20+ options: /squad, /map, /party, /guild, /say, /team, whispers (/1-/5), guild channels (/g1-/g6), and more.

**Q: Can I use this without being a Commander?**
A: Yes! It works for anyone in a squad. Just enable LT Mode when you want to send messages.

**Q: Will this get me banned?**
A: No. This module uses the same keyboard input as if you typed messages manually. It's a quality-of-life tool.

**Q: Can I organize messages for different events?**
A: Yes! Use the 6 customizable message lists. Rename them to "WvW", "Metas", "HP Trains", etc. and switch between them in the editor.

**Q: Do I need to set up keybinds?**
A: No! Keybinds are completely optional. The corner icon works great on its own.

---

## Support & Contributing

- **Issues or Questions**: [GitHub Issues](https://github.com/senzal/LTMessages/issues)
- **Blish HUD Discord**: [discord.gg/FYKN3qh](https://discord.gg/FYKN3qh)

Pull requests are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for full version history.

**v0.9.0** (Major Feature Update)
- Custom list names for organizing messages
- Refactored chat system with flexible channel targeting
- 20+ chat channel options (squad, map, party, guild, whispers, etc.)
- Optimized typing delay with 16 options (5-150ms, default 40ms)
- 6 customizable message lists
- In-game help dialog with comprehensive guides
- Updated to GW2's actual 199 character limit

---

## Credits

- **Created by**: Senzall
- **Built for**: [Blish HUD](https://blishhud.com/)
- **Made with ❤️ for the Guild Wars 2 community**

## License

MIT License - see [LICENSE](LICENSE) file for details.
