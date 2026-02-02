# LT Messages v0.9.1 - UX Improvements Release

**Release Date**: February 1, 2026
**Type**: Minor Update - Quality of Life Improvements

---

## 🎯 What's New in v0.9.1

This release focuses on improving the user experience with smarter workflow protection, better visual feedback, and polished UI elements.

---

## ✨ New Features

### 1. Unsaved Changes Protection 🛡️

Never lose your work accidentally!

**List Switching Warning**
- Warns when switching lists while editing a message
- Three options: **Save & Switch**, **Discard & Switch**, or **Cancel**
- Prevents accidental loss of work in progress

**Editor Close Warning**
- Warns when closing editor with unsaved edits
- Three options: **Save & Close**, **Discard & Close**, or **Cancel**
- Ensures you never lose changes by accident

**Auto-Save to File**
- Individual message saves now immediately persist to disk
- No more manual "Save All" step needed
- Your changes are safe the moment you click Save

### 2. Mode Tooltip Enhancement 📊

Know your current configuration at a glance!

The [LT] corner icon tooltip now displays your active mode:

```
LT Messages
Left-click: Show messages
Right-click: Open editor
LT Mode: ENABLED
Mode: Enter | Send | Squad
```

**Shows**:
- Chat Focus (Shift+Enter or Enter)
- Chat Action (Send or Paste Only)
- Chat Command (Default/Squad/Map/etc.)
- Updates dynamically when settings change

Perfect for confirming your settings without opening the settings panel!

### 3. Message Popup Improvements 🎨

**Dynamic Sizing**
- Window height adjusts based on message count
- Minimum: 3 message spaces (even with 0-2 messages)
- Maximum: 15 messages visible before scrollbar appears
- Smooth scrolling for 16+ messages

**Visual Polish**
- Perfectly symmetric borders (Left: 6px, Right: 10px, Bottom: 10px, Top: 40px)
- X button positioned correctly at 10px from right edge
- Professional, balanced appearance
- Consistent spacing: 28px per message + 3px padding

**Scrollbar Behavior**
- Only appears when needed (16+ messages)
- Properly updates when switching lists or editing messages
- Smooth, responsive scrolling

### 4. Editor Window Improvements ⚙️

**Dynamic Sizing**
- Shows 3-6 messages with automatic height adjustment
- Scrollbar appears for 7+ messages
- Optimized for the most common use cases

**Streamlined Button Layout**
- Clean left-right justification for better visual organization
- **Top row**: `[Defaults] [Reset All]` (left) | `[Add] [Close]` (right)
- **Bottom row**: `[List Dropdown] [Rename]` (left) | `[Help]` (right)
- Removed redundant "Save" button from title bar (messages auto-save now)

---

## 🔧 Technical Improvements

### Under the Hood
- Enhanced FlowPanel recalculation with `Invalidate()` calls
- Improved content area calculations for both windows
- Smarter dialog management with proper state tracking
- Optimized file I/O for immediate persistence

### Code Quality
- ~150 lines added for unsaved changes system
- ~40 lines for mode tooltip functionality
- Improved window sizing calculations
- Better separation of concerns

---

## 📊 Testing & Quality Assurance

All features have been extensively tested:
- ✅ Unsaved changes warnings (list switching, editor closing)
- ✅ Mode tooltip displays and updates correctly
- ✅ Popup dynamic sizing (0, 1-2, 10, 15, 16+ messages)
- ✅ Editor dynamic sizing (various message counts)
- ✅ Border symmetry and X button positioning
- ✅ Scrollbar behavior (appears/disappears properly)
- ✅ Button layout clean and organized
- ✅ All chat commands work in-game

---

## 🔄 Upgrading from v0.9.0

### Automatic - No Action Required!
- All existing settings are preserved
- Message lists remain intact
- Custom list names are unchanged
- Simply install the new version and go!

### What You'll Notice
1. Unsaved changes warnings when switching lists or closing editor
2. Mode information in the [LT] icon tooltip
3. Cleaner, more organized editor layout
4. Smoother popup window behavior

---

## 🎮 Compatibility

- **Blish HUD**: Requires ≥1.2.0
- **Guild Wars 2**: All game modes
- **Platform**: Windows
- **Save Data**: 100% compatible with v0.9.0

---

## 🐛 Bug Fixes

- Fixed scrollbar not updating when message list changes
- Fixed popup border asymmetry
- Fixed X button positioning issues
- Improved editor window sizing consistency

---

## 🚫 Breaking Changes

**None!** All changes are purely additive UX improvements. Existing configurations and message lists work perfectly.

---

## 📦 Installation

### Via Blish HUD Module Repository (Recommended)
1. Open Blish HUD
2. Go to Module Repo
3. Search for "LT Messages"
4. Click Install/Update

### Manual Installation
1. Download `LTMessages.bhm` from the release
2. Copy to `Documents\Guild Wars 2\addons\blishhud\modules\`
3. Restart Blish HUD or enable the module

---

## 🙏 Thank You

Special thanks to:
- The Guild Wars 2 community for feedback and testing
- Blish HUD developers for the amazing platform
- All commanders and LTs using this module!

---

## 📚 Resources

- **Wiki**: [github.com/senzal/LTMessages/wiki](https://github.com/senzal/LTMessages/wiki)
- **Issues**: [github.com/senzal/LTMessages/issues](https://github.com/senzal/LTMessages/issues)
- **Blish HUD**: [blishhud.com](https://blishhud.com/)
- **Discord**: [Blish HUD Discord](https://discord.gg/FYKN3qh)

---

## 🔮 What's Next?

Future features under consideration:
- Message reordering (drag-drop)
- Import/export message lists
- Message templates with variables
- Linux support (pending Blish documentation)

Have a feature request? [Open an issue on GitHub!](https://github.com/senzal/LTMessages/issues)

---

**Enjoy v0.9.1!** 🚀

*Built with ❤️ for the GW2 commanding community*
