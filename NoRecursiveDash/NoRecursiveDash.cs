using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NoRecursiveDash
{
    public interface IFixCommonError
    {
        void Fix(object subtitle, object callbacks);
    }

    public class NoRecursiveDash : IFixCommonError
    {
        private InvokerProxy _invokerProxy;

        public NoRecursiveDash(object invoker)
        {
            _invokerProxy = new InvokerProxy(invoker);
        }

        public void Fix(object subtitle, object callbacks)
        {
            var sub = ToDomainSubtitle(subtitle);
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            foreach (Subtitle.Paragraph paragraph in sub.Paragraphs)
            {
            }
        }

        private Subtitle ToDomainSubtitle(object subtitle)
        {
            // ReSharper disable once PossibleNullReferenceException
            var paragraphsReflection = subtitle.GetType()
                .GetProperty("Pargraphs", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(subtitle);

            return new Subtitle(subtitle, Create(paragraphsReflection));

            IReadOnlyCollection<Subtitle.Paragraph> Create(object paragraphs)
            {
                var result = new List<Subtitle.Paragraph>();
                var enumerable = (IEnumerable)paragraphs;
                foreach (var reflectionParagraph in enumerable)
                {
                    var paragraph = CreateParagraph(reflectionParagraph);
                    if (paragraph != null)
                    {
                        result.Add(paragraph);
                    }
                }

                return result;

                Subtitle.Paragraph CreateParagraph(object instance) => new Subtitle.Paragraph(instance);
            }
        }
    }

    public class Subtitle
    {
        private readonly object _subtitle;

        public IReadOnlyCollection<Paragraph> Paragraphs { get; set; }

        public Subtitle(object subtitle, IReadOnlyCollection<Paragraph> paragraphs)
        {
            _subtitle = subtitle;
            Paragraphs = paragraphs;
        }

        public class Paragraph
        {
            private readonly object _instance;
            
            public string Text { get; set; }

            public Paragraph(object instance)
            {
                _instance = instance;
            }
        }
    }

    public class InvokerProxy
    {
        private readonly object _invoker;

        public InvokerProxy(object invoker)
        {
            _invoker = invoker;
        }
        
        // Add method you wish to access from invoker wrapped object
    }
}