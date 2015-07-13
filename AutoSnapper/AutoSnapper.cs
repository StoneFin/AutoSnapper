using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLAP;

namespace AutoSnapper
{
  public class AutoSnapper
  {
    [Verb(Aliases = "/DisplaySummary", Description = "List information for ec2, volumes, snapshots, simpleDB and s3")]
    public static void DisplaySummary() {
      SafeInvoke(Services.GetServiceOutput);
    }

    [Verb(Aliases = "/SnapshotVolumes", Description = "Take snapshots of all ebs volumes. Then purge all old snapshots according to config.")]
    public static void SnapshotVolumes() {
      SafeInvoke(SnapshotManager.CreateSnapshotAllVolumes);
      SafeInvoke(SnapshotManager.PurgeOldSnapshots);
    }

    [Verb(Aliases = "/StopInstances", Description = "Stop instances as defined in config.")]
    public static void StopInstances() {
      SafeInvoke(() => InstanceManager.StopInstances(Services.GetInstancesToStop()));
    }

    [Verb(Aliases = "/StartInstances", Description = "Start instances as defined in config.")]
    public static void StartInstances() {
      SafeInvoke(() => {
        var instancesToStart = Services.GetInstancesToStart();

        Console.Write(InstanceManager.StartInstances(instancesToStart));
        Console.Write("Starting instances, please wait... ");

        //TODO: make this a threaded process, so we can sleep until the instances are ready

        //wait here until all the started instances are running, can't assign IPs until then
        int count = 0;
        while (!InstanceManager.InstancesAreRunning(instancesToStart)) {
          if(count++ % 10 == 0)
            Console.WriteLine("...");
          Console.Write("...");
          Thread.Sleep(1000);
        }

        Console.WriteLine();
        Console.WriteLine("Instances started, Associating Elastic IPs.");
        Console.Write(InstanceManager.AssociateInstancesToElasticIps(instancesToStart));
        return "Done Starting.";
      });
    }
    static void pauseForReal() {
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
    }
    [Empty, Help]
    public static void Help(string help) {
      // this is an empty handler that prints
      // the automatic help string to the console.
      Console.WriteLine("Usage: AutoSnapper.exe /arg1 [/arg2...]");
      Console.WriteLine(help);
      pauseForReal();
    }
    private static void SafeInvoke(Func<String> a) {
      try {
        Console.WriteLine(a());
      }
      catch (Exception ex) {

      }
    }
  }
}
