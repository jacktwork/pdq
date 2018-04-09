using System;
using System.IO;

namespace pdq
{
  public interface ILineBreakCounter : IDisposable
  {
    bool GetCount(out int lineBreakCount);
  }
}
