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
          var instancesToStart = Services.GetInstancesToStart();

          Console.Write(InstanceManager.StartInstances(instancesToStart));
          Console.Write("Starting instances, please wait... ");

          //TODO: make this a threaded process, so we can sleep until the instances are ready

          //wait here until all the started instances are running, can't assign IPs until then
          while (!InstanceManager.InstancesAreRunning(instancesToStart))
          {
            Console.Write("... ");
          }

          Console.Write("\r\n");

          Console.Write(InstanceManager.AssociateInstancesToElasticIps(instancesToStart));
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
