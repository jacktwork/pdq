using System.Collections.Concurrent;

namespace pdq
{
  public sealed class FilesDict : IFilesDict

  {

    //the volatile keyword ensures that the instantiation is complete 
    //before it can be accessed further helping with thread safety.

    private static volatile FilesDict _instance;

    private static readonly object _syncLock = new object();

    private ConcurrentDictionary<string, int> _dict;

    public FilesDict()
    {
      _dict = new ConcurrentDictionary<string, int>();
    }


    //uses a pattern known as double check locking

    public static FilesDict Instance
    {
      get
      {

        if (_instance != null) return _instance;

        lock (_syncLock)
        {
          if (_instance == null)
          {
            _instance = new FilesDict();
          }
        }

        return _instance;
      }
    }

    public int Get(string name)
    {
      return _dict[name.ToLower()];
    }

    public void Set(string name, int count)
    {
      _dict[name.ToLower()] = count;
    }

    public void Delete(string name)
    {
      int x;
      _dict.TryRemove(name.ToLower(), out x);
    }
    public bool ContainsKey(string name)
    {
      return _dict.ContainsKey(name.ToLower());
    }
  }
}
