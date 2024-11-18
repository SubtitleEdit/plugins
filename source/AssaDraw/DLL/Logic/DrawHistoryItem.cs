using System;
using System.Collections.Generic;

namespace AssaDraw.Logic
{
    public class DrawHistoryItem
    {
        public List<DrawShape> DrawShapes { get; set; }
        public DrawShape ActiveDrawShape { get; set; }
        public float ZoomFactor { get; set; }
        public DrawShape OldDrawShape { get; set; }
        public DrawCoordinate ActivePoint { get; set; }
        public DateTime Created { get; private set; }

        public DrawHistoryItem()
        {
            Created = DateTime.Now;
        }

        public int GetFastHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                foreach (var command in DrawShapes)
                {
                    hash = hash * 23 + command.GetFastHashCode();
                }

                return hash;
            }
        }
    }
}
