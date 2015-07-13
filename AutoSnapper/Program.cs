using CLAP;
using NLog;

namespace AutoSnapper
{
  public class Program {
    private static Logger _logger = LogManager.GetCurrentClassLogger();
    public static void Main(string[] args) {
      _logger.Trace("Program Start.");
      Parser.Run<AutoSnapper>(args);
      _logger.Trace("Program End.");
      pause();
    }

    static void pause() {
#if DEBUG
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
#endif
    }
  }
}
