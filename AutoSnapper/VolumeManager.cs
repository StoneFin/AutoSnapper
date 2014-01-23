using Amazon.EC2.Model;
using System.Collections.Generic;

namespace AutoSnapper
{
  class VolumeManager
  {
    /// <summary>
    /// gets a list of current Volumes
    /// </summary>
    /// <returns></returns>
    public static List<Volume> GetVolumes()
    {
      var ec2 = Services.GetEc2Client();
      var describeVolumesResp = ec2.DescribeVolumes();

      return describeVolumesResp.Volumes;
    }
  }
}
