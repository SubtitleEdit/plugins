namespace Nikse.SubtitleEdit.PluginLogic.Helpers
{
    public static class CharUtils
    {
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        public static bool IsDigit(char ch) => (ch >= '0') && (ch <= '9');

        /// <summary>
        /// Checkes if given character is hexadecimal
        /// </summary>
        public static bool IsHexadecimal(char ch) => (ch >= '0') && (ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');

    }
}
