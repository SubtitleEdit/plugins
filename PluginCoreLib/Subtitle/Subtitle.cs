namespace PluginCoreLib.Subtitle
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Subtitle : IList<Paragraph>
    {
        private List<Paragraph> _paragraphs;
        public string FileName { get; set; }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
        }

        public Subtitle(IEnumerable<Paragraph> paragraphs)
        {
            _paragraphs = paragraphs.ToList();
        }

        #region IList<Paragraph>

        public Paragraph this[int index]
        {
            get
            {
                return _paragraphs[index];
            }

            set
            {
                _paragraphs[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return _paragraphs.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Paragraph>)_paragraphs).IsReadOnly;
            }
        }

        public void Add(Paragraph item)
        {
            _paragraphs.Add(item);
        }

        public void Clear()
        {
            _paragraphs.Clear();
        }

        public bool Contains(Paragraph item)
        {
            return _paragraphs.Contains(item);
        }

        public void CopyTo(Paragraph[] array, int arrayIndex)
        {
            _paragraphs.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Paragraph> GetEnumerator()
        {
            return _paragraphs.GetEnumerator();
        }

        public int IndexOf(Paragraph item)
        {
            return _paragraphs.IndexOf(item);
        }

        public void Insert(int index, Paragraph item)
        {
            _paragraphs.Insert(index, item);
        }

        public bool Remove(Paragraph item)
        {
            return _paragraphs.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _paragraphs.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _paragraphs.GetEnumerator();
        }


        #endregion

        public void Renumber(int startNumber = 1)
        {
            if (startNumber <= 0)
            {
                startNumber = 1;
            }
            foreach (var p in _paragraphs)
            {
                p.Number = startNumber;
            }
        }

        public void RemoveById(string id)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                Paragraph p = _paragraphs[i];
                if (p.ID.Equals(id, StringComparison.Ordinal))
                {
                    _paragraphs.Remove(p);
                    break;
                }
            }
        }

        public int RemoveEmptyLines()
        {
            int count = _paragraphs.Count;
            if (count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    if (string.IsNullOrWhiteSpace(p.Text))
                        _paragraphs.RemoveAt(i);
                }
                if (count != _paragraphs.Count)
                    Renumber(firstNumber);
            }
            return count - _paragraphs.Count;
        }

    }
}