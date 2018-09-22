using Nikse.SubtitleEdit.PluginLogic;
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
        private readonly Assembly _libse;

        private const string DefaultNameSpace = "Nikse.SubtitleEdit.Core";

        public SECasingApi()
        {
            // return only AssemblyName
            //var libse = Assembly.GetEntryAssembly().GetReferencedAssemblies().FirstOrDefault(s => s.Name.Equals("libse"));

            _libse = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.FullName.Contains("libse"));

            var fixCasingT = _libse.GetType($"{DefaultNameSpace}.FixCasing");
            _fixCasing = Activator.CreateInstance(fixCasingT, "en");
            // TODO: Build strongly typed function with Expression
        }

        public void DoCasing(List<Paragraph> paragraphs, List<string> names)
        {
            //List<object> prgObjs = new List<object>();
            Type paragraphT = _libse.GetType($"{DefaultNameSpace}.Paragraph");

            // constructed generic list of paragraphs
            Type genericListOfParagraph = typeof(List<>).MakeGenericType(paragraphT);

            object genericList = Activator.CreateInstance(genericListOfParagraph);

            MethodInfo methodAdd = genericListOfParagraph.GetMethod("Add");

            //marshall this plugin's Paragraph type to SubtileEdit.exe's
            foreach (Paragraph p in paragraphs)
            {
                var marshalP = Activator.CreateInstance(paragraphT, p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                methodAdd.Invoke(genericList, new[] { marshalP });
            }

            Type subtitleT = _libse.GetType($"{DefaultNameSpace}.Subtitle");
            ConstructorInfo constructInfo = subtitleT.GetConstructor(new[] { genericListOfParagraph });

            // object of subtile edit
            var subtitle = constructInfo.Invoke(new[] { genericList });

            Debug.WriteLine($"is subtile: {subtitle == null}");
            // create instance of SubtileEdit.exe's Subtitle type passing the object marshalled to libse.dll type.
            //object subObj = Activator.CreateInstance(_libse.GetType($"{DefaultNameSpace}.Subtitle"), x);

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

            fieldNames.SetValue(_fixCasing, names);

            foreach (string name in (List<string>)fieldNames.GetValue(_fixCasing))
            {
                Debug.WriteLine(name);
            }

            // invoke method and pass the instance and the paragraphs marshalled to libse.dll paragraph type.
            //fixMethod.Invoke(_fixCasing, prgObjs.ToArray());
            fixMethod.Invoke(_fixCasing, new[] { subtitle });


            // make indexer property accessor
            // genericListOfParagraph.GetProperty("Item") // this is the indexer prop for all generic list
            // select indexer property

            PropertyInfo indexProp = genericListOfParagraph.GetProperties().Where(p => p.GetIndexParameters().Length > 0).First();
            
            // marshall back the result and store them in local paragraphs
            int count = paragraphs.Count;
            for (int i = 0; i < count; i++)
            {
                // marshall back to online-casing's paragraph type
                var pSE = indexProp.GetValue(genericList, new object[] { i });
                string text = (string)pSE.GetType()
                    .GetProperty("Text", BindingFlags.Instance | BindingFlags.Public)
                    .GetValue(pSE);

                if (text.Equals(paragraphs[i].Text, StringComparison.Ordinal) == false)
                {
                    paragraphs[i].Text = text;
                }
            }

        }
        
    }
}
