using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LTMessages
{
    [Export(typeof(Module))]
    public class LTMessagesModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<LTMessagesModule>();

        // Module parameters
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;

        // Settings
        private SettingEntry<ChatFocus> _chatFocus;
        private SettingEntry<ChatAction> _chatAction;
        private SettingEntry<ChatCommand> _chatCommand;
        private SettingEntry<SendDelay> _sendDelay;
        private SettingEntry<bool> _showCornerIcon;
        private SettingEntry<KeyBinding> _popupKeybind;
        private SettingEntry<KeyBinding> _toggleLTModeKeybind;
        private SettingEntry<KeyBinding> _openEditorKeybind;
        private SettingEntry<bool> _ltModeEnabled;

        // Internal data (not exposed in settings)
        private int _activeMessageList = 0; // Which list (0-5) is active for sending messages
        private Dictionary<int, string> _listNames = new Dictionary<int, string>();

        // Enums for settings
        private enum ChatFocus
        {
            ShiftEnter,  // Opens squad chat (Shift+Enter)
            Enter        // Opens last used chat (Enter)
        }

        private enum ChatAction
        {
            PasteOnly,   // Copy to clipboard only
            Send         // Type and send the message
        }

        private enum ChatCommand
        {
            Default,     // Use whatever channel is already selected
            Squad,       // /squad
            Subgroup,    // /subgroup
            Party1,      // /1
            Party2,      // /2
            Party3,      // /3
            Party4,      // /4
            Party5,      // /5
            Guild,       // /guild
            Guild1,      // /g1
            Guild2,      // /g2
            Guild3,      // /g3
            Guild4,      // /g4
            Guild5,      // /g5
            Guild6,      // /g6
            Say,         // /say
            Map,         // /map
            Party,       // /party
            Team         // /team
        }

        private enum SendDelay
        {
            _5ms = 5,
            _8ms = 8,
            _10ms = 10,
            _12ms = 12,
            _15ms = 15,
            _20ms = 20,
            _25ms = 25,
            _30ms = 30,
            _35ms = 35,
            _40ms = 40,
            _50ms = 50,
            _60ms = 60,
            _75ms = 75,
            _100ms = 100,
            _125ms = 125,
            _150ms = 150
        }

        // UI Components
        private CornerIcon _cornerIcon;
        private Panel _popupWindow;
        private StandardButton _popupCloseButton;
        private FlowPanel _messageFlowPanel;
        private Panel _editorWindow;
        private FlowPanel _editorFlowPanel;
        private Dropdown _listSelectorDropdown;
        private Panel _editDialogWindow;
        private TextBox _editTitleTextBox;
        private TextBox _editMessageTextBox;
        private MessageEntry _editingMessage;
        private int _editingMessageIndex = -1;
        private bool _suppressListSwitchWarning = false; // Flag to suppress unsaved changes warning

        // Data
        private List<MessageEntry> _messages = new List<MessageEntry>();
        private FileSystemWatcher _fileWatcher;
        private int _currentListIndex = 0; // Which list (0-5) is currently loaded (for editing)

        // Constants
        private const int MessageListCount = 6; // Support 6 lists total (0-5)
        private static readonly string DefaultFilePathBase = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"Guild Wars 2\addons\blishhud\ltmessages\");
        private const int DefaultMaxMessageLength = 199;  // GW2 chat limit

        #region Windows API for Keyboard Input

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern ushort MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const ushort VK_RETURN = 0x0D;
        private const ushort VK_SHIFT = 0x10;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_V = 0x56;
        private const ushort VK_SLASH = 0xBF;  // Forward slash key

        private static void SendKeyPress(ushort keyCode, bool shift = false, bool ctrl = false)
        {
            List<INPUT> inputs = new List<INPUT>();

            // Press modifier keys first
            if (shift)
                inputs.Add(CreateKeyInput(VK_SHIFT, true));
            if (ctrl)
                inputs.Add(CreateKeyInput(VK_CONTROL, true));

            // Press main key
            inputs.Add(CreateKeyInput(keyCode, true));

            // Release main key
            inputs.Add(CreateKeyInput(keyCode, false));

            // Release modifier keys
            if (ctrl)
                inputs.Add(CreateKeyInput(VK_CONTROL, false));
            if (shift)
                inputs.Add(CreateKeyInput(VK_SHIFT, false));

            uint result = SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));

            if (result == 0)
            {
                Logger.Warn($"SendInput failed for keyCode {keyCode}, shift={shift}, ctrl={ctrl}");
            }
            else
            {
                Logger.Debug($"SendInput sent {result} inputs successfully");
            }
        }

        private static async Task TypeString(string text, int delayMs = 20)
        {
            foreach (char c in text)
            {
                // Get virtual key code and shift state for this character
                short vkAndShift = VkKeyScan(c);
                if (vkAndShift == -1)
                {
                    Logger.Warn($"Could not get virtual key for character: {c}");
                    continue;
                }

                byte vk = (byte)(vkAndShift & 0xFF);
                byte shiftState = (byte)((vkAndShift >> 8) & 0xFF);

                // Check if shift is needed (bit 0 of shift state)
                bool needShift = (shiftState & 1) != 0;

                SendKeyPress(vk, shift: needShift);
                await Task.Delay(delayMs);
            }
        }

        private static INPUT CreateKeyInput(ushort keyCode, bool keyDown)
        {
            // Get scan code from virtual key code (required for some games)
            ushort scanCode = (ushort)MapVirtualKey(keyCode, 0);

            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = scanCode,
                        dwFlags = keyDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        private static IntPtr FindGW2Window()
        {
            // Get the current foreground window
            IntPtr hwnd = GetForegroundWindow();

            // Check if it's GW2
            System.Text.StringBuilder title = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, title, title.Capacity);

            if (title.ToString().Contains("Guild Wars 2"))
            {
                return hwnd;
            }

            // If not, try to find it by checking if we're injected into GW2
            // Blish HUD runs inside GW2, so the game window should be findable
            return hwnd;
        }

        private static void FocusGameWindow()
        {
            try
            {
                IntPtr gameWindow = FindGW2Window();
                if (gameWindow != IntPtr.Zero)
                {
                    SetForegroundWindow(gameWindow);
                    Logger.Debug("Focused GW2 window");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to focus game window");
            }
        }

        #endregion

        [ImportingConstructor]
        public LTMessagesModule([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _popupKeybind = settings.DefineSetting(
                "PopupKeybind",
                new KeyBinding(),
                () => "Popup Keybind",
                () => "Press this key to show the message popup at your cursor (optional - no default binding)");

            _toggleLTModeKeybind = settings.DefineSetting(
                "ToggleLTModeKeybind",
                new KeyBinding(),
                () => "Toggle LT Mode Keybind",
                () => "Press this key to toggle LT Mode on/off (optional - no default binding)");

            _openEditorKeybind = settings.DefineSetting(
                "OpenEditorKeybind",
                new KeyBinding(),
                () => "Open Editor Keybind",
                () => "Press this key to open the message editor window (optional - no default binding)");

            _showCornerIcon = settings.DefineSetting(
                "ShowCornerIcon",
                true,
                () => "Show Corner Icon",
                () => "Display an icon in the Blish HUD menu for alternative access");

            _ltModeEnabled = settings.DefineSetting(
                "LTModeEnabled",
                true,
                () => "LT Mode Enabled",
                () => "Enable this when you are a Lieutenant or Commander. Messages won't send when disabled.");

            _chatFocus = settings.DefineSetting(
                "ChatFocus",
                ChatFocus.ShiftEnter,
                () => "Chat Focus",
                () => "Shift+Enter = Open squad chat directly | Enter = Open last used chat");

            _chatAction = settings.DefineSetting(
                "ChatAction",
                ChatAction.Send,
                () => "Chat Action",
                () => "Send = Type and send message automatically | Paste Only = Copy to clipboard");

            _chatCommand = settings.DefineSetting(
                "ChatCommand",
                ChatCommand.Default,
                () => "Chat Command",
                () => "Which chat channel to send to. Default = Use whatever channel is already active.");

            _sendDelay = settings.DefineSetting(
                "SendDelay",
                SendDelay._40ms,
                () => "Typing Delay (ms)",
                () => "Delay between keystrokes when typing messages (adjust if messages don't send reliably)");

            // Migrate old delay values that are no longer valid
            if (!Enum.IsDefined(typeof(SendDelay), _sendDelay.Value))
            {
                Logger.Info($"Migrating invalid send delay value to default (40ms)");
                _sendDelay.Value = SendDelay._40ms;
            }

            // Add button to open message editor
            var openEditorButton = settings.DefineSetting(
                "OpenEditorButton",
                false,
                () => "Open Message Editor",
                () => "Toggle this on to open the in-game message editor");

            openEditorButton.SettingChanged += (s, e) =>
            {
                if (e.NewValue && !e.PreviousValue)
                {
                    ShowEditorWindow();

                    // Reset button
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        openEditorButton.Value = false;
                    });
                }
            };

            // Add button to open help dialog
            var openHelpButton = settings.DefineSetting(
                "OpenHelpButton",
                false,
                () => "Show Help",
                () => "Toggle this on to see help and configuration guide");

            openHelpButton.SettingChanged += (s, e) =>
            {
                if (e.NewValue && !e.PreviousValue)
                {
                    ShowHelpDialog();

                    // Reset button
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        openHelpButton.Value = false;
                    });
                }
            };
        }

        protected override async Task LoadAsync()
        {
            Logger.Info("Loading LT Messages module...");

            // Load internal data (active list and custom names)
            LoadInternalData();

            // Load the active message list
            _currentListIndex = _activeMessageList;
            LoadMessagesFromFile();
            Logger.Info($"Loaded {GetListDisplayName(_currentListIndex)} with {_messages.Count} messages");

            // Setup file watcher for live reload
            SetupFileWatcher();

            // Create corner icon if enabled
            if (_showCornerIcon.Value)
            {
                CreateCornerIcon();
            }

            // Listen for corner icon setting changes
            _showCornerIcon.SettingChanged += OnShowCornerIconChanged;

            // Create the popup window (hidden initially)
            CreatePopupWindow();

            // Register keybinds
            _popupKeybind.Value.Enabled = true;
            _popupKeybind.Value.Activated += OnPopupKeybindActivated;

            _toggleLTModeKeybind.Value.Enabled = true;
            _toggleLTModeKeybind.Value.Activated += OnToggleLTModeKeybindActivated;

            _openEditorKeybind.Value.Enabled = true;
            _openEditorKeybind.Value.Activated += OnOpenEditorKeybindActivated;

            Logger.Info("LT Messages module loaded successfully.");

            await Task.CompletedTask;
        }

        protected override void Update(GameTime gameTime)
        {
            // Check for ESC key to close popup
            if (_popupWindow != null && _popupWindow.Visible)
            {
                if (GameService.Input.Keyboard.KeysDown.Contains(Keys.Escape))
                {
                    HidePopup();
                }
            }
        }

        protected override void Unload()
        {
            Logger.Info("Unloading LT Messages module...");

            // Save internal data before unloading
            SaveInternalData();

            // Unsubscribe from events
            _showCornerIcon.SettingChanged -= OnShowCornerIconChanged;

            if (_popupKeybind?.Value != null)
            {
                _popupKeybind.Value.Activated -= OnPopupKeybindActivated;
                _popupKeybind.Value.Enabled = false;
            }

            if (_toggleLTModeKeybind?.Value != null)
            {
                _toggleLTModeKeybind.Value.Activated -= OnToggleLTModeKeybindActivated;
                _toggleLTModeKeybind.Value.Enabled = false;
            }

            if (_openEditorKeybind?.Value != null)
            {
                _openEditorKeybind.Value.Activated -= OnOpenEditorKeybindActivated;
                _openEditorKeybind.Value.Enabled = false;
            }

            // Dispose file watcher
            _fileWatcher?.Dispose();

            // Unsubscribe from screen click
            if (GameService.Graphics.SpriteScreen != null)
            {
                GameService.Graphics.SpriteScreen.LeftMouseButtonPressed -= OnScreenClicked;
            }

            // Dispose UI
            _cornerIcon?.Dispose();
            _popupWindow?.Dispose();
            _editorWindow?.Dispose();
            _editDialogWindow?.Dispose();
            _renameDialogWindow?.Dispose();
            _helpDialogWindow?.Dispose();

            // Clear messages
            _messages.Clear();

            Logger.Info("LT Messages module unloaded.");
        }

        #region File Operations

        private List<MessageEntry> GetDefaultMessages()
        {
            // Embedded default messages - always available even if file doesn't exist
            return new List<MessageEntry>
            {
                // Pre-Movement & Positioning
                new MessageEntry("Stack", "Stack on Tag"),
                new MessageEntry("Wait", "Wait for the squad!"),
                new MessageEntry("Buffs", "Buffs dropped near tag"),
                new MessageEntry("Stealth", "Stack for stealth share"),
                new MessageEntry("Blast", "Blast the field for might/stealth"),

                // Movement Commands
                new MessageEntry("Moving", "Tag is moving"),
                new MessageEntry("Stop", "Stop! Hold position"),
                new MessageEntry("Port", "Port is on the marker"),
                new MessageEntry("Portal", "Portal is up at marker"),
                new MessageEntry("Unlock-WP", "Unlock the Waypoint!"),
                new MessageEntry("Take-WP", "Take the Waypoint in chat"),
                new MessageEntry("Link-WP", "Link the Waypoint in chat"),

                // Combat - Priority Actions
                new MessageEntry("Focus", "Focus the target"),
                new MessageEntry("Kill-Adds", "Kill the adds"),
                new MessageEntry("Spread", "Spread out!"),
                new MessageEntry("Dodge", "Dodge the AoE attacks!"),
                new MessageEntry("Rez", "Rez downed players!"),
                new MessageEntry("Mount-CC", "Springer or Warclaw up for CC"),
                new MessageEntry("Need-CC", "We need CC!"),
                new MessageEntry("Safe", "Area is clear - all safe"),

                // Objectives
                new MessageEntry("HP-Combat", "Please let Tag start the combat HP!"),
                new MessageEntry("HP-Commune", "Commune with HP and then stack on tag!"),
                new MessageEntry("F-Vista", "F the Vista and then stack on tag!"),
                new MessageEntry("POI-Tag", "Point of Interest on Tag!"),
                new MessageEntry("POI-Marker", "Point of Interest on Marker"),

                // Squad Management
                new MessageEntry("Guard", "We need 1-2 people to guard this spot"),
                new MessageEntry("Loot", "F for loot! Some chests need manual looting"),
                new MessageEntry("Help", "If you get lost ask for help!"),
                new MessageEntry("No-Drop", "Please don't drop items. Let Commander set up stations."),
                new MessageEntry("Break", "We are taking a short break. BRB")
            };
        }

        // Get a single sample message for lists 2-5
        private List<MessageEntry> GetSampleMessage()
        {
            return new List<MessageEntry>
            {
                new MessageEntry("Stack", "Stack on Tag")
            };
        }

        // Get file path for a specific list index (0-5)
        private string GetFilePathForList(int listIndex)
        {
            if (listIndex == 0)
            {
                // List 0 uses messages.txt for backwards compatibility
                return Path.Combine(DefaultFilePathBase, "messages.txt");
            }
            else
            {
                // Lists 1-5 use messages_1.txt through messages_5.txt
                return Path.Combine(DefaultFilePathBase, $"messages_{listIndex}.txt");
            }
        }

        // Load internal data (active list and custom names) from file
        private void LoadInternalData()
        {
            _listNames.Clear();

            string dataFile = Path.Combine(DefaultFilePathBase, "module_data.txt");

            try
            {
                if (File.Exists(dataFile))
                {
                    string[] lines = File.ReadAllLines(dataFile);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("ActiveList="))
                        {
                            if (int.TryParse(line.Substring(11), out int listIndex))
                            {
                                _activeMessageList = listIndex;
                            }
                        }
                        else if (line.StartsWith("ListName:"))
                        {
                            // Format: ListName:0:Name
                            string[] parts = line.Substring(9).Split(new[] { ':' }, 2);
                            if (parts.Length == 2 && int.TryParse(parts[0], out int index))
                            {
                                _listNames[index] = parts[1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load internal data");
            }

            // Fill in any missing default names
            for (int i = 0; i < MessageListCount; i++)
            {
                if (!_listNames.ContainsKey(i))
                {
                    _listNames[i] = (i == 0) ? "Default" : $"List {i}";
                }
            }

            // Ensure active list is valid
            if (_activeMessageList < 0 || _activeMessageList >= MessageListCount)
            {
                _activeMessageList = 0;
            }
        }

        // Save internal data to file
        private void SaveInternalData()
        {
            string dataFile = Path.Combine(DefaultFilePathBase, "module_data.txt");

            try
            {
                // Create directory if needed
                string directory = Path.GetDirectoryName(dataFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = new List<string>();
                lines.Add($"ActiveList={_activeMessageList}");

                foreach (var kvp in _listNames)
                {
                    lines.Add($"ListName:{kvp.Key}:{kvp.Value}");
                }

                File.WriteAllLines(dataFile, lines);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save internal data");
            }
        }

        // Get display name for a list
        private string GetListDisplayName(int listIndex)
        {
            if (_listNames.ContainsKey(listIndex))
            {
                return _listNames[listIndex];
            }
            return (listIndex == 0) ? "Default" : $"List {listIndex}";
        }

        // Get chat command string from enum
        private string GetChatCommandString(ChatCommand command)
        {
            switch (command)
            {
                case ChatCommand.Default: return "";
                case ChatCommand.Squad: return "/squad";
                case ChatCommand.Subgroup: return "/subgroup";
                case ChatCommand.Party1: return "/1";
                case ChatCommand.Party2: return "/2";
                case ChatCommand.Party3: return "/3";
                case ChatCommand.Party4: return "/4";
                case ChatCommand.Party5: return "/5";
                case ChatCommand.Guild: return "/guild";
                case ChatCommand.Guild1: return "/g1";
                case ChatCommand.Guild2: return "/g2";
                case ChatCommand.Guild3: return "/g3";
                case ChatCommand.Guild4: return "/g4";
                case ChatCommand.Guild5: return "/g5";
                case ChatCommand.Guild6: return "/g6";
                case ChatCommand.Say: return "/say";
                case ChatCommand.Map: return "/map";
                case ChatCommand.Party: return "/party";
                case ChatCommand.Team: return "/team";
                default: return "";
            }
        }

        private void LoadMessagesFromFile()
        {
            string filePath = GetFilePathForList(_currentListIndex);

            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Info($"Created directory: {directory}");
                }

                // Create default file if it doesn't exist
                if (!File.Exists(filePath))
                {
                    CreateDefaultMessageFile(filePath, _currentListIndex);
                    // Use appropriate defaults based on list index
                    _messages = (_currentListIndex == 0) ? GetDefaultMessages() : GetSampleMessage();
                    RefreshMessageUI();
                    return;
                }

                // Read and parse the file
                var lines = File.ReadAllLines(filePath);
                var newMessages = new List<MessageEntry>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue; // Skip empty lines and comments

                    var parts = line.Split(new[] { ',' }, 2);
                    if (parts.Length != 2)
                    {
                        Logger.Warn($"Malformed line in message file (expected 'Title,Message'): {line}");
                        continue;
                    }

                    string title = parts[0].Trim();
                    string message = parts[1].Trim();

                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                    {
                        Logger.Warn($"Skipping line with empty title or message: {line}");
                        continue;
                    }

                    // Enforce max message length (GW2 limit: 199 characters)
                    if (message.Length > DefaultMaxMessageLength)
                    {
                        message = message.Substring(0, DefaultMaxMessageLength);
                        Logger.Warn($"Message truncated to {DefaultMaxMessageLength} characters: {message}");
                    }

                    // MessageEntry will auto-truncate title if needed
                    var entry = new MessageEntry(title, message);

                    if (title.Length > 16)
                        Logger.Warn($"Title '{title}' truncated to 16 characters: '{entry.Title}'");

                    newMessages.Add(entry);
                }

                _messages = newMessages;
                Logger.Info($"Loaded {_messages.Count} messages from {filePath}");

                // Refresh UI
                RefreshMessageUI();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error loading messages from {filePath}");

                // Fall back to embedded defaults
                _messages = GetDefaultMessages();
                Logger.Info("Loaded embedded default messages as fallback");

                ScreenNotification.ShowNotification(
                    "LT Messages: Failed to load message file. Using default messages. Check logs for details.",
                    ScreenNotification.NotificationType.Warning);

                // Refresh UI with defaults
                RefreshMessageUI();
            }
        }

        private void CreateDefaultMessageFile(string filePath, int listIndex)
        {
            try
            {
                List<string> defaultMessages;

                if (listIndex == 0)
                {
                    // List 0 gets the full 30 default messages
                    defaultMessages = new List<string>
                    {
                        "# ========================================",
                        "# LT Messages Configuration File - List 0 (Default)",
                        "# ========================================",
                    "# ",
                    "# TO EDIT: Open this file in Notepad, VSCode, or any text editor",
                    "# FILE LOCATION: " + filePath,
                    "# ",
                    "# After editing, save the file - changes reload automatically!",
                    "# ",
                    "# FORMAT: Title,Message",
                    "#   Title: max 16 characters (shown in popup menu)",
                    "#   Message: max 199 characters (GW2 chat limit)",
                    "# ",
                    "# Lines starting with # are comments and ignored",
                    "# ========================================",
                    "",
                    "# Pre-Movement & Positioning",
                    "Stack,Stack on Tag",
                    "Wait,Wait for the squad!",
                    "Buffs,Buffs dropped near tag",
                    "Stealth,Stack for stealth share",
                    "Blast,Blast the field for might/stealth",
                    "",
                    "# Movement Commands",
                    "Moving,Tag is moving",
                    "Stop,Stop! Hold position",
                    "Port,Port is on the marker",
                    "Portal,Portal is up at marker",
                    "Unlock-WP,Unlock the Waypoint!",
                    "Take-WP,Take the Waypoint in chat",
                    "Link-WP,Link the Waypoint in chat",
                    "",
                    "# Combat - Priority Actions",
                    "Focus,Focus the target",
                    "Kill-Adds,Kill the adds",
                    "Spread,Spread out!",
                    "Dodge,Dodge the AoE attacks!",
                    "Rez,Rez downed players!",
                    "Mount-CC,Springer or Warclaw up for CC",
                    "Need-CC,We need CC!",
                    "Safe,Area is clear - all safe",
                    "",
                    "# Objectives",
                    "HP-Combat,Please let Tag start the combat HP!",
                    "HP-Commune,Commune with HP and then stack on tag!",
                    "F-Vista,F the Vista and then stack on tag!",
                    "POI-Tag,Point of Interest on Tag!",
                    "POI-Marker,Point of Interest on Marker",
                    "",
                    "# Squad Management",
                    "Guard,We need 1-2 people to guard this spot",
                    "Loot,F for loot! Some chests need manual looting",
                    "Help,If you get lost ask for help!",
                        "No-Drop,Please don't drop items. Let Commander set up stations.",
                        "Break,We are taking a short break. BRB"
                    };
                }
                else
                {
                    // Lists 1-5 get a single sample message
                    defaultMessages = new List<string>
                    {
                        "# ========================================",
                        $"# LT Messages Configuration File - List {listIndex}",
                        "# ========================================",
                        "# ",
                        "# TO EDIT: Open this file in Notepad, VSCode, or any text editor",
                        "# FILE LOCATION: " + filePath,
                        "# ",
                        "# Use this list for specific event types (WvW, Metas, HP Trains, etc.)",
                        "# Add your custom messages below.",
                        "# ",
                        "# FORMAT: Title,Message",
                        "#   Title: max 16 characters (shown in popup menu)",
                        "#   Message: max 199 characters (GW2 chat limit)",
                        "# ",
                        "# Lines starting with # are comments and ignored",
                        "# ========================================",
                        "",
                        "Stack,Stack on Tag"
                    };
                }

                File.WriteAllLines(filePath, defaultMessages);
                Logger.Info($"Created default message file at {filePath} (List {listIndex})");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to create default message file at {filePath}");
            }
        }

        private void SetupFileWatcher()
        {
            try
            {
                // Watch the currently active list file
                string filePath = GetFilePathForList(_activeMessageList);
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                    return;

                _fileWatcher?.Dispose();

                _fileWatcher = new FileSystemWatcher
                {
                    Path = directory,
                    Filter = fileName,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;

                Logger.Info($"File watcher setup for {filePath} (List {_activeMessageList})");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to setup file watcher");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce file changes (file can trigger multiple events)
            Thread.Sleep(100);
            Logger.Info($"Message file changed for List {_activeMessageList}, reloading...");

            // If the changed file is the active list, reload it
            if (_activeMessageList == _currentListIndex)
            {
                LoadMessagesFromFile();
            }
        }

        #endregion

        #region UI Creation

        private void CreateCornerIcon()
        {
            try
            {
                // Use a built-in texture or load from ref folder
                var iconTexture = ContentsManager.GetTexture("icon.png")
                    ?? GameService.Content.DatAssetCache.GetTextureFromAssetId(156027); // Generic icon

                _cornerIcon = new CornerIcon
                {
                    Icon = iconTexture,
                    BasicTooltipText = "LT Messages - Click to show messages",
                    Priority = 1645843599, // Random constant
                    Parent = GameService.Graphics.SpriteScreen
                };

                _cornerIcon.Click += OnCornerIconClicked;
                _cornerIcon.RightMouseButtonPressed += OnCornerIconRightClicked;

                // Update tooltip based on LT mode and chat settings
                UpdateCornerIconTooltip();
                _ltModeEnabled.SettingChanged += (s, e) => UpdateCornerIconTooltip();
                _chatFocus.SettingChanged += (s, e) => UpdateCornerIconTooltip();
                _chatAction.SettingChanged += (s, e) => UpdateCornerIconTooltip();
                _chatCommand.SettingChanged += (s, e) => UpdateCornerIconTooltip();

                Logger.Info("Corner icon created");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create corner icon");
            }
        }

        private void UpdateCornerIconTooltip()
        {
            if (_cornerIcon != null)
            {
                string status = _ltModeEnabled.Value ? "ENABLED" : "DISABLED";

                // Get friendly names for mode settings
                string focus = _chatFocus.Value == ChatFocus.ShiftEnter ? "Shift+Enter" : "Enter";
                string action = _chatAction.Value == ChatAction.Send ? "Send" : "Paste";
                string command = GetChatCommandDisplayName(_chatCommand.Value);

                _cornerIcon.BasicTooltipText = $"LT Messages\nLeft-click: Show messages\nRight-click: Open editor\nLT Mode: {status}\nMode: {focus} | {action} | {command}";
            }
        }

        private string GetChatCommandDisplayName(ChatCommand command)
        {
            switch (command)
            {
                case ChatCommand.Default: return "Default";
                case ChatCommand.Squad: return "Squad";
                case ChatCommand.Subgroup: return "Subgroup";
                case ChatCommand.Map: return "Map";
                case ChatCommand.Say: return "Say";
                case ChatCommand.Party: return "Party";
                case ChatCommand.Team: return "Team";
                case ChatCommand.Guild: return "Guild";
                case ChatCommand.Guild1: return "Guild1";
                case ChatCommand.Guild2: return "Guild2";
                case ChatCommand.Guild3: return "Guild3";
                case ChatCommand.Guild4: return "Guild4";
                case ChatCommand.Guild5: return "Guild5";
                case ChatCommand.Guild6: return "Guild6";
                case ChatCommand.Party1: return "Party1";
                case ChatCommand.Party2: return "Party2";
                case ChatCommand.Party3: return "Party3";
                case ChatCommand.Party4: return "Party4";
                case ChatCommand.Party5: return "Party5";
                default: return command.ToString();
            }
        }

        private void CreatePopupWindow()
        {
            _popupWindow = new Panel
            {
                Size = new Point(220, 300),
                Location = new Point(100, 100),
                Visible = false,
                ZIndex = 9999,
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 220),  // GW2-style dark brown
                ShowBorder = true
            };

            // Add header label
            new Label
            {
                Text = "Messages",
                Font = GameService.Content.DefaultFont16,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 8),
                TextColor = new Color(220, 200, 150, 255),  // GW2 gold color
                ShowShadow = true,
                Parent = _popupWindow
            };

            // Add close button in top-right corner
            _popupCloseButton = new StandardButton
            {
                Text = "X",
                Width = 25,
                Height = 25,
                Location = new Point(_popupWindow.Width - 30, 5),
                Parent = _popupWindow,
                ZIndex = 10
            };
            _popupCloseButton.Click += (s, e) => HidePopup();

            _messageFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
                Location = new Point(5, 35),
                Size = new Point(210, 260),
                Parent = _popupWindow,
                OuterControlPadding = new Vector2(8, 8),
                ControlPadding = new Vector2(0, 2)
            };

            // Add click handler to screen to close popup when clicking outside
            GameService.Graphics.SpriteScreen.LeftMouseButtonPressed += OnScreenClicked;

            // Populate with messages
            RefreshMessageUI();
        }

        private void RefreshMessageUI()
        {
            // Update popup window
            if (_messageFlowPanel != null)
            {
                _messageFlowPanel.ClearChildren();

                foreach (var message in _messages)
                {
                    // Create a simple label for each message (no container panel)
                    var label = new Label
                    {
                        Text = message.Title,
                        Width = 200,
                        Height = 26,
                        TextColor = new Color(220, 200, 150, 255),  // GW2 gold color
                        Font = GameService.Content.DefaultFont16,
                        ShowShadow = true,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Middle,
                        AutoSizeHeight = false,
                        AutoSizeWidth = false,
                        WrapText = false,
                        BackgroundColor = new Color(40, 35, 30, 180),
                        Parent = _messageFlowPanel
                    };

                    // Capture message in closure
                    var capturedMessage = message;

                    // Make label clickable
                    label.Click += (s, e) =>
                    {
                        OnMessageSelected(capturedMessage);
                        HidePopup();
                    };

                    // Hover effects
                    label.MouseEntered += (s, e) =>
                    {
                        label.BackgroundColor = new Color(60, 50, 40, 200);
                        label.TextColor = Color.Yellow;
                    };

                    label.MouseLeft += (s, e) =>
                    {
                        label.BackgroundColor = new Color(40, 35, 30, 180);
                        label.TextColor = new Color(220, 200, 150, 255);
                    };
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnShowCornerIconChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue && _cornerIcon == null)
            {
                CreateCornerIcon();
            }
            else if (!e.NewValue && _cornerIcon != null)
            {
                _cornerIcon.Dispose();
                _cornerIcon = null;
            }
        }

        private void OnPopupKeybindActivated(object sender, EventArgs e)
        {
            ShowPopupAtCursor();
        }

        private void OnCornerIconClicked(object sender, MouseEventArgs e)
        {
            // Show popup at a fixed position near the corner icon
            ShowPopupAtCursor();
        }

        private void OnCornerIconRightClicked(object sender, MouseEventArgs e)
        {
            // Open message editor
            ShowEditorWindow();
            Logger.Info("Editor window opened via corner icon right-click");
        }

        private void OnToggleLTModeKeybindActivated(object sender, EventArgs e)
        {
            // Toggle LT Mode
            _ltModeEnabled.Value = !_ltModeEnabled.Value;

            string status = _ltModeEnabled.Value ? "ENABLED" : "DISABLED";
            ScreenNotification.ShowNotification(
                $"LT Messages: LT Mode {status}",
                ScreenNotification.NotificationType.Info);

            Logger.Info($"LT Mode toggled via keybind: {status}");
        }

        private void OnOpenEditorKeybindActivated(object sender, EventArgs e)
        {
            ShowEditorWindow();
            Logger.Info("Editor window opened via keybind");
        }

        private void OnScreenClicked(object sender, MouseEventArgs e)
        {
            // Close popup if clicking outside of it
            if (_popupWindow != null && _popupWindow.Visible)
            {
                var mousePos = GameService.Input.Mouse.Position;
                var popupBounds = _popupWindow.AbsoluteBounds;

                // Check if click is outside the popup
                if (!popupBounds.Contains(mousePos))
                {
                    HidePopup();
                }
            }
        }

        #endregion

        #region Popup Management

        private void ShowPopupAtCursor()
        {
            if (_popupWindow == null || _messages.Count == 0)
                return;

            try
            {
                // Get mouse position
                var mousePos = GameService.Input.Mouse.Position;

                // Adjust window size based on content (add 40 for close button area at top)
                int itemHeight = 25;
                int windowHeight = Math.Min(_messages.Count * itemHeight + 60, 440);
                _popupWindow.Size = new Point(200, windowHeight);

                // Update close button position to match new window width
                if (_popupCloseButton != null)
                {
                    _popupCloseButton.Location = new Point(_popupWindow.Width - 30, 5);
                }

                // Position at cursor, but keep on screen
                int x = mousePos.X;
                int y = mousePos.Y;

                // Adjust if near screen edges
                if (x + _popupWindow.Width > GameService.Graphics.SpriteScreen.Width)
                    x = GameService.Graphics.SpriteScreen.Width - _popupWindow.Width;
                if (y + _popupWindow.Height > GameService.Graphics.SpriteScreen.Height)
                    y = GameService.Graphics.SpriteScreen.Height - _popupWindow.Height;

                if (x < 0) x = 0;
                if (y < 0) y = 0;

                _popupWindow.Location = new Point(x, y);
                _popupWindow.Show();

                Logger.Debug($"Showing popup at {x}, {y}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show popup at cursor");
            }
        }

        private void HidePopup()
        {
            if (_popupWindow != null)
            {
                _popupWindow.Hide();
            }
        }

        #endregion

        #region Message Editor

        private void ShowEditorWindow()
        {
            if (_editorWindow == null)
            {
                CreateEditorWindow();
            }

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - _editorWindow.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - _editorWindow.Height) / 2;
            _editorWindow.Location = new Point(x, y);

            _editorWindow.Show();
            RefreshEditorUI();
        }

        private void CreateEditorWindow()
        {
            _editorWindow = new Panel
            {
                Size = new Point(500, 400),
                ZIndex = 10000,
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 240),
                ShowBorder = true,
                CanScroll = false
            };

            // Title
            new Label
            {
                Text = "LT Messages Editor",
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                ShowShadow = true,
                Parent = _editorWindow
            };

            // List selector dropdown
            _listSelectorDropdown = new Dropdown
            {
                Location = new Point(10, 40),
                Width = 150,
                Parent = _editorWindow
            };

            // Populate dropdown with list options (0-5)
            for (int i = 0; i < MessageListCount; i++)
            {
                _listSelectorDropdown.Items.Add(GetListDisplayName(i));
            }
            _listSelectorDropdown.SelectedItem = GetListDisplayName(_currentListIndex);

            // Handle dropdown selection change
            _listSelectorDropdown.ValueChanged += (s, e) =>
            {
                // Check if there are unsaved changes
                if (!_suppressListSwitchWarning && _editDialogWindow != null && _editDialogWindow.Visible)
                {
                    // Save the target list selection
                    string targetList = _listSelectorDropdown.SelectedItem;

                    // Revert dropdown to current list (will be changed if user confirms)
                    _suppressListSwitchWarning = true;
                    _listSelectorDropdown.SelectedItem = GetListDisplayName(_currentListIndex);
                    _suppressListSwitchWarning = false;

                    // Show confirmation dialog
                    ShowUnsavedChangesDialog(targetList);
                    return;
                }

                // Find which list index matches the selected name
                string selected = _listSelectorDropdown.SelectedItem;
                for (int i = 0; i < MessageListCount; i++)
                {
                    if (GetListDisplayName(i) == selected)
                    {
                        _currentListIndex = i;
                        Logger.Info($"Switched to editing {selected} (index {i})");
                        LoadMessagesFromFile();
                        RefreshEditorUI();
                        break;
                    }
                }
            };

            // All buttons same size (70px) for consistency

            // Top row: Left: [Defaults] [Reset All]  Right: [Add] [Close]
            // Defaults button (left side)
            var defaultButton = new StandardButton
            {
                Text = "Defaults",
                Width = 70,
                Location = new Point(170, 8),
                Parent = _editorWindow
            };
            defaultButton.Click += (s, e) => RestoreDefaultMessages();

            // Reset All button (left side)
            var resetButton = new StandardButton
            {
                Text = "Reset All",
                Width = 70,
                Location = new Point(250, 8),
                Parent = _editorWindow
            };
            resetButton.Click += (s, e) => ResetAllListsToDefaults();

            // Add button (right side)
            var addButton = new StandardButton
            {
                Text = "Add",
                Width = 70,
                Location = new Point(340, 8),
                Parent = _editorWindow
            };
            addButton.Click += (s, e) => ShowEditDialog(-1, null);

            // Close button (right side)
            var closeButton = new StandardButton
            {
                Text = "Close",
                Width = 70,
                Location = new Point(420, 8),
                Parent = _editorWindow
            };
            closeButton.Click += (s, e) => CloseEditor();

            // Bottom row: [Dropdown] [Rename]  (right side) [Help]
            // Rename button (next to dropdown)
            var renameButton = new StandardButton
            {
                Text = "Rename",
                Width = 70,
                Location = new Point(170, 40),
                Parent = _editorWindow
            };
            renameButton.Click += (s, e) => ShowRenameListDialog();

            // Help button (under Close, right-aligned)
            var helpButton = new StandardButton
            {
                Text = "Help",
                Width = 70,
                Location = new Point(420, 40),
                Parent = _editorWindow
            };
            helpButton.Click += (s, e) => ShowHelpDialog();

            // Message list panel (below dropdown)
            _editorFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
                Location = new Point(10, 70),
                Size = new Point(480, 315),
                Parent = _editorWindow,
                OuterControlPadding = new Vector2(5, 5),
                ControlPadding = new Vector2(0, 3)
            };
        }

        private void RefreshEditorUI()
        {
            if (_editorFlowPanel == null) return;

            _editorFlowPanel.ClearChildren();

            for (int i = 0; i < _messages.Count; i++)
            {
                var message = _messages[i];
                var index = i; // Capture for closure

                // Container panel for each message
                var itemPanel = new Panel
                {
                    Width = 460,
                    Height = 60,
                    BackgroundColor = new Color(40, 35, 30, 180),
                    ShowBorder = true,
                    Parent = _editorFlowPanel
                };

                // Title label
                new Label
                {
                    Text = $"Title: {message.Title}",
                    Width = 300,
                    Location = new Point(5, 5),
                    TextColor = new Color(220, 200, 150, 255),
                    Font = GameService.Content.DefaultFont14,
                    Parent = itemPanel
                };

                // Message label
                new Label
                {
                    Text = $"Message: {message.Message}",
                    Width = 300,
                    Location = new Point(5, 25),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont12,
                    Parent = itemPanel
                };

                // Edit button
                var editButton = new StandardButton
                {
                    Text = "Edit",
                    Width = 60,
                    Location = new Point(320, 10),
                    Parent = itemPanel
                };
                editButton.Click += (s, e) => ShowEditDialog(index, message);

                // Delete button
                var deleteButton = new StandardButton
                {
                    Text = "Delete",
                    Width = 60,
                    Location = new Point(390, 10),
                    Parent = itemPanel
                };
                deleteButton.Click += (s, e) => DeleteMessage(index);
            }
        }

        private void ShowEditDialog(int messageIndex, MessageEntry message)
        {
            _editingMessageIndex = messageIndex;
            _editingMessage = message;

            if (_editDialogWindow == null)
            {
                CreateEditDialog();
            }

            // Set values
            if (message != null)
            {
                _editTitleTextBox.Text = message.Title;
                _editMessageTextBox.Text = message.Message;
            }
            else
            {
                _editTitleTextBox.Text = "";
                _editMessageTextBox.Text = "";
            }

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - _editDialogWindow.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - _editDialogWindow.Height) / 2;
            _editDialogWindow.Location = new Point(x, y);

            _editDialogWindow.Show();
        }

        private void CreateEditDialog()
        {
            _editDialogWindow = new Panel
            {
                Size = new Point(400, 250),
                ZIndex = 10001,
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 250),
                ShowBorder = true
            };

            // Title
            new Label
            {
                Text = "Edit Message",
                Font = GameService.Content.DefaultFont16,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                Parent = _editDialogWindow
            };

            // Title label
            new Label
            {
                Text = "Title (max 16 chars):",
                Location = new Point(10, 45),
                Width = 200,
                TextColor = Color.White,
                Parent = _editDialogWindow
            };

            // Title textbox
            _editTitleTextBox = new TextBox
            {
                Location = new Point(10, 65),
                Width = 380,
                MaxLength = 16,
                Parent = _editDialogWindow
            };

            // Message label
            new Label
            {
                Text = "Message (max 199 chars):",
                Location = new Point(10, 100),
                Width = 200,
                TextColor = Color.White,
                Parent = _editDialogWindow
            };

            // Message textbox
            _editMessageTextBox = new TextBox
            {
                Location = new Point(10, 120),
                Width = 380,
                MaxLength = 199,
                Parent = _editDialogWindow
            };

            // Character counter for message
            var charCountLabel = new Label
            {
                Text = "0 / 199",
                Location = new Point(10, 145),
                Width = 100,
                TextColor = Color.Gray,
                Font = GameService.Content.DefaultFont12,
                Parent = _editDialogWindow
            };

            _editMessageTextBox.TextChanged += (s, e) =>
            {
                charCountLabel.Text = $"{_editMessageTextBox.Text.Length} / 199";
            };

            // Save button
            var saveButton = new StandardButton
            {
                Text = "Save",
                Width = 80,
                Location = new Point(220, 200),
                Parent = _editDialogWindow
            };
            saveButton.Click += (s, e) => SaveEditedMessage();

            // Cancel button
            var cancelButton = new StandardButton
            {
                Text = "Cancel",
                Width = 80,
                Location = new Point(310, 200),
                Parent = _editDialogWindow
            };
            cancelButton.Click += (s, e) => _editDialogWindow.Hide();
        }

        private void SaveEditedMessage()
        {
            string title = _editTitleTextBox.Text.Trim();
            string messageText = _editMessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(messageText))
            {
                ScreenNotification.ShowNotification(
                    "LT Messages: Title and message cannot be empty",
                    ScreenNotification.NotificationType.Error);
                return;
            }

            var newEntry = new MessageEntry(title, messageText);

            if (_editingMessageIndex >= 0)
            {
                // Update existing message
                _messages[_editingMessageIndex] = newEntry;
            }
            else
            {
                // Add new message
                _messages.Add(newEntry);
            }

            _editDialogWindow.Hide();
            RefreshEditorUI();
            RefreshMessageUI();
            SaveMessagesToFile(); // Auto-save to file when message is saved
        }

        private void DeleteMessage(int index)
        {
            if (index >= 0 && index < _messages.Count)
            {
                _messages.RemoveAt(index);
                RefreshEditorUI();
                RefreshMessageUI();
            }
        }

        private void ShowUnsavedChangesDialog(string targetListName)
        {
            // Create confirmation dialog
            var confirmDialog = new Panel
            {
                Size = new Point(450, 180),
                ZIndex = 10002, // Above edit dialog
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 250),
                ShowBorder = true
            };

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - confirmDialog.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - confirmDialog.Height) / 2;
            confirmDialog.Location = new Point(x, y);

            // Title
            new Label
            {
                Text = "Unsaved Changes",
                Font = GameService.Content.DefaultFont16,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                Parent = confirmDialog
            };

            // Message
            new Label
            {
                Text = "You have unsaved changes in the message editor.\nWhat would you like to do?",
                Location = new Point(10, 45),
                Width = 430,
                Height = 50,
                TextColor = Color.White,
                WrapText = true,
                Parent = confirmDialog
            };

            // Save and Switch button
            var saveButton = new StandardButton
            {
                Text = "Save & Switch",
                Width = 130,
                Location = new Point(10, 130),
                Parent = confirmDialog
            };
            saveButton.Click += (s, e) =>
            {
                SaveEditedMessage();
                confirmDialog.Dispose();
                SwitchToList(targetListName);
            };

            // Discard and Switch button
            var discardButton = new StandardButton
            {
                Text = "Discard & Switch",
                Width = 130,
                Location = new Point(150, 130),
                Parent = confirmDialog
            };
            discardButton.Click += (s, e) =>
            {
                _editDialogWindow.Hide();
                confirmDialog.Dispose();
                SwitchToList(targetListName);
            };

            // Cancel button
            var cancelButton = new StandardButton
            {
                Text = "Cancel",
                Width = 130,
                Location = new Point(290, 130),
                Parent = confirmDialog
            };
            cancelButton.Click += (s, e) =>
            {
                confirmDialog.Dispose();
                // Stay on current list - dropdown already reverted
            };

            confirmDialog.Show();
        }

        private void SwitchToList(string listName)
        {
            // Update dropdown selection and trigger list switch
            _suppressListSwitchWarning = true;
            _listSelectorDropdown.SelectedItem = listName;
            _suppressListSwitchWarning = false;

            // Find which list index matches the selected name
            for (int i = 0; i < MessageListCount; i++)
            {
                if (GetListDisplayName(i) == listName)
                {
                    _currentListIndex = i;
                    Logger.Info($"Switched to editing {listName} (index {i})");
                    LoadMessagesFromFile();
                    RefreshEditorUI();
                    break;
                }
            }
        }

        private void CloseEditor()
        {
            // Check if there are unsaved changes in the edit dialog
            if (_editDialogWindow != null && _editDialogWindow.Visible)
            {
                // Show confirmation dialog for closing editor with unsaved changes
                ShowCloseEditorConfirmation();
            }
            else
            {
                // No unsaved changes, close normally
                _editorWindow.Hide();
            }
        }

        private void ShowCloseEditorConfirmation()
        {
            // Create confirmation dialog
            var confirmDialog = new Panel
            {
                Size = new Point(450, 180),
                ZIndex = 10002, // Above edit dialog
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 250),
                ShowBorder = true
            };

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - confirmDialog.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - confirmDialog.Height) / 2;
            confirmDialog.Location = new Point(x, y);

            // Title
            new Label
            {
                Text = "Unsaved Changes",
                Font = GameService.Content.DefaultFont16,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                Parent = confirmDialog
            };

            // Message
            new Label
            {
                Text = "You have unsaved changes in the message editor.\nWhat would you like to do?",
                Location = new Point(10, 45),
                Width = 430,
                Height = 50,
                TextColor = Color.White,
                WrapText = true,
                Parent = confirmDialog
            };

            // Save and Close button
            var saveButton = new StandardButton
            {
                Text = "Save & Close",
                Width = 130,
                Location = new Point(10, 130),
                Parent = confirmDialog
            };
            saveButton.Click += (s, e) =>
            {
                SaveEditedMessage();
                confirmDialog.Dispose();
                _editorWindow.Hide();
            };

            // Discard and Close button
            var discardButton = new StandardButton
            {
                Text = "Discard & Close",
                Width = 130,
                Location = new Point(150, 130),
                Parent = confirmDialog
            };
            discardButton.Click += (s, e) =>
            {
                _editDialogWindow.Hide();
                confirmDialog.Dispose();
                _editorWindow.Hide();
            };

            // Cancel button
            var cancelButton = new StandardButton
            {
                Text = "Cancel",
                Width = 130,
                Location = new Point(290, 130),
                Parent = confirmDialog
            };
            cancelButton.Click += (s, e) =>
            {
                confirmDialog.Dispose();
                // Stay in editor - don't close
            };

            confirmDialog.Show();
        }

        private TextBox _renameTextBox;
        private Panel _renameDialogWindow;

        private void ShowRenameListDialog()
        {
            if (_renameDialogWindow == null)
            {
                CreateRenameDialog();
            }

            // Set current name
            _renameTextBox.Text = GetListDisplayName(_currentListIndex);

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - _renameDialogWindow.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - _renameDialogWindow.Height) / 2;
            _renameDialogWindow.Location = new Point(x, y);

            _renameDialogWindow.Show();
            _renameTextBox.Focused = true;
        }

        private void CreateRenameDialog()
        {
            _renameDialogWindow = new Panel
            {
                Size = new Point(350, 150),
                ZIndex = 10002,
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 250),
                ShowBorder = true
            };

            // Title
            new Label
            {
                Text = $"Rename {GetListDisplayName(_currentListIndex)}",
                Font = GameService.Content.DefaultFont16,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                Parent = _renameDialogWindow
            };

            // Name label
            new Label
            {
                Text = "List Name (max 30 chars):",
                Location = new Point(10, 45),
                Width = 200,
                TextColor = Color.White,
                Parent = _renameDialogWindow
            };

            // Name textbox
            _renameTextBox = new TextBox
            {
                Location = new Point(10, 65),
                Width = 330,
                MaxLength = 30,
                Parent = _renameDialogWindow
            };

            // Save button
            var saveButton = new StandardButton
            {
                Text = "Save",
                Width = 80,
                Location = new Point(170, 100),
                Parent = _renameDialogWindow
            };
            saveButton.Click += (s, e) => SaveListRename();

            // Cancel button
            var cancelButton = new StandardButton
            {
                Text = "Cancel",
                Width = 80,
                Location = new Point(260, 100),
                Parent = _renameDialogWindow
            };
            cancelButton.Click += (s, e) => _renameDialogWindow.Hide();
        }

        private void SaveListRename()
        {
            string newName = _renameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                ScreenNotification.ShowNotification(
                    "LT Messages: List name cannot be empty",
                    ScreenNotification.NotificationType.Error);
                return;
            }

            // Update the list name
            _listNames[_currentListIndex] = newName;
            SaveInternalData();

            // Update dropdown to show new name
            _listSelectorDropdown.Items.Clear();
            for (int i = 0; i < MessageListCount; i++)
            {
                _listSelectorDropdown.Items.Add(GetListDisplayName(i));
            }
            _listSelectorDropdown.SelectedItem = GetListDisplayName(_currentListIndex);

            _renameDialogWindow.Hide();

            ScreenNotification.ShowNotification(
                $"LT Messages: List renamed to '{newName}'",
                ScreenNotification.NotificationType.Info);
            Logger.Info($"Renamed list {_currentListIndex} to '{newName}'");
        }

        private Panel _helpDialogWindow;

        private void ShowHelpDialog()
        {
            if (_helpDialogWindow == null)
            {
                CreateHelpDialog();
            }

            // Center on screen
            int x = (GameService.Graphics.SpriteScreen.Width - _helpDialogWindow.Width) / 2;
            int y = (GameService.Graphics.SpriteScreen.Height - _helpDialogWindow.Height) / 2;
            _helpDialogWindow.Location = new Point(x, y);

            _helpDialogWindow.Show();
        }

        private void CreateHelpDialog()
        {
            _helpDialogWindow = new Panel
            {
                Size = new Point(600, 500),
                ZIndex = 10003,
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(25, 20, 15, 250),
                ShowBorder = true,
                CanScroll = false
            };

            // Title
            new Label
            {
                Text = "LT Messages - Help",
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 10),
                TextColor = new Color(220, 200, 150, 255),
                Parent = _helpDialogWindow
            };

            // Close button (top right, next to title)
            var closeButton = new StandardButton
            {
                Text = "Close",
                Width = 80,
                Location = new Point(510, 8),
                Parent = _helpDialogWindow
            };
            closeButton.Click += (s, e) => _helpDialogWindow.Hide();

            // Scrollable content panel
            var contentPanel = new Panel
            {
                Location = new Point(10, 45),
                Size = new Point(580, 445),
                Parent = _helpDialogWindow,
                CanScroll = true,
                ShowBorder = false
            };

            // Help content
            var helpText = @"QUICK START
