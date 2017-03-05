using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class TimeCode
    {
        public const double BaseUnit = 1000.00;

        private double _totalMilliseconds = 0d;

        public TimeCode()
        {
        }

        public TimeCode(double totalMilliseconds)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            _totalMilliseconds = hours * 60 * 60 * BaseUnit + minutes * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
        }

        public double TotalMilliseconds
        {
            get => _totalMilliseconds;
            set => _totalMilliseconds = value;
        }

        public override string ToString()
        {
            var t = TimeSpan.FromMilliseconds(_totalMilliseconds);
            string ts = $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00},{t.Milliseconds:000}";
            if (_totalMilliseconds < 0)
                return "-" + ts.Replace("-", string.Empty);
            return ts;
        }

    }
}