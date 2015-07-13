using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.S3;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoSnapper
{
  class Services
  {
    #region Public Methods
    /// <summary>
    /// returns the list of instances specified in the instancesToStop section in App.config
    /// </summary>
    /// <returns></returns>
    public static List<Instance> GetInstancesToStop()
    {
      var instances = ((RegisterInstancesConfig)ConfigurationManager.GetSection("RegisterInstances")).InstancesToStop.ToList();

      return instances;
    }

    /// <summary>
    /// returns the list of instances specified in the instancesToStart section in App.config
    /// </summary>
    /// <returns></returns>
    public static List<Instance> GetInstancesToStart()
    {
     var instances = ((RegisterInstancesConfig)ConfigurationManager.GetSection("RegisterInstances")).InstancesToStart.ToList();

      return instances;
    }

    /// <summary>
    /// returns a string containing EC2 Instance Count, Volume Details, Snapshot Details, Simble DB Instance Count and S3 Instance Count
    /// </summary>
    /// <returns></returns>
    public static string GetServiceOutput()
    {
      var sb = new StringBuilder();
      
      using (var sr = new StringWriter(sb))
      {
        sr.WriteLine("===========================================");
        sr.WriteLine("AWS Details");
        sr.WriteLine("===========================================");

        WriteEc2Count(sr);
        WriteVolumeDetails(sr);
        WriteSnapshotDetails(sr);
        WriteSimpleDbCount(sr);
        WriteS3Count(sr);
      }

      return sb.ToString();
    }

    /// <summary>
    /// returns a snapshot description for the owner specified in the SnapshotOwnerId in the App.config
    /// </summary>
    /// <returns></returns>
    public static DescribeSnapshotsResponse GetDescribeSnapshotsResponse()
    {
      var ec2 = GetEc2Client();

      var describeSnapshotReq = new DescribeSnapshotsRequest
                                  {
                                    OwnerIds = new List<string> { ConfigurationManager.AppSettings["SnapshotOwnerId"] }
                                  };

      return ec2.DescribeSnapshots(describeSnapshotReq);
    }

    /// <summary>
    /// gets an instance of an EC2 Client
    /// </summary>
    /// <returns></returns>
    public static IAmazonEC2 GetEc2Client()
    {
      var ec2 = AWSClientFactory.CreateAmazonEC2Client();

      return ec2;
    }

    /// <summary>
    /// gets an instance of a Simple DB Client
    /// </summary>
    /// <returns></returns>
    public static IAmazonSimpleDB GetSimpleDbClient()
    {
      var sdb = AWSClientFactory.CreateAmazonSimpleDBClient();

      return sdb;
    }

    /// <summary>
    /// gets an instance of an S3 Client
    /// </summary>
    /// <returns></returns>
    public static IAmazonS3 GetS3Client()
    {
      var s3Client = AWSClientFactory.CreateAmazonS3Client();

      return s3Client;
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// writes the count of current EC2 instances to sr
    /// </summary>
    /// <param name="sr"></param>
    private static void WriteEc2Count(TextWriter sr)
    {
      var ec2 = GetEc2Client();
      var ec2Request = new DescribeInstancesRequest();

      try
      {
        var ec2Response = ec2.DescribeInstances(ec2Request);

        var numInstances = ec2Response.Reservations.Count;

        sr.WriteLine("You have {0} Amazon EC2 instance(s) running in the {1} region.", numInstances,
                     ConfigurationManager.AppSettings["AWSRegion"]);
      }
      catch (AmazonEC2Exception ex)
      {
        if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
        {
          sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
          sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
        }
        else
        {
          sr.WriteLine("Caught Exception: " + ex.Message);
          sr.WriteLine("Response Status Code: " + ex.StatusCode);
          sr.WriteLine("Error Code: " + ex.ErrorCode);
          sr.WriteLine("Error Type: " + ex.ErrorType);
          sr.WriteLine("Request ID: " + ex.RequestId);
        }
      }

      sr.WriteLine();
    }

    /// <summary>
    /// writes a detailed description of current Volumes to sr
    /// </summary>
    /// <param name="sr"></param>
    private static void WriteVolumeDetails(TextWriter sr)
    {
      var ec2 = GetEc2Client();

      try
      {
        var describeVolumesResp = ec2.DescribeVolumes();

        sr.WriteLine("You have {0} volume(s) running in the {1} region.", describeVolumesResp.Volumes.Count,
                     ConfigurationManager.AppSettings["AWSRegion"]);

        foreach (var volume in describeVolumesResp.Volumes)
        {
          sr.WriteLine("VolumeId: {0} VolumeType: {1} State: {2} Size: {3}", volume.VolumeId, volume.VolumeType, volume.State, volume.Size);
        }
      }
      catch (AmazonEC2Exception ex)
      {
        if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
        {
          sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
          sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
        }
        else
        {
          sr.WriteLine("Caught Exception: " + ex.Message);
          sr.WriteLine("Response Status Code: " + ex.StatusCode);
          sr.WriteLine("Error Code: " + ex.ErrorCode);
          sr.WriteLine("Error Type: " + ex.ErrorType);
          sr.WriteLine("Request ID: " + ex.RequestId);
        }
      }

      sr.WriteLine();
    }

    /// <summary>
    /// writes a detailed description of current Snapshots to sr
    /// </summary>
    /// <param name="sr"></param>
    private static void WriteSnapshotDetails(TextWriter sr)
    {
      try
      {
        var describeSnapshotsResp = GetDescribeSnapshotsResponse();

        sr.WriteLine("You have {0} snapshot(s).", describeSnapshotsResp.Snapshots.Count);

        foreach (var snapshot in describeSnapshotsResp.Snapshots)
        {
          sr.WriteLine("VolumeId: {0} SnapshotId: {1}, StartTime: {2}, State: {3}, VolumeSize: {4}", snapshot.VolumeId,
                       snapshot.SnapshotId, snapshot.StartTime, snapshot.State,
                       snapshot.VolumeSize);
        }
      }
      catch (AmazonEC2Exception ex)
      {
        if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
        {
          sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
          sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
        }
        else
        {
          sr.WriteLine("Caught Exception: " + ex.Message);
          sr.WriteLine("Response Status Code: " + ex.StatusCode);
          sr.WriteLine("Error Code: " + ex.ErrorCode);
          sr.WriteLine("Error Type: " + ex.ErrorType);
          sr.WriteLine("Request ID: " + ex.RequestId);
        }
      }

      sr.WriteLine();
    }

    /// <summary>
    /// writes a count of current Simple DBs to sr
    /// </summary>
    /// <param name="sr"></param>
    private static void WriteSimpleDbCount(TextWriter sr)
    {
      var sdb = GetSimpleDbClient();
      var sdbRequest = new ListDomainsRequest();

      try
      {
        var sdbResponse = sdb.ListDomains(sdbRequest);
        var numDomains = sdbResponse.DomainNames.Count;

        sr.WriteLine("You have {0} Amazon SimpleDB domain(s) in the {1} region.", numDomains,
                     ConfigurationManager.AppSettings["AWSRegion"]);
      }
      catch (AmazonSimpleDBException ex)
      {
        if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
        {
          sr.WriteLine("The account you are using is not signed up for Amazon SimpleDB.");
          sr.WriteLine("You can sign up for Amazon SimpleDB at http://aws.amazon.com/simpledb");
        }
        else
        {
          sr.WriteLine("Caught Exception: " + ex.Message);
          sr.WriteLine("Response Status Code: " + ex.StatusCode);
          sr.WriteLine("Error Code: " + ex.ErrorCode);
          sr.WriteLine("Error Type: " + ex.ErrorType);
          sr.WriteLine("Request ID: " + ex.RequestId);
        }
      }

      sr.WriteLine();
    }

    /// <summary>
    /// writes a count of current S3 buckets to sr
    /// </summary>
    /// <param name="sr"></param>
    private static void WriteS3Count(TextWriter sr)
    {
      var s3Client = GetS3Client();

      try
      {
        var response = s3Client.ListBuckets();
        var numBuckets = 0;

        if (response.Buckets != null &&
            response.Buckets.Count > 0)
        {
          numBuckets = response.Buckets.Count;
        }

        sr.WriteLine("You have " + numBuckets + " Amazon S3 bucket(s).");
      }
      catch (AmazonS3Exception ex)
      {
        if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
            ex.ErrorCode.Equals("InvalidSecurity")))
        {
          sr.WriteLine("Please check the provided AWS Credentials.");
          sr.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
        }
        else
        {
          sr.WriteLine("Caught Exception: " + ex.Message);
          sr.WriteLine("Response Status Code: " + ex.StatusCode);
          sr.WriteLine("Error Code: " + ex.ErrorCode);
          sr.WriteLine("Request ID: " + ex.RequestId);
        }
      }
    }
    #endregion

  }
}
