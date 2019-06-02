using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace Settings
{
    public class SettingsManager
    {
        private static DefaultValueManager defaultValueManager = new DefaultValueManager();

        public static T Get<T>(Setting setting, bool deserialize = false)
        {
            KeyValuePair<string, object> settingValue = ApplicationData.Current.LocalSettings.Values.FirstOrDefault(v => v.Key == setting.ToString());
            object value = null;

            if (settingValue.Key == null)
            {
                value = defaultValueManager.GetDefaultValue(setting);
            }
            else
            {
                value = settingValue.Value;

                if (deserialize)
                {
                    value = JsonConvert.DeserializeObject<T>((string)value);
                }
            }

            return (T)value;
        }

        public static void Save(Setting setting, object value, bool serialize = false)
        {
            if (serialize)
            {
                value = JsonConvert.SerializeObject(value);
            }

            ApplicationData.Current.LocalSettings.Values[setting.ToString()] = value;
        }
    }
}
