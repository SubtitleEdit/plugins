using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ExposeSEApi : IPlugin
    {
        /// <summary>
        /// The function pointer to Subtitle Edit's RemoveHtmlTags function.
        /// </summary>
        private Func<string, bool, string> RemoveHtmlTags;

        public string Name => nameof(ExposeSEApi);
        public string Text => nameof(ExposeSEApi);
        public decimal Version => .1m;
        public string Description => "Exposed Subtitle Edit APIs using expression/reflection";
        public string ActionType => "tool";
        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            // the type of Main.cs
            Type parentType = parentForm.GetType();

            // get the assembly where type of Main is defined
            Assembly assembly = parentType.Assembly;

            // TODO: handle portable mode!
            // note: in portable mode there is no HtmlUtil in SubtitleEdit, the type is located in libse assembly
            // which can be retrived from appdomain at this point since the plugins are loaded in same appdomain as main exe.
            // get the HtmlUtil type
            Type htmlUtilType = assembly.GetType("Nikse.SubtitleEdit.Core.HtmlUtil");

            // build parameters
            ParameterExpression inputExp = Expression.Parameter(typeof(string), "input");
            ParameterExpression removeASSTag = Expression.Parameter(typeof(bool), "removeAssTag");

            // build method call
            MethodInfo removeHtmltagsMI = htmlUtilType.GetMethod("RemoveHtmlTags", BindingFlags.Public | BindingFlags.Static);
            MethodCallExpression methodCallExp = Expression.Call(removeHtmltagsMI, inputExp, removeASSTag);

            // build/compile lambda to function
            RemoveHtmlTags = Expression.Lambda<Func<string, bool, string>>(methodCallExp, inputExp, removeASSTag).Compile();

            string text = "<i>Fooobar</i>";

            // call the method Subtitle Edit RemoveHtmlTags method from HtmlUTil to remove html tags
            text = RemoveHtmlTags(text, true);

            MessageBox.Show(text);

            return string.Empty;
        }
    }
}
