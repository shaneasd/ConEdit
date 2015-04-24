using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    //See https://msdn.microsoft.com/library/ms244737%28VS.100%29.aspx
    public abstract class Disposable : IDisposable
    {
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected abstract void Dispose(bool disposing);
    }
}
