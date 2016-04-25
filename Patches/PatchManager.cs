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

        public static async void ApplyPatches()
        {
            Assembly assembly = Assembly.Load(new AssemblyName("Patches"));

            IEnumerable<Type> types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPatch)));
            int currentVersion = 0;
            bool failed = false;
            int index = 0;

            KeyValuePair<string, object> settingValue = ApplicationData.Current.LocalSettings.Values.FirstOrDefault(v => v.Key == VERSION_KEY);

            if (settingValue.Key != null)
            {
                int.TryParse(settingValue.Value.ToString(), out currentVersion);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[VERSION_KEY] = currentVersion;
            }

            while (!failed && index < types.Count())
            {
                Type type = types.ElementAt(index);

                string[] parts = type.Name.Split('_');
                int version = int.Parse(parts[parts.Length - 2]);

                if (version > currentVersion)
                {
                    IPatch patch = (IPatch)Activator.CreateInstance(type);

                    failed = await patch.Apply();

                    failed = !failed;

                    if (!failed)
                    {
                        currentVersion = version;

                        ApplicationData.Current.LocalSettings.Values[VERSION_KEY] = currentVersion;
                    }
                }

                index++;
            }

            if (failed)
            {
                // TODO: Add logging and notification to user.
            }
        }
    }
}
