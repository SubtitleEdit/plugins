using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    [InvokeOnLoad(false)]
    public class JackSE : IPlugin
    {
        // note: probably not a good idea because SE will Load entire assembly for each invoke
        //private volatile static bool isCapture = false;

        public JackSE()
        {
            //if (isCapture)
            //{
            //    return;
            //}

            //AppDomain.CurrentDomain.GetAssemblies();
            //Type mainForm = Assembly.GetEntryAssembly().GetType("Nikse.SubtitleEdit.Forms.Main");
            //AppDomain.CurrentDomain.

            //if (mainForm != null)
            //{
            //    //AddMergeLinesButton((Form)mainForm);
            //}


            // TODO: check if main is already jacked
            //Type programmType = Assembly.GetEntryAssembly().GetType("Nikse.SubtitleEdit.Program");
            //MethodInfo mainMi = programmType.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            // this one is started in new thread and 
            //var thread = new Thread(() =>
            //{
            //    Thread.CurrentThread.IsBackground = false;
            //    isCapture = true;
            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    var mainInstance = Activator.CreateInstance(mainForm);
            //    Application.Run((Form)mainInstance);
            //});
            //thread.Start();

            // close current instance start a new instance where the main is already captured (this didn't work as expected...)
            //Application.Exit();

            //AppDomain.CurrentDomain

            // try to force application / appdomain thread exception which will be capture in Program.cs Application exception handler
            // note: make sure IF #DEBUG instead of IF !#DEBUG becuase the handler will only be added when !#DEBUG by default
            //var t = new Thread(() => throw new InvalidOperationException());
            //t.Start();
        }

        public JackSE(Form mainForm)
        {
            MessageBox.Show("main captured");
        }


        /// <summary>
        /// Private members binding flags.
        /// </summary>
        private readonly BindingFlags _privateMembersFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private Form _parentForm;
        private Func<object, MenuStrip> MenuStripProvider;

        /// <summary>
        /// This function will return true if there are selected items from listview
        /// </summary>
        private Func<bool> ShouldInvokeMerge;

        private Func<object, bool> ShouldInvokeMergeExp;
        private Action<object, EventArgs> InsertLineInvoker;

        #region Metadata

        public string Name => "JackSE";
        public string Text => "JackSE";
        public decimal Version => 1;
        public string Description => "Inject features into Subtilte Edit.";
        public string ActionType => "tool";
        public string Shortcut => string.Empty;

        #endregion

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            if (HackTools(parentForm))
            {
                return string.Empty;
            }

            // false: operation shouldn't continue
            if (DemoHack(parentForm) == false)
            {
                return string.Empty;
            }

            _parentForm = parentForm;

            // url format in configuration
            // {"name": "https://www.github.com/ivandrofly"}
            BuildProviders();
            string configFile = Path.Combine(FileUtils.Plugins, "jack-se-config.xml");
            if (File.Exists(configFile))
            {
                // Xml template
                /*
                 * <urls>
                 *  <url title="Ivandrofly (github)">https://wwww.github.com/ivandrofly</url>
                 *  <url title="<url-title>">https://wwww.github.com/ivandrofly</url>
                 * </urls>
                 */
                XDocument xdoc = XDocument.Load(configFile);
                Dictionary<string, string> dic = xdoc.Root.Elements("url").ToDictionary(x => x.Attribute("title").Value, x => x.Value);
                // var urls = File.ReadAllLines(configFile).Select(l => l.Split('=').Aggregate()
                LoadLinks(dic);
            }
            else
            {
                MessageBox.Show("URLS CONFIG FILE NOT FOUND!");
            }

            AddMergeLinesButton(parentForm);
            // returns type of Main
            //MessageBox.Show(type.ToString());
            SelfDestroy();
            return string.Empty;
        }

        private bool ShouldLoadLinks()
        {
            // check if loads were already loadedkdkd
            return true;
        }

        private void LoadLinks(IDictionary<string, string> links)
        {
            // type of SE Main
            Type type = _parentForm.GetType();

            // get menuStrip1 type: MenuStrip

            // this is the control at top position of subtitle edit
            //MenuStrip menuStrip = (MenuStrip)type.GetField("menuStrip1", bindingFlag).GetValue(_parentForm);
            MenuStrip menuStrip = MenuStripProvider(_parentForm);
            if (menuStrip == null)
            {
                return;
            }

            // always insert at end-4 items
            int insertIdx = Math.Max(menuStrip.Items.Count - 4, 0);

            // this is the link menu item that will be diplayed at top bar
            ToolStripMenuItem linksToolStripItem = new ToolStripMenuItem("Links");

            // key = title
            // value = url
            foreach (KeyValuePair<string, string> link in links)
            {
                if (!Uri.TryCreate(link.Value, UriKind.Absolute, out Uri result))
                {
                    continue;
                }

                linksToolStripItem.DropDownItems.Add(link.Key, null, delegate { Process.Start(result.AbsoluteUri); });
            }

            // add link menu item to SubtitleEdit menu-item
            menuStrip.Items.Insert(insertIdx, linksToolStripItem);
        }

        private void BuildProviders()
        {
            BuildMenuStripProvider();
            BuildMergeInvoker();
            BuildInsertInvoker();
        }

        private void BuildMenuStripProvider()
        {
            // the type that represent Main which inhrets Form
            Type formType = _parentForm.GetType();
            // factory MenuStripProvider
            ParameterExpression paramExp = Expression.Parameter(typeof(object), "target");
            // convert the param from obj to Menu then access menuStrip1
            MemberExpression fieldExp = Expression.Field(Expression.Convert(paramExp, formType), "menuStrip1");
            // convert type of field to its defined type (MenuStrip)
            UnaryExpression convertToMenuStripExp = Expression.TypeAs(fieldExp, fieldExp.Type);
            // now this can be called to provide MenuStrip
            MenuStripProvider = Expression.Lambda<Func<object, MenuStrip>>(convertToMenuStripExp, paramExp).Compile();
        }

        private void BuildMergeInvoker()
        {
            Type formType = _parentForm.GetType();
            ParameterExpression paramExp = Expression.Parameter(typeof(object), "target");

            // name of subtile edit's main listview
            const string fieldName = "SubtitleListview1";

            // # USING REFLECTION
            FieldInfo lvFielInfo = _parentForm.GetType().GetField(fieldName, _privateMembersFlags);
            ListView mainListview = (ListView)lvFielInfo.GetValue(_parentForm);
            ShouldInvokeMerge = () => mainListview.SelectedItems.Count > 0;

            // # USING EXPRESSION

            // convert the param to type of Main (form)
            UnaryExpression convertExp = Expression.TypeAs(paramExp, formType);
            // access a field named `fielName` and cast it to type of listview (which is the base class of SubtitleListView) in _parentForm
            UnaryExpression lvexp = Expression.TypeAs(Expression.Field(convertExp, fieldName), typeof(ListView));

            // from that field access Property named SelectedItems
            MemberExpression selitemsExp = Expression.Property(lvexp, "SelectedItems");
            // from selected items select property named Count
            MemberExpression countExp = Expression.Property(selitemsExp, "Count");
            // from prop named Count text if it's greater than zero (0)
            BinaryExpression testExp = Expression.GreaterThan(countExp, Expression.Constant(0));

            // this takes param of object -> target
            ShouldInvokeMergeExp = Expression.Lambda<Func<object, bool>>(testExp, paramExp).Compile();
        }

        private void BuildInsertInvoker()
        {
            // method name InsertLineToolStripMenuItemClick
            Type mainType = _parentForm.GetType();

            // build parameterskd
            ParameterExpression senderParam = Expression.Parameter(typeof(object), "sender");
            ParameterExpression eventArgParam = Expression.Parameter(typeof(EventArgs), "eventArg");

            // map method
            MethodInfo insertClick = mainType.GetMethod("InsertLineToolStripMenuItemClick", _privateMembersFlags);
            if (insertClick == null)
            {
                throw new InvalidOperationException("Method not found!");
            }

            ConstantExpression targetObject = Expression.Constant(_parentForm, mainType);
            MethodCallExpression methodCallExp = Expression.Call(targetObject, insertClick, senderParam, eventArgParam);
            // TODO: BUILD THE HANDLER
            InsertLineInvoker = Expression.Lambda<Action<object, EventArgs>>(methodCallExp, senderParam, eventArgParam).Compile();
        }

        private void AddMergeLinesButton(Form parentForm)
        {
            GroupBox gb = null;
            // get the container where the main textbox is
            // insert button on left side of main-textbox
            // add anchor
            // images
            // add: handler which will merge text
            Button buttonMergeLines = new Button
            {
                Name = "jackseButtonMergeLines",
                Text = "ML",
                //Size = { Width = 44, Height = 33 } // doesn't work with valuetype
            };

            // set button size :)
            buttonMergeLines.Size = new System.Drawing.Size(44, 33);

            // get the type which is main
            Type formType = parentForm.GetType();
            GroupBox groupBox = (GroupBox)formType.GetField("groupBoxEdit", _privateMembersFlags).GetValue(parentForm);

            Control seTextbox = null;

            Control textBoxListViewText = groupBox.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name.Equals("textBoxListViewText", StringComparison.Ordinal));

            // get subtitle's main textbox
            // which will be used to get the cordinates to copy to merge lines button
            foreach (Control control in groupBox.Controls)
            {
                if (control.Name.Equals("textBoxListViewText", StringComparison.OrdinalIgnoreCase))
                {
                    seTextbox = control;
                    break;
                }
            }

            if (seTextbox == null)
            {
                MessageBox.Show("SETEXTBOX COULDN'T BE FOUND!");
                return;
            }

            groupBox.Controls.Add(buttonMergeLines);

            // get cordinates/location

            // merge handler
            MethodInfo methodInfo = formType.GetMethod("MergeWithLineAfter", _privateMembersFlags);
            buttonMergeLines.Click += delegate
            {
                // TODO: Handle the hash insert from SE
                // because passing true will always insert hyphen
                // and passing false will not insert hyphen even if it's a dialog text

                // optimize this by building invoker with ExpressionTree to avoid reflecting everytime user click
                // this button

                // TODO: handle when no item is selected from listview (which throws exception)
                if ( /*ShouldInvokeMerge()*/ ShouldInvokeMergeExp(_parentForm))
                {
                    methodInfo.Invoke(parentForm, new object[] { false });
                }
            };

            // handle button size
            // handle button location
            buttonMergeLines.Location = new System.Drawing.Point(seTextbox.Location.X - buttonMergeLines.Size.Width - 3,
                seTextbox.Location.Y);
        }

        /// <summary>
        /// Remove JackSE from menu items (in order to prevent re-adding control)
        /// </summary>
        private void SelfDestroy()
        {
            // toolsToolStripMenuItem
            Type mainType = _parentForm.GetType();
            FieldInfo fieldInfo = mainType.GetField("toolsToolStripMenuItem", _privateMembersFlags);
            ToolStripMenuItem toolStripMenu = (ToolStripMenuItem)fieldInfo.GetValue(_parentForm);
            for (int i = toolStripMenu.DropDown.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem tsi = toolStripMenu.DropDown.Items[i];
                if (tsi.Text.Equals(Text, StringComparison.OrdinalIgnoreCase) || tsi.Text.Equals(Name, StringComparison.OrdinalIgnoreCase))
                {
                    toolStripMenu.DropDownItems.RemoveAt(i);
                    break;
                }
            }
        }

        private static bool DemoHack(object main)
        {
            BindingFlags privateFlag = BindingFlags.NonPublic | BindingFlags.Instance;

            // get type of main
            Type mainType = main.GetType();
            Type aboutType = main.GetType().Assembly.GetType("Nikse.SubtitleEdit.Forms.About");

            // AboutToolStripMenuItemClick
            // main->aboutToolStripMenuItem.click->AboutToolStripMenuItemClick
            var menuItem = mainType.GetField("aboutToolStripMenuItem", privateFlag).GetValue(main);
            var clickcEventInMenuItem = menuItem.GetType().GetEvent("Click");
            var eventType = clickcEventInMenuItem.EventHandlerType;
            var clickMethodHandler = mainType.GetMethod("AboutToolStripMenuItemClick", privateFlag);
            var del = Delegate.CreateDelegate(eventType, main, clickMethodHandler);
            clickcEventInMenuItem.RemoveEventHandler(menuItem, del);

            MessageBox.Show("event unhooked");
            return false;
        }

        private bool HackTools(object mainInstance)
        {
            var toolsFieldInfo = mainInstance.GetType().GetField("toolsToolStripMenuItem", BindingFlags.Instance | BindingFlags.NonPublic);
            var toolsInstance = toolsFieldInfo.GetValue(mainInstance);

            var toolsClickEvent = toolsFieldInfo.FieldType.GetEvent("Click", BindingFlags.Public | BindingFlags.Instance);
            var toolsClickHandler = Delegate.CreateDelegate(toolsClickEvent.EventHandlerType, this, "ToolsClickHandler");
            toolsClickEvent.AddEventHandler(toolsInstance, toolsClickHandler);

            // note: is "this" disposed? 

            return true;
        }

        private void ToolsClickHandler(object sender, EventArgs e)
        {
            try
            {
                string output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "downloads");
                File.WriteAllText(Path.Combine(output, "outfile.txt"), "tadaaaa!");
            }
            catch
            {
                // ignore
            }
        }

    }
}
