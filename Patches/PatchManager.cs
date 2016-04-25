using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Patches
{
    public abstract class PatchManager
    {
        private const string VERSION_KEY = "Version";

        public static void ApplyPatches()
        {
            Assembly assembly = Assembly.Load(new AssemblyName("Patches"));

            IEnumerable<Type> types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPatch)));
            int currentVersion = 0;

            KeyValuePair<string, object> settingValue = ApplicationData.Current.LocalSettings.Values.FirstOrDefault(v => v.Key == VERSION_KEY);

            if (settingValue.Key != null)
            {
                int.TryParse(settingValue.Value.ToString(), out currentVersion);
            }

            foreach (Type type in types)
            {
                string[] parts = type.Name.Split('_');
                int version = int.Parse(parts[parts.Length - 2]);

                if (version > currentVersion)
                {
                    IPatch patch = (IPatch)Activator.CreateInstance(type);

                    bool result = patch.Apply();

                    if (result)
                    {
                        currentVersion = version;

                        ApplicationData.Current.LocalSettings.Values[VERSION_KEY] = currentVersion;
                    }
                }
            }
        }
    }
}
