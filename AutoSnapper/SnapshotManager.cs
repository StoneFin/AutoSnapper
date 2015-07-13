using System.Configuration;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.EC2;
using NLog;

namespace AutoSnapper
{
  class SnapshotManager {
    static SnapshotManager() {
      Log = LogManager.GetCurrentClassLogger();
    }
    public static Logger Log { get; set; }
    #region Public Methods
    /// <summary>
    /// creates a Snapshot for each Volume
    /// </summary>
    /// <returns></returns>
    public static string CreateSnapshotAllVolumes()
    {
      var sb = new StringBuilder();

      using (var sr = new StringWriter(sb))
      {
        sr.WriteLine("===========================================");
        sr.WriteLine("Creating Snapshots for All Volumes");
        sr.WriteLine("===========================================");

        var volumeList = VolumeManager.GetVolumes();
        Log.Trace("volume count: " + volumeList.Count);
        var ec2 = Services.GetEc2Client();

        foreach (var volume in volumeList)
        {
          var instanceIdList = volume.Attachments.Select(x => x.InstanceId).ToList();

          instanceIdList = (instanceIdList.Count == 0) ? new List<string> { "None" } : instanceIdList;

          var snapshotDescription = string.Format("Snapshot By Stone Fin AutoSnapper, VolumeId: {0}, Instance(s): {1} ",
                                                  volume.VolumeId, string.Join(",", instanceIdList));

          sr.WriteLine(snapshotDescription);
          CreateSnapshot(volume, snapshotDescription, ec2);
        }
      }
      var outputMsg = sb.ToString();
      Log.Trace(outputMsg);
      return outputMsg;
    }

    /// <summary>
    /// deletes Snapshots older than the SnapshotExpiration setting in the config
    /// </summary>
    /// <returns></returns>
    public static string PurgeOldSnapshots()
    {
      var sb = new StringBuilder();
      var expDays = Convert.ToInt32(ConfigurationManager.AppSettings["SnapshotExpiration"]);
      using (var sr = new StringWriter(sb))
      {
        if (expDays != 0)
        {
          var expDate = DateTime.Now.AddDays(-1*expDays);

          sr.WriteLine("===========================================");
          sr.WriteLine("Purging Snapshots for all Volumes older than {0}", expDate);
          sr.WriteLine("===========================================");

          var snapshotExpiredList = GetSnapshots(expDate,
                                                 new Tag
                                                   {
                                                     Key = ConfigurationManager.AppSettings["SnapshotTagKey"],
                                                     Value = ConfigurationManager.AppSettings["SnapshotTagValue"]
                                                   });

          if (snapshotExpiredList.Any())
          {
            var ec2 = Services.GetEc2Client();

            foreach (var snapshot in snapshotExpiredList)
            {
              sr.WriteLine("Deleting Snapshot, SnapshotId: {0}, VolumeId: {1}, StartTime: {2}", snapshot.SnapshotId,
                           snapshot.VolumeId, snapshot.StartTime);


              var deleteSnapshotRequest = new DeleteSnapshotRequest
                                            {
                                              SnapshotId = snapshot.SnapshotId
                                            };

              try {
                ec2.DeleteSnapshot(deleteSnapshotRequest);
              }
              catch (Exception ex) {
                var msg = String.Format("Failed to delete snapshot {0}, failed with exception {1}", snapshot.SnapshotId,
                  ex.Message);
                sr.WriteLine(msg);
                Log.Error(ex, msg);
              }
            }
          }
          else
          {
            sr.WriteLine("===========================================");
            sr.WriteLine("No expired Snapshots found.");
            sr.WriteLine("===========================================");
          }
        }
        else
        {
          sr.WriteLine("===========================================");
          sr.WriteLine("No Snapshot expiration set in .config. Skipping Snapshot purge.");
          sr.WriteLine("===========================================");
        }
      }
      var outputText = sb.ToString();
      Log.Trace(outputText);
      return outputText;
    }

    /// <summary>
    /// creates a snapshot for the given volume, with the given description
    /// </summary>
    /// <param name="v"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static Snapshot CreateSnapshot(Volume v, string description, IAmazonEC2 ec2)
    {
      
      var snapReq = new CreateSnapshotRequest
                      {
                        VolumeId = v.VolumeId,
                        Description = description
                      };

      Snapshot snapshot;
      try {
        var snapResp = ec2.CreateSnapshot(snapReq);
        snapshot = snapResp.Snapshot;
      }
      catch (Exception ex) {
        Log.Error(ex,"Error creating snapshot for volume " + v.VolumeId);
        return null;
      }

      if (string.IsNullOrEmpty(description)) 
        return snapshot;
      var resourceList = new List<string>
      {
        snapshot.SnapshotId
      };

      var tagDescription = new Tag
      {
        Key = "Description",
        Value = description
      };

      var tagAutoSnapper = new Tag
      {
        Key = ConfigurationManager.AppSettings["SnapshotTagKey"],
        Value = ConfigurationManager.AppSettings["SnapshotTagValue"]
      };

      var tagList = new List<Tag>
      {
        tagDescription,
        tagAutoSnapper
      };

      var createTagsReq = new CreateTagsRequest
      {
        Resources = resourceList,
        Tags = tagList
      };

      try {
        ec2.CreateTags(createTagsReq);
      }
      catch (Exception ex) {
        Log.Error(ex,"Error creating tags for snapshot " + snapshot.SnapshotId);
      }
      return snapshot;
    }

    /// <summary>
    /// lists all Snapshots for the SnapshotOwnerId in the config
    /// </summary>
    /// <returns></returns>
    public static List<Snapshot> GetSnapshots()
    {
      var describeSnapshotsResponse = Services.GetDescribeSnapshotsResponse();
      var snapshots = describeSnapshotsResponse.Snapshots;

      return snapshots;
    }

    /// <summary>
    /// lists all Snapshots for the SnapshotOwnerId in the config that are older than given expDate
    /// </summary>
    /// <param name="expDate"></param>
    /// <returns></returns>
    public static List<Snapshot> GetSnapshots(DateTime expDate)
    {
      var snapshots = GetSnapshots().Where(x => x.StartTime < expDate).ToList();

      return snapshots;
    }

    /// <summary>
    /// lists all snapshots for the SnapshotOwnerId in the config that contain the given tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static List<Snapshot> GetSnapshots(Tag tag)
    {
      var snapshots = GetSnapshots().Where(x => x.Tags.Any(y => y.Key == tag.Key && y.Value == tag.Value)).ToList();

      return snapshots;
    }

    /// <summary>
    /// lists all snapshots for the SnapshotOwnerId in the config that are both order than the given expDate
    /// and contain the given tag
    /// </summary>
    /// <param name="expDate"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static List<Snapshot> GetSnapshots(DateTime expDate, Tag tag)
    {
      var snapshots =
        GetSnapshots()
          .Where(x => x.StartTime < expDate && x.Tags.Any(y => y.Key == tag.Key && y.Value == tag.Value))
          .ToList();

      return snapshots;
    }
    #endregion
  }
}
