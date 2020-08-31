using Nikse.SubtitleEdit.PluginLogic;
using OnlineCasing.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OnlineCasing
{
    internal class SECasingApi
    {
        private readonly object _fixCasing;

        /// <summary>
        /// The default assembly is the assembly that contains the "libse" types.
        /// </summary>
        private readonly Assembly _coreAssembly;

        private const string DefaultNameSpace = "Nikse.SubtitleEdit.Core";

        public SECasingApi()
        {
            //var libse = Assembly.GetEntryAssembly().GetReferencedAssemblies().FirstOrDefault(s => s.Name.Equals("libse"));

            // note: when SubtitlEdit is running in installed mode we won't be 
            // able to retrive "libse.dll" using this approach because the assembly is merged into SubtitleEdit.exe
            _coreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.FullName.Contains("libse"));
            _coreAssembly = _coreAssembly ?? Assembly.GetEntryAssembly();
            _fixCasing = Activator.CreateInstance(_coreAssembly.GetType($"{DefaultNameSpace}.FixCasing"), "en");

            // TODO: Build strongly typed function with Expression
            //Debug.WriteLine("leaving static SECasingApi ctor");
        }

        public void DoCasing(List<Paragraph> paragraphs, List<string> names)
        {
            //List<object> prgObjs = new List<object>();
            Type paragraphT = _coreAssembly.GetType($"{DefaultNameSpace}.Paragraph");

            // constructed generic list of paragraphs
            Type genericListOfParagraph = typeof(List<>).MakeGenericType(paragraphT);

            // create instance of generic list of paragraph
            object genericList = Activator.CreateInstance(genericListOfParagraph);

            // get Add method of generic list of paragraph
            MethodInfo methodAdd = genericListOfParagraph.GetMethod("Add");

            //marshall this plugin's Paragraph type to SubtileEdit.exe's
            foreach (Paragraph p in paragraphs)
            {
                // make instance of generic of marshall paragraph
                object marshallP = Activator.CreateInstance(paragraphT, p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                methodAdd.Invoke(genericList, new[] { marshallP });
            }

            // get subtitle type of/from libse.dll
            Type subtitleT = _coreAssembly.GetType($"{DefaultNameSpace}.Subtitle");

            // create instance of SubtileEdit.exe's Subtitle type passing the object marshalled to libse.dll type.
            ConstructorInfo constructInfo = subtitleT.GetConstructor(new[] { genericListOfParagraph });

            Type fixCasingT = _fixCasing.GetType();

            // Configure FixCasing
            //public bool FixNormal = true;
            //public bool FixNormalOnlyAllUppercase = false;
            //public bool FixMakeLowercase = false;
            //public bool FixMakeUppercase = false;

            // invoke the method Fix in FixCasing type in libse.dll (which will do casing using a custom name instead of build-in name)
            MethodInfo fixMethod = fixCasingT.GetMethod("Fix", BindingFlags.Instance | BindingFlags.Public);

            // *readnly field set new names to do casing with
            FieldInfo fieldNames = fixCasingT.GetField("_names", BindingFlags.Instance | BindingFlags.NonPublic);

            // make FixCasing name point to passed names instead of subtitle edit built-in name.
            fieldNames.SetValue(_fixCasing, names);

            // object of subtile edit
            object subtitle = constructInfo.Invoke(new[] { genericList });

            // invoke method and pass the instance and the paragraphs marshalled to libse.dll paragraph type.
            fixMethod.Invoke(_fixCasing, new[] { subtitle });

            // make indexer property accessor
            // genericListOfParagraph.GetProperty("Item") // this is the indexer prop for all generic list
            // select indexer property

            PropertyInfo indexProp = genericListOfParagraph.GetProperties().Where(p => p.GetIndexParameters().Length > 0).First();

            // marshall back the result and store them in local paragraphs
            int count = paragraphs.Count;
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.Public;
            for (int i = 0; i < count; i++)
            {
                // marshall back to online-casing's paragraph type
                object pSE = indexProp.GetValue(genericList, new object[] { i });
                string text = (string)pSE.GetType()
                    .GetProperty("Text", binding)
                    .GetValue(pSE);

                if (text.Equals(paragraphs[i].Text, StringComparison.Ordinal) == false)
                {
                    paragraphs[i].Text = text;
                }
            }

            // NOTE:
            // IN ORDER FOR THIS METHOD WORK, SUBTITLE EDIT METHOD Fix(string text, string lastLine, List<string> nameList, CultureInfo subtitleCulture, double millisecondsFromLast)
            // NEEDS TO BE CHANGED WHERE casing = false to true,
        }

        public void DoCasing(CasingContext context)
        {
            var publicMemberFlags = BindingFlags.Public | BindingFlags.Instance;
            Type stripTextT = _coreAssembly.GetType(DefaultNameSpace + ".StrippableText");
            MethodInfo fixCasing = stripTextT.GetMethod("FixCasing", publicMemberFlags);
            Paragraph preParagraph = null;
            double gaps = 10000;
            foreach (Paragraph p in context.Paragraphs)
            {
                object stripTextObj = Activator.CreateInstance(stripTextT, p.Text);
                if (preParagraph != null)
                {
                    gaps = p.StartTime.TotalMilliseconds - preParagraph.EndTime.TotalMilliseconds;
                }

                fixCasing.Invoke(stripTextObj, new object[] { context.Names, true, false, false, p?.Text, gaps });
                p.Text = (string)stripTextT.GetProperty("MergedString", publicMemberFlags).GetValue(stripTextObj);
                preParagraph = p;
            }

        }

    }
}

// Author: Ivandro Ismael Gomes Jao (@ivandrofly)