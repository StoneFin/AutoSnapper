using System.Configuration;

namespace AutoSnapper
{
  public class Instance : ConfigurationElement
  {
    [ConfigurationProperty("instanceId", IsRequired = true)]
    public string InstanceId
    {
      get { return this["instanceId"] as string; }
    }

    [ConfigurationProperty("elasticIp", IsRequired = false)]
    public string ElasticIp
    {
      get { return this["elasticIp"] as string; }
    }
  }
}
