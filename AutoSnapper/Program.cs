using System;

namespace AutoSnapper
{
  class Program
  {
    public static void Main(string[] args)
    {
      Console.Write(Services.GetServiceOutput());
      Console.Write(SnapshotManager.CreateSnapshotAllVolumes());
      Console.Write(SnapshotManager.PurgeOldSnapshots());
      
#if DEBUG
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
#endif
    }
  }
}
