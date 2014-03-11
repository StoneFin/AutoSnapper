using System.Configuration;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoSnapper
{
  class SnapshotManager
  {
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

        foreach (var volume in volumeList)
        {
          var instanceIdList = volume.Attachments.Select(x => x.InstanceId).ToList();

          instanceIdList = (instanceIdList.Count == 0) ? new List<string> { "None" } : instanceIdList;

          var snapshotDescription = string.Format("Snapshot By Stone Fin AutoSnapper, VolumeId: {0}, Instance(s): {1} ",
                                                  volume.VolumeId, string.Join(",", instanceIdList));

          sr.WriteLine(snapshotDescription);
          CreateSnapshot(volume, snapshotDescription);
        }
      }

      return sb.ToString();
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
            foreach (var snapshot in snapshotExpiredList)
            {
              sr.WriteLine("Deleting Snapshot, SnapshotId: {0}, VolumeId: {1}, StartTime: {2}", snapshot.SnapshotId,
                           snapshot.VolumeId, snapshot.StartTime);

              var ec2 = Services.GetEc2Client();

              var deleteSnapshotRequest = new DeleteSnapshotRequest
                                            {
                                              SnapshotId = snapshot.SnapshotId
                                            };

              ec2.DeleteSnapshot(deleteSnapshotRequest);
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

      return sb.ToString();
    }

    /// <summary>
    /// creates a snapshot for the given volume, with the given description
    /// </summary>
    /// <param name="v"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static Snapshot CreateSnapshot(Volume v, string description)
    {
      var ec2 = Services.GetEc2Client();
      
      var snapReq = new CreateSnapshotRequest
                      {
                        VolumeId = v.VolumeId,
                        Description = description
                      };

      var snapResp = ec2.CreateSnapshot(snapReq);
      var snapshot = snapResp.Snapshot;

      if (!string.IsNullOrEmpty(description))
      {
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

        ec2.CreateTags(createTagsReq);
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
