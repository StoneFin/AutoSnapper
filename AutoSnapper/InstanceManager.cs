using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoSnapper
{
  class InstanceManager
  {
    #region Public Methods
    /// <summary>
    /// stops all instances contained in the collection
    /// </summary>
    /// <param name="instances"></param>
    /// <returns></returns>
    public static string StopInstances(List<Instance> instances)
    {
      var instanceIds = new List<string>();
      
      foreach (var instance in instances)
      {
        instanceIds.Add(instance.InstanceId);
      }

      var sb = new StringBuilder();
      var instanceIdCsv = String.Join(", ", instanceIds);

      using (var sr = new StringWriter(sb))
      {
        sr.WriteLine("===========================================");
        sr.WriteLine(string.Format("Stopping EC2 Instance(s): {0}", instanceIdCsv));
        sr.WriteLine("===========================================");

        var stopInstancesRequest = new StopInstancesRequest()
        {
          InstanceIds = instanceIds,
          Force = false
        };

        var ec2Client = Services.GetEc2Client();

        ec2Client.StopInstances(stopInstancesRequest);
      }

      return sb.ToString();
    }

    /// <summary>
    /// starts all instances contained in the collection
    /// </summary>
    /// <param name="instances"></param>
    /// <param name="associateIPs"></param>
    /// <returns></returns>
    public static string StartInstances(List<Instance> instances)
    {
      var instanceIds = new List<string>();
      
      foreach (var instance in instances)
      {
        instanceIds.Add(instance.InstanceId);
      }

      var sb = new StringBuilder();
      
      using (var sr = new StringWriter(sb))
      {
        var instanceIdCsv = String.Join(", ", instanceIds);

        sr.WriteLine("===========================================");
        sr.WriteLine(string.Format("Starting EC2 Instance(s): {0}", instanceIdCsv));
        sr.WriteLine("===========================================");

        var startInstancesRequest = new StartInstancesRequest()
        {
          InstanceIds = instanceIds
        };

        var ec2Client = Services.GetEc2Client();

        ec2Client.StartInstances(startInstancesRequest);
      }

      return sb.ToString();
    }

    /// <summary>
    /// associates instances with their ElasticIPs
    /// </summary>
    /// <param name="instances"></param>
    /// <returns></returns>
    public static string AssociateInstancesToElasticIps(List<Instance> instances)
    {
      var instanceIpPairs = new List<string>();
      var associateAddressRequests = new List<AssociateAddressRequest>();

      foreach (var instance in instances)
      {
        if (!string.IsNullOrEmpty(instance.ElasticIp))
        {
          instanceIpPairs.Add(string.Format("{0}::{1}", instance.InstanceId, instance.ElasticIp));
          associateAddressRequests.Add(new AssociateAddressRequest() { InstanceId = instance.InstanceId, PublicIp = instance.ElasticIp });
        }
      }

      var sb = new StringBuilder();

      using (var sr = new StringWriter(sb))
      {
        var instanceIpPairCsv = String.Join(", ", instanceIpPairs);

        sr.WriteLine("===========================================");
        sr.WriteLine(string.Format("Associating EC2 Instance(s) to ElasticIP(s): {0}", instanceIpPairCsv));
        sr.WriteLine("===========================================");

        var ec2Client = Services.GetEc2Client();

        foreach (var aar in associateAddressRequests)
        {
          ec2Client.AssociateAddress(aar);
        }
      }

      return sb.ToString();
    }
    
    /// <summary>
    /// returns true when all instances are running
    /// </summary>
    /// <param name="instances"></param>
    /// <returns></returns>
    public static bool InstancesAreRunning(List<Instance> instances)
    {
      var instanceIds = new List<string>();
      
      foreach (var instance in instances)
      {
        instanceIds.Add(instance.InstanceId);
      }

      var ec2Request = new DescribeInstanceStatusRequest()
      {
        InstanceIds = instanceIds
      };

      var ec2Client = Services.GetEc2Client();
      var statusResult = ec2Client.DescribeInstanceStatus(ec2Request);

      if (statusResult.InstanceStatuses.Count == 0)
      {
        //no instances found; must not be running yet
        return false;
      }

      foreach (var instanceStatus in statusResult.InstanceStatuses)
      {
        if (!instanceStatus.InstanceState.Name.Value.Equals("running"))
        {
          return false;
        }
      }

      return true;
    }
    #endregion
  }
}
