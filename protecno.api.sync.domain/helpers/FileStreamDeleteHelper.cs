using System.IO;

namespace protecno.api.sync.domain.helpers
{
    public class FileStreamDeleteHelper : FileStream
    {
        readonly string path;

        public FileStreamDeleteHelper(string path, FileMode mode) : base(path, mode)
        {
            this.path = path;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
