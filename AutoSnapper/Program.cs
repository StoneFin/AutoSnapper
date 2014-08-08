using System;

namespace AutoSnapper
{
  class Program
  {
    public static void Main(string[] args)
    {
      Console.Write(Services.GetServiceOutput());
      
      foreach (var arg in args)
      {
        if (arg.Equals("/snapshotVolumes"))
        {
          Console.Write(SnapshotManager.CreateSnapshotAllVolumes());
          Console.Write(SnapshotManager.PurgeOldSnapshots());
        }

        if (arg.Equals("/startInstances"))
        {
          Console.Write(InstanceManager.StartInstances(Services.GetInstancesToStart()));
          Console.Write(InstanceManager.AssociateInstancesToElasticIps(Services.GetInstancesToStart()));
        }

        if (arg.Equals("/stopInstances"))
        {
          Console.Write(InstanceManager.StopInstances(Services.GetInstancesToStop()));
        }
      }

#if DEBUG
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
#endif
    }
  }
}
