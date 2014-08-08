using System.Configuration;

namespace AutoSnapper
{
  public class RegisterInstancesConfig : ConfigurationSection
  {
    public static RegisterInstancesConfig GetConfig()
    {
      return (RegisterInstancesConfig)ConfigurationManager.GetSection("RegisterInstances") ?? new RegisterInstancesConfig();
    }

    [ConfigurationProperty("InstancesToStart")]
    [ConfigurationCollection(typeof(Instances), AddItemName = "Instance")]
    public Instances InstancesToStart
    {
      get
      {
        object o = this["InstancesToStart"];
        return o as Instances;
      }
    }

    [ConfigurationProperty("InstancesToStop")]
    [ConfigurationCollection(typeof(Instances), AddItemName = "Instance")]
    public Instances InstancesToStop
    {
      get
      {
        object o = this["InstancesToStop"];
        return o as Instances;
      }
    }
  }
}
