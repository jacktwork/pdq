using System;
using Unity;

namespace pdq
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine("Please enter a path and file pattern argument.");
        return;
      }

      var container = new UnityContainer();
      container.RegisterType<ILineBreakCounter, CPULineBreakCounter>();
      container.RegisterType<IFilesDict, FilesDict>();
      container.RegisterType<ILogger, ConsoleLogger>();

      DirectoryMonitor directoryMonitor = new DirectoryMonitor(container);
      directoryMonitor.Initialize(args[0], args[1]);
      directoryMonitor.Monitor(args[0], args[1]);

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }
  }
}
