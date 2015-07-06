using System;
using Fclp;

namespace AutoSnapper
{
  public class Program
  {
    public static void Main(string[] args) {
      var pa = new FluentCommandLineParser();
      var p = new FluentCommandLineParser<SnapperArguments> {IsCaseSensitive = false};
      p.Setup<bool>(x => x.DisplaySummary).As('v', "verbose").WithDescription("Test Description");
      p.SetupHelp("?", "help").Callback(Console.Write);
      var result = p.Parse(args);
      //display help and pause, or display errors and (if debug pause else exit) else run.
      if (result.HelpCalled || result.EmptyArgs) {
        p.HelpOption.ShowHelp(p.Options);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
      }
      else {
        if (result.HasErrors) {
          Console.WriteLine(result.ErrorText);
          p.HelpOption.ShowHelp(p.Options);
          pause();
        }
        else {
          run(p.Object);
          pause();
        }
      }
    }

    static void pause() {
#if DEBUG
      //stops the console so you can read the output for testing purposes
      Console.Write("press enter to continue...");
      Console.Read();
#endif
    }

    private static void SafeInvoke(Func<String> a) {
      try {
        Console.WriteLine(a());
      }
      catch (Exception ex) {
        
      }
    }

    private static void run(SnapperArguments args) {
      if (args.DisplaySummary)
        SafeInvoke(Services.GetServiceOutput);

        if (args.SnapshotVolumes) {
          SafeInvoke(SnapshotManager.CreateSnapshotAllVolumes);
          SafeInvoke(SnapshotManager.PurgeOldSnapshots);
        }

        if (args.StartInstances) {
          SafeInvoke(()=>
          {
            var instancesToStart = Services.GetInstancesToStart();

            Console.Write(InstanceManager.StartInstances(instancesToStart));
            Console.Write("Starting instances, please wait... ");

            //TODO: make this a threaded process, so we can sleep until the instances are ready

            //wait here until all the started instances are running, can't assign IPs until then
            while (!InstanceManager.InstancesAreRunning(instancesToStart)) Console.Write("... ");

            Console.Write("\r\nAssociating elastic ips....");

            Console.Write(InstanceManager.AssociateInstancesToElasticIps(instancesToStart));
            return "Done Starting.";
          });
        }

        if (args.StopInstances) 
          SafeInvoke(()=>InstanceManager.StopInstances(Services.GetInstancesToStop()));
      
    }
  }
}
