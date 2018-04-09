using System;
using System.IO;
using System.Linq;
using System.Threading;
using Unity;
using Unity.Resolution;

namespace pdq
{
  public class DirectoryMonitor
  {
    protected ILogger _logger;
    protected IFilesDict _dict;
    private UnityContainer _container;
    public DirectoryMonitor(UnityContainer container)
    {
      _logger = container.Resolve<ILogger>();
      _dict = container.Resolve<IFilesDict>();
      _container = container;
    }

    public void Initialize(string path, string pattern)
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(path);

      FileInfo[] allFiles =
        directoryInfo.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly)
        .AsParallel()
        .ToArray();

      // note: do not want to init in parallel because significant
      // amount of huge files can overwhelm a small system (like my laptop)

      Console.WriteLine("Please wait, processing preexisting files...");
      foreach (FileInfo fileInfo in allFiles.ToArray())
      {
        Process(fileInfo, false);
      }
    }

    public void Monitor(string path, string pattern)
    {
      DirectoryInfo directoryInfo;
      DateTime previousDateTime = DateTime.MaxValue;
      string[] previousFiles = { };

      Console.WriteLine("Starting directory monitor loop...");

      // note: instructions did not say when to stop this loop
      // so it will go on forever and ever - until true is false
      // not good practice but will do for temporary test demo
      while (true)
      {
        directoryInfo = new DirectoryInfo(path);

        FileInfo[] allFiles =
          directoryInfo.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly)
          .AsParallel()
          .ToArray();

        string[] currentFilenames =
          allFiles.Select(a => a.Name.ToLower())
          .ToArray();

        FileInfo[] updatedFiles =
            allFiles
           .Where(a => a.LastWriteTimeUtc > previousDateTime)
           .Where(a => _dict.ContainsKey(a.Name))
           .ToArray();

        string[] deletedFiles =
          previousFiles.Except(currentFilenames)
          .ToArray();

        FileInfo[] createdFiles =
          allFiles.Where(a => !_dict.ContainsKey(a.Name))
          .ToArray();

        previousFiles = allFiles.Select(a => a.Name.ToLower()).ToArray();
        previousDateTime = DateTime.Now.ToUniversalTime();

        foreach (FileInfo fileInfo in updatedFiles.Concat(createdFiles).ToArray())
        {
          Thread t = new Thread(() => Process(fileInfo, true));
          t.Start();
        }

        foreach (string filename in deletedFiles)
        {
          _logger.Log(filename + " has been deleted");
          _dict.Delete(filename);
        }

        // for now hardwired to 10K - as per instructions
        // should get this value from app.config or some other way
        Thread.Sleep(10000);
      }
    }

    public void Process(FileInfo fileInfo, bool log)
    {
      // check 2GB limit
      if (fileInfo.Length >= Int32.MaxValue)
      {
        Console.WriteLine("file " + fileInfo.Name + " exceeds 2GB limit: " + fileInfo.Length);
        return;
      }

      bool newFile = !_dict.ContainsKey(fileInfo.Name);
      if (newFile)
      {
        _dict.Set(fileInfo.Name, 0);
      }

      int lineBreakCount = 0;
      using (ILineBreakCounter lineBreakCounter = (ILineBreakCounter)_container.Resolve<ILineBreakCounter>(new ParameterOverride("fileInfo", fileInfo)))
      {
        if (!lineBreakCounter.GetCount(out lineBreakCount))
        {
          Console.WriteLine("lineBreakCounter.Get failed: " + fileInfo.Name);
          return;
        }
      }

      if (log)
      {
        Log(fileInfo, lineBreakCount, newFile);
      }

      _dict.Set(fileInfo.Name, lineBreakCount);
    }



    public void Log(FileInfo fileInfo, int lineBreakCount, bool newFile)
    {
      if (newFile)
      {
        _logger.Log(fileInfo.Name + " created has " + lineBreakCount + " lines");
      }
      else
      {
        int prevcount = _dict.Get(fileInfo.Name);
        int diff = lineBreakCount - prevcount;
        string tmpstr = "0";
        if (diff > 0)
        {
          tmpstr = "+" + diff.ToString();
        }
        if (diff <= 0)
        {
          tmpstr = diff.ToString();
        }
        _logger.Log(fileInfo.Name + " modified changed lines: " + tmpstr);
      }
    }
  }
}