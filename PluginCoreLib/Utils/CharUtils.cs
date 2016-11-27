namespace PluginCoreLib.Utils
{
    public static class CharUtils
    {
        public static bool IsAscii(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch < 'Z');

        public static bool IsDigit(char ch) => (ch >= '0' && ch <= '9');
    }
}
