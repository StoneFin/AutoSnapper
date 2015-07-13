using System;
using Fclp;
using CLAP;

namespace AutoSnapper
{
  public class Program
  {
    public static void Main(string[] args) {
      Parser.Run<AutoSnapper>(args);
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
