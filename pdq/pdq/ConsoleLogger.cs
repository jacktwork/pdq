using System;

namespace pdq
{
  public class ConsoleLogger : ILogger
  {
    public void Log(string text)
    {
      Console.WriteLine(text);
    }
  }
}
