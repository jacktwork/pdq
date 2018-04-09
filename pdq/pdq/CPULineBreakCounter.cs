using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace pdq
{
  class CPULineBreakCounter : ILineBreakCounter 
  {
    IntPtr _ptr = IntPtr.Zero;
    FileInfo _fileInfo;
    public CPULineBreakCounter(FileInfo fileInfo)
    {
      if (!Allocate(fileInfo))
      {
        Debug.WriteLine("CPULineBreakCounter constructor allocate failed: "+fileInfo.Name);
      }
      _fileInfo = fileInfo;
    }
    public virtual bool GetCount(out int lineBreakCount)
    {
      lineBreakCount = 0;
      try
      {
        int processorCount = Environment.ProcessorCount;
        Task[] tasks = new Task[processorCount];
        int portion = (int)_fileInfo.Length / processorCount;
        int from = 0;
        int to = portion;
        portion += 1;

        for (int i = 0; i < processorCount; i++)
        {
          tasks[i] = CPUCounter.Count(_ptr, from, to);
          from += portion;
          to += portion;
        }
        Task.WaitAll(tasks);

        for (int i = 0; i < processorCount; i++)
        {
          LineBreakCount result = ((Task<LineBreakCount>)tasks[i]).Result;
          if (!result.Success)
          {
            return false;
          }
          lineBreakCount += result.Count;
        }

      }
      catch (Exception ex)
      {
        Debug.WriteLine("ex: " + ex.Message);
        return false;
      }

      return true;
    }
    protected bool Allocate(FileInfo fileInfo)
    {
      int len = (int)fileInfo.Length;
      try
      {

        int attempts = 0;
        bool success = false;
        while (!success && attempts < 10)
        {
          try
          {
            _ptr = Marshal.AllocHGlobal(len);
            success = true;
          }
          catch (OutOfMemoryException ex)
          {
            Debug.WriteLine("ex: " + ex.Message);
            // holding pattern - wait for memory to become available

            // wait for memory to free up
            // possibly replace 100 with random 0..100?
            Thread.Sleep(100);
          }
        }

        if (attempts >= 10)
        {
          Console.WriteLine("Allocate max attempts failed");
          return false;
        }

        Marshal.Copy(File.ReadAllBytes(fileInfo.FullName), 0, _ptr, len);
      }
      catch (Exception ex)
      {
        Debug.WriteLine("ex: " + ex.Message);
        return false;
      }

      return true;
    }
    public void Dispose()
    {
      Marshal.FreeHGlobal(_ptr);
    }
  }
}
