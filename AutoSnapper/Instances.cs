using System.Collections.Generic;
using System.Configuration;

namespace AutoSnapper
{
  public class Instances : ConfigurationElementCollection, IEnumerable<Instance>
  {
    public Instance this[int index]
    {
      get { return base.BaseGet(index) as Instance; }
      set
      {
        if (base.BaseGet(index) != null)
        {
          base.BaseRemoveAt(index);
        }

        this.BaseAdd(index, value);
      }
    }

    public new Instance this[string responseString]
    {
      get { return (Instance)BaseGet(responseString); }
      set
      {
        if (BaseGet(responseString) != null)
        {
          BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
        }

        BaseAdd(value);
      }
    }

    protected override ConfigurationElement CreateNewElement()
    {
      return new Instance();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((Instance)element).InstanceId;
    }

    public new IEnumerator<Instance> GetEnumerator()
    {
      var count = base.Count;

      for (int i = 0; i < count; i++)
      {
        yield return base.BaseGet(i) as Instance;
      }
    }
  }
}
