using System;
using System.Collections.Generic;

namespace AutoSnapper
{
  class Program
  {
    public static void Main(string[] args)
    {
      Console.Write(Services.GetServiceOutput());
      Console.Write(SnapshotManager.CreateSnapshotAllVolumes());
      Console.Write(SnapshotManager.PurgeOldSnapshots());

      //TODO: add command line args to specify this, or put it into the config, or whatever
      var instanceIds = new List<string>();

      Console.Write(InstanceManager.StopInstances(instanceIds));
#if DEBUG
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
#endif
    }
  }
}
