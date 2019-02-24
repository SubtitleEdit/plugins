using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    [InvokeOnLoad(false)]
    public class TriggerOnLoad : IPlugin
    {
        public string Name => "Trigger";

        public string Text => "Trigger";

        public decimal Version => 1.0m;

        public string Description => "Run on loaded";

        public string ActionType => "tool";

        public string Shortcut { get; }

        public TriggerOnLoad()
        {
            File.WriteAllText("d:/file.txt", "hello world");
            System.Diagnostics.Debug.WriteLine("instance created...");
        }

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            parentForm.Text = "Jacked! ^_^";

            //var timer = new Timer
            //{
            //    Interval = 1000 * 10,
            //};
            //timer.Tick += (sender, e) =>
            //{
            //    MessageBox.Show($"Is menu item valid: {menuItem != null}");
            //};
            //GC.KeepAlive(timer);
            //timer.Start();

            //https://stackoverflow.com/questions/171970/how-can-i-find-the-method-that-called-the-current-method
            // the info can be used to check wheter to trigger any action right away


            // #1
            //using System.Diagnostics;
            // Get call stack
            //StackTrace stackTrace = new StackTrace();

            // Get calling method name
            //Console.WriteLine(stackTrace.GetFrame(1).GetMethod().Name);

            // #2
            // new StackFrame(1).GetMethod().Name
            return string.Empty;
        }
    }
}
