using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static string AssociateElasticIps(List<Instance> instances)
    {
      //look at the instances we are going to associate
      //assign them to the desired elastic ip if they aren't already associated
      using (var ec2Client = Services.GetEc2Client())
      {
        var describeInstancesRequest = new DescribeInstancesRequest
        {
          InstanceIds = instances.Select(x => x.InstanceId).ToList()
        };

        //description of all instances
        var describeInstanceResponse = ec2Client.DescribeInstances(describeInstancesRequest);

        //description of all addresses
        var describeAddressResponses = ec2Client.DescribeAddresses().Addresses;

        //return value string InstanceId::ElasticIp format
        var associatedInstanceIpPairs = new List<string>();

        //list of address association requests
        var associateAddressRequests = new List<AssociateAddressRequest>();

        foreach (var reservation in describeInstanceResponse.Reservations)
        {
          foreach (var reservationInstance in reservation.Instances)
          {
            var instance = instances
              .SingleOrDefault(x => x.InstanceId.Equals(reservationInstance.InstanceId));

            if (instance != null && !instance.ElasticIp.Equals(reservationInstance.PublicIpAddress))
            {
              //we have a matching instance and it is not assigned to the desired elastic ip
              associatedInstanceIpPairs.Add(string.Format("{0}::{1}", instance.InstanceId, instance.ElasticIp));

              var allocationId = describeAddressResponses
                .SingleOrDefault(x => x.PublicIp.Equals(instance.ElasticIp))
                .AllocationId;

              if (!string.IsNullOrEmpty(allocationId))
              {
                var associatedAddressRequest = new AssociateAddressRequest
                {
                  AllocationId = allocationId,
                  InstanceId = instance.InstanceId
                };

                associateAddressRequests.Add(associatedAddressRequest);
              }
            }
          }
        }

        //now that we have a set of associations to make, build the output and make the associations
        var sb = new StringBuilder();

        using (var sr = new StringWriter(sb))
        {
          if (associateAddressRequests.Any())
          {
            var instanceIpPairCsv = String.Join(", ", associatedInstanceIpPairs);

            sr.WriteLine("===========================================");
            sr.WriteLine(string.Format("Associating EC2 Instance(s) to ElasticIP(s): {0}", instanceIpPairCsv));
            sr.WriteLine("===========================================");

            foreach (var aar in associateAddressRequests)
            {
              ec2Client.AssociateAddress(aar);
            }
          }
          else
          {
            sr.WriteLine("===========================================");
            sr.WriteLine("No Instances found to associate with ElasticIPs.");
            sr.WriteLine("===========================================");
          }
        }

        return sb.ToString();
      }
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
      var instanceStatuses = ec2Client.DescribeInstanceStatus(ec2Request).InstanceStatuses;
      
      if (instanceStatuses.Count == 0)
      {
        //no instances found; must not be running yet
        return false;
      }

      foreach (var instanceStatus in instanceStatuses)
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
