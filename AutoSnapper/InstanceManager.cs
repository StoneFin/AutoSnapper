using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.S3;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;


namespace AutoSnapper
{
  class InstanceManager
  {
    /// <summary>
    /// stops instances whos InstanceIds are contained in instanceIds
    /// </summary>
    /// <param name="instanceIds"></param>
    /// <returns></returns>
    public static string StopInstances(List<string> instanceIds)
    {
      var sb = new StringBuilder();

      using (var sr = new StringWriter(sb))
      {
        var instanceCsv = String.Join(", ", instanceIds);

        sr.WriteLine("===========================================");
        sr.WriteLine(string.Format("Stopping EC2 Instances: {0}", instanceCsv));
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
  }
}
