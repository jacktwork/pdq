using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdq
{
  public interface IFilesDict
  {
    int Get(string name);
    void Set(string name, int count);
    void Delete(string name);
    bool ContainsKey(string name);
  }
}