• Left-click [LT] icon to see your messages
• Click a message to send it
• Right-click [LT] icon to open this editor

MODULE SETTINGS
All operational settings are found in:
Blish HUD → Manage Modules → LT Messages

CHAT SETTINGS

Chat Focus - How to open chat:
• Shift+Enter: Opens squad chat directly
• Enter: Opens last used chat (map, say, etc.)

Chat Action - What to do with message:
• Send: Automatically types and sends
• Paste Only: Copies to clipboard (Ctrl+V to paste)

Chat Command - Which channel to send to:
• Default: Uses your current active channel
• /squad: Squad broadcast
• /map: Map chat
• /party: Party chat
• /guild: Guild chat
• /say: Local say
• /1 through /5: Whisper to party members
• /g1 through /g6: Guild channels 1-6
• And more!

COMMON CONFIGURATIONS

For Squad Broadcast:
• Chat Focus: Shift+Enter
• Chat Action: Send
• Chat Command: Default (or /squad)

For Map Chat:
• Chat Focus: Enter
• Chat Action: Send
• Chat Command: /map

For Manual Control (Clipboard):
• Chat Action: Paste Only
• (Chat Focus and Command don't matter)

MESSAGE LISTS

• Use 6 different lists for different events
• Click dropdown to switch lists
• Click ""Rename"" to give lists custom names
  (e.g., ""WvW"", ""Metas"", ""HP Trains"")

EDITING MESSAGES

• Add: Create new message
• Edit: Modify existing message (click in list)
• Delete: Remove message (click Delete on message)
• Save: Saves to file automatically
• Defaults: Restore 30 default commander messages

TYPING DELAY

• Adjust if messages don't send reliably
• Lower = faster typing (5-40ms recommended)
• Higher = more reliable on slow systems
• Default: 40ms works for most people

TROUBLESHOOTING

Messages not sending?
• Check that LT Mode is enabled
• Set Chat Action to ""Send""
• Try higher Typing Delay
• Verify Chat Command is correct

Need more help?
• GitHub: github.com/senzal/LTMessages
• Blish Discord: discord.gg/FYKN3qh

---

THANK YOU!

Thanks to the amazing Guild Wars 2 community
and the Blish HUD community for using and
creating incredible plugins that make our
gaming experience even better!

Your support, feedback, and contributions
help make tools like this possible.

Happy commanding! ♥

---

For full documentation, visit the wiki:
https://github.com/senzal/LTMessages/wiki";

            var helpLabel = new Label
            {
                Text = helpText,
                Location = new Point(5, 5),
                Width = 555,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont14,
                Parent = contentPanel,
                WrapText = true,
                AutoSizeHeight = true
            };
        }

        private void SaveMessagesToFile()
        {
            try
            {
                string filePath = GetFilePathForList(_currentListIndex);

                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = new List<string>
                {
                    "# ========================================",
                    $"# LT Messages Configuration File - List {_currentListIndex}",
                    "# ========================================",
                    "# ",
                    "# Format: Title,Message",
                    "# Title: max 16 characters (shown in popup menu)",
                    "# Message: max 200 characters (default GW2 chat limit)",
                    "# ",
                    "# Lines starting with # are comments",
                    "# ========================================",
                    ""
                };

                foreach (var message in _messages)
                {
                    lines.Add($"{message.Title},{message.Message}");
                }

                File.WriteAllLines(filePath, lines);

                Logger.Info($"Saved {_messages.Count} messages to {filePath} ({GetListDisplayName(_currentListIndex)})");

                ScreenNotification.ShowNotification(
                    $"LT Messages: Saved {_messages.Count} messages to {GetListDisplayName(_currentListIndex)}",
                    ScreenNotification.NotificationType.Info);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save messages to file");
                ScreenNotification.ShowNotification(
                    "LT Messages: Failed to save messages to file",
                    ScreenNotification.NotificationType.Error);
            }
        }

        private void RestoreDefaultMessages()
        {
            try
            {
                // Confirm with user first
                var confirmDialog = new Panel
                {
                    Size = new Point(450, 220),
                    Location = new Point((GameService.Graphics.SpriteScreen.Width - 450) / 2, (GameService.Graphics.SpriteScreen.Height - 220) / 2),
                    ZIndex = 15000,
                    Parent = GameService.Graphics.SpriteScreen,
                    BackgroundColor = new Color(25, 20, 15, 250),
                    ShowBorder = true
                };

                new Label
                {
                    Text = $"⚠ Load Defaults for {GetListDisplayName(_currentListIndex)}?",
                    Font = GameService.Content.DefaultFont18,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Location = new Point(20, 20),
                    TextColor = new Color(255, 200, 0, 255), // Orange warning
                    ShowShadow = true,
                    Parent = confirmDialog
                };

                string messageText = (_currentListIndex == 0)
                    ? $"WARNING: This will replace ALL messages in {GetListDisplayName(_currentListIndex)}\nwith the 30 default messages.\n\nYour current {_messages.Count} message(s) will be lost!\n\nThis action CANNOT be undone."
                    : $"WARNING: This will replace ALL messages in {GetListDisplayName(_currentListIndex)}\nwith one sample message: 'Stack on Tag'.\n\nYour current {_messages.Count} message(s) will be lost!\n\nThis action CANNOT be undone.";

                new Label
                {
                    Text = messageText,
                    Width = 410,
                    Height = 100,
                    Location = new Point(20, 60),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont14,
                    Parent = confirmDialog
                };

                var yesButton = new StandardButton
                {
                    Text = "Yes, Load Defaults",
                    Width = 140,
                    Location = new Point(20, 170),
                    Parent = confirmDialog
                };

                var noButton = new StandardButton
                {
                    Text = "Cancel",
                    Width = 100,
                    Location = new Point(170, 170),
                    Parent = confirmDialog
                };

                yesButton.Click += (s, e) =>
                {
                    // Restore defaults for current list
                    _messages = (_currentListIndex == 0) ? GetDefaultMessages() : GetSampleMessage();
                    RefreshEditorUI();
                    RefreshMessageUI();
                    SaveMessagesToFile();

                    confirmDialog.Dispose();

                    int messageCount = _messages.Count;
                    ScreenNotification.ShowNotification(
                        $"LT Messages: Loaded {messageCount} default message(s) for {GetListDisplayName(_currentListIndex)}",
                        ScreenNotification.NotificationType.Info);

                    Logger.Info($"Loaded default messages for {GetListDisplayName(_currentListIndex)}");
                };

                noButton.Click += (s, e) => confirmDialog.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to restore default messages");
                ScreenNotification.ShowNotification(
                    "LT Messages: Failed to load defaults",
                    ScreenNotification.NotificationType.Error);
            }
        }

        private void ResetAllListsToDefaults()
        {
            try
            {
                // STRONG warning - this resets ALL lists
                var confirmDialog = new Panel
                {
                    Size = new Point(500, 280),
                    Location = new Point((GameService.Graphics.SpriteScreen.Width - 500) / 2, (GameService.Graphics.SpriteScreen.Height - 280) / 2),
                    ZIndex = 15000,
                    Parent = GameService.Graphics.SpriteScreen,
                    BackgroundColor = new Color(40, 10, 10, 250), // Red tint for danger
                    ShowBorder = true
                };

                new Label
                {
                    Text = "⚠⚠⚠ RESET ALL LISTS? ⚠⚠⚠",
                    Font = GameService.Content.DefaultFont18,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Location = new Point(20, 20),
                    TextColor = new Color(255, 100, 100, 255), // Bright red
                    ShowShadow = true,
                    Parent = confirmDialog
                };

                new Label
                {
                    Text = "DANGER: This will reset ALL 6 message lists (0-5)\nback to their default states!\n\n• List 0: 30 default messages\n• Lists 1-5: Single sample message\n\nALL your custom messages in ALL lists will be lost!\n\nThis action CANNOT be undone!\n\nAre you absolutely sure?",
                    Width = 460,
                    Height = 160,
                    Location = new Point(20, 60),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont14,
                    Parent = confirmDialog
                };

                var yesButton = new StandardButton
                {
                    Text = "YES, RESET ALL LISTS",
                    Width = 180,
                    Location = new Point(20, 230),
                    Parent = confirmDialog
                };

                var noButton = new StandardButton
                {
                    Text = "Cancel (Recommended)",
                    Width = 160,
                    Location = new Point(210, 230),
                    Parent = confirmDialog
                };

                yesButton.Click += (s, e) =>
                {
                    try
                    {
                        // Reset all lists to defaults
                        for (int i = 0; i < MessageListCount; i++)
                        {
                            string filePath = GetFilePathForList(i);

                            // Create directory if needed
                            string directory = Path.GetDirectoryName(filePath);
                            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            // Create default file for this list
                            CreateDefaultMessageFile(filePath, i);
                            Logger.Info($"Reset List {i} to defaults");
                        }

                        // Reload the current list being edited
                        LoadMessagesFromFile();
                        RefreshEditorUI();
                        RefreshMessageUI();

                        confirmDialog.Dispose();

                        ScreenNotification.ShowNotification(
                            "LT Messages: All lists reset to defaults!",
                            ScreenNotification.NotificationType.Info);

                        Logger.Info("Reset all message lists to defaults");
                    }
                    catch (Exception resetEx)
                    {
                        Logger.Error(resetEx, "Failed to reset all lists");
                        ScreenNotification.ShowNotification(
                            "LT Messages: Failed to reset all lists",
                            ScreenNotification.NotificationType.Error);
                        confirmDialog.Dispose();
                    }
                };

                noButton.Click += (s, e) => confirmDialog.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show reset dialog");
                ScreenNotification.ShowNotification(
                    "LT Messages: Failed to show reset dialog",
                    ScreenNotification.NotificationType.Error);
            }
        }

        #endregion

        #region Message Sending

        private void OnMessageSelected(MessageEntry message)
        {
            Logger.Info($"Message selected: {message.Title}");

            // Check if LT mode is enabled
            if (!_ltModeEnabled.Value)
            {
                ScreenNotification.ShowNotification(
                    "LT Messages: LT Mode is disabled. Enable it in settings to send messages.",
                    ScreenNotification.NotificationType.Warning);
                Logger.Warn("Message send blocked - LT Mode is disabled");
                HidePopup();
                return;
            }

            if (_chatAction.Value == ChatAction.Send)
            {
                SendMessageAutomatic(message);
            }
            else
            {
                SendMessageClipboard(message);
            }
        }

        private void SendMessageClipboard(MessageEntry message)
        {
            try
            {
                string originalClipboard = null;
                string clipboardText;

                // Build message with chat command if specified
                string commandString = GetChatCommandString(_chatCommand.Value);
                if (!string.IsNullOrEmpty(commandString))
                {
                    clipboardText = $"{commandString} {message.Message}";
                }
                else
                {
                    // Default - just the message, will use whatever chat is active
                    clipboardText = message.Message;
                }

                // Clipboard operations need to run on STA thread
                var clipboardThread = new Thread(() =>
                {
                    try
                    {
                        // Save original clipboard
                        originalClipboard = System.Windows.Forms.Clipboard.GetText();

                        // Copy message
                        System.Windows.Forms.Clipboard.SetText(clipboardText);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Clipboard operation failed");
                        throw;
                    }
                });
                clipboardThread.SetApartmentState(ApartmentState.STA);
                clipboardThread.Start();
                clipboardThread.Join();

                Logger.Info($"Message copied to clipboard: {clipboardText}");

                // Restore clipboard after a delay
                Task.Run(async () =>
                {
                    await Task.Delay(10000); // 10 seconds

                    if (!string.IsNullOrEmpty(originalClipboard))
                    {
                        var restoreThread = new Thread(() =>
                        {
                            try
                            {
                                System.Windows.Forms.Clipboard.SetText(originalClipboard);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn(ex, "Failed to restore clipboard");
                            }
                        });
                        restoreThread.SetApartmentState(ApartmentState.STA);
                        restoreThread.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to copy message to clipboard");
                ScreenNotification.ShowNotification(
                    $"LT Messages: Failed to copy - {ex.Message}",
                    ScreenNotification.NotificationType.Error);
            }
        }

        private void SendMessageAutomatic(MessageEntry message)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Focus the game window
                    FocusGameWindow();

                    // Wait for focus to settle
                    await Task.Delay(100);

                    // Build the message to type (command + message, or just message)
                    string commandString = GetChatCommandString(_chatCommand.Value);
                    string fullMessage;

                    if (!string.IsNullOrEmpty(commandString))
                    {
                        fullMessage = $"{commandString} {message.Message}";
                        Logger.Info($"Sending message: {fullMessage}");
                    }
                    else
                    {
                        fullMessage = message.Message;
                        Logger.Info($"Sending message to active channel: {message.Message}");
                    }

                    // Open chat based on focus setting
                    if (_chatFocus.Value == ChatFocus.ShiftEnter)
                    {
                        // Open squad chat (Shift+Enter)
                        Logger.Debug("Sending Shift+Enter to open squad chat");
                        SendKeyPress(VK_RETURN, shift: true);
                    }
                    else
                    {
                        // Open last used chat (Enter)
                        Logger.Debug("Sending Enter to open last used chat");
                        SendKeyPress(VK_RETURN);
                    }

                    // Wait for chat box to fully open and be ready
                    await Task.Delay(150);

                    // Type the full message (with command if specified)
                    Logger.Debug($"Typing: {fullMessage}");
                    await TypeString(fullMessage, (int)_sendDelay.Value);

                    // Brief wait before sending
                    await Task.Delay(50);

                    // Send (Enter)
                    Logger.Debug("Sending Enter to send message");
                    SendKeyPress(VK_RETURN);

                    // Hide popup AFTER key sequence completes
                    HidePopup();

                    Logger.Info($"Message auto-sent: {message.Message}");

                    // No success notification - it interferes with the key sequence
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to auto-send message");
                    ScreenNotification.ShowNotification(
                        $"LT Messages: Failed to send - {ex.Message}",
                        ScreenNotification.NotificationType.Error);
                }
            });
        }

        #endregion
    }
}
