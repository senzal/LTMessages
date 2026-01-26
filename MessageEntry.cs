using System;

namespace LTMessages
{
    /// <summary>
    /// Represents a message entry with a title and message content.
    /// </summary>
    public class MessageEntry
    {
        private const int MaxTitleLength = 16;

        private string _title;
        private string _message;

        /// <summary>
        /// Gets or sets the title (max 16 characters).
        /// Will be automatically truncated if longer.
        /// </summary>
        public string Title
        {
            get => _title;
            set => _title = TruncateString(value, MaxTitleLength);
        }

        /// <summary>
        /// Gets or sets the message.
        /// Length is validated when loading from file.
        /// </summary>
        public string Message
        {
            get => _message;
            set => _message = value ?? string.Empty;
        }

        /// <summary>
        /// Creates a new MessageEntry with the specified title and message.
        /// </summary>
        public MessageEntry(string title, string message)
        {
            Title = title;
            Message = message;
        }

        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public override string ToString()
        {
            return $"{Title}: {Message}";
        }
    }
}
