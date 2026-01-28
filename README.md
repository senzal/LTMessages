# LT Messages

**Send preset squad messages with one click!**

Allows LTs and Commanders to send preset squad broadcast messages. Helping squads and LTs literally get the message out!

[![Version](https://img.shields.io/badge/version-0.8.5-blue.svg)](https://github.com/senzal/LTMessages/releases)
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
1. Download `bh.lt.messages_0.8.5.bhm` from [Releases](https://github.com/senzal/LTMessages/releases)
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
- **Click to send**: One click sends your message to squad chat
- **Auto-send or clipboard**: Choose whether messages auto-type or copy to clipboard
- Works with `/squad` and `/subgroup` chat

### ✏️ Message Editor
- **Right-click the [LT] icon** to open the editor
- Add, edit, or delete messages without leaving the game
- Changes save automatically

### 🎨 Easy to Use
- Clean popup menu at your cursor
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

### Default Messages Included
The module comes with 17 helpful messages:
- Tag movement (Moving, Stack on Tag)
- Hero Points (combat and commune instructions)
- Navigation (Waypoint, Vista, POI)
- Combat tips (Red circles, mechanics, CC)
- Squad management (Guard, Help, Special Squad)

---

## Settings

Open Blish HUD settings → LT Messages module

### Essential Settings
- **LT Mode Enabled**: Turn on when you're LT/Commander (prevents accidental sends)
- **Auto-send messages**: On = automatically types messages | Off = copies to clipboard
- **Show Corner Icon**: Display the [LT] icon in Blish menu

### Optional Settings
- **Popup Keybind**: Set a hotkey to open the message popup (optional)
- **Toggle LT Mode Keybind**: Hotkey to toggle LT Mode on/off (optional)
- **Open Editor Keybind**: Hotkey to open the message editor (optional)
- **Chat Method**: Choose Shift+Enter (squad chat) or Shift+/ (chat command)
- **Send delay**: Adjust typing speed (if messages don't send reliably)

---

## Advanced: Editing Messages File

You can also edit messages with a text editor:

1. Navigate to: `Documents\Guild Wars 2\addons\blishhud\ltmessages\messages.txt`
2. Open in Notepad or any text editor
3. Edit and save (changes load automatically)

**Format:**
```
Title,Message
Moving,Tag is moving
Stack,Stack on Tag
```
- Title: Max 16 characters (shown in the popup)
- Message: Max 200 characters (GW2 chat limit)
- Lines starting with # are comments

---

## Troubleshooting

### Messages aren't sending
- Make sure **LT Mode** is enabled in settings
- Enable **Auto-send messages** (or manually paste from clipboard)
- Try increasing **Send delay** in settings

### Popup isn't showing
- Left-click the **[LT] icon** in the corner
- Or set a **Popup Keybind** in settings

### Module not loading
- Check you have **Blish HUD 1.2.0 or newer**
- Make sure the `.bhm` file is in `Documents\Guild Wars 2\addons\blishhud\modules\`
- Check Blish HUD logs: `Documents\Guild Wars 2\addons\blishhud\logs\`

---

## FAQ

**Q: Does it work with subgroup chat?**
A: Yes! Change **Chat Method** to "Shift+/" and **Chat Command** to "/subgroup" in settings.

**Q: Can I use this without being a Commander?**
A: Yes! It works for anyone in a squad. Just enable LT Mode when you want to send messages.

**Q: Will this get me banned?**
A: No. This module uses the same keyboard input as if you typed messages manually. It's a quality-of-life tool.

**Q: Can I have more than 17 messages?**
A: Yes! Use the in-game editor or edit `messages.txt` to add as many as you want.

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

**v0.8.5** (First Release)
- Send preset messages with one click
- In-game message editor
- Optional keybinds for quick access
- LT Mode toggle for safety
- Auto-send or clipboard modes
- 17 default commander/LT messages included

---

## Credits

- **Created by**: Senzall
- **Built for**: [Blish HUD](https://blishhud.com/)
- **Made with ❤️ for the Guild Wars 2 community**

## License

MIT License - see [LICENSE](LICENSE) file for details.
