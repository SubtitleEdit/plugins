using System;

namespace OneDriveSave
{
    public class OneDriveContent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public string Path { get; internal set; }

        public OneDriveContent()
        {
        }

        public bool IsFolder
        {
            get
            {
                return Type.ToLower() == "folder";
            }
        }

        public bool IsFile
        {
            get
            {
                return Type.ToLower() == "file";
            }
        }

    }
}