using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace pdq
{
  public static class CPUCounter 
  {
    public static async Task<LineBreakCount> Count(IntPtr ptr, int from, int to)
    {
      int count = 0;
      try
      {
        unsafe
        {
          byte* p = (byte*)ptr + from;

          for (int i = from; i <= to; i++)
          {
            if (*p == 13 && *(p + 1) == 10)
            {
              count++;
            }
            p++;
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("ex: " + ex.Message);
        return new LineBreakCount() { Success = false };
      }

      return new LineBreakCount() { Success = true, Count = count };
    }
  }
}
