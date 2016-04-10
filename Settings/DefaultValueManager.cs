using System;
using System.Collections.Generic;

namespace Settings
{
    class DefaultValueManager
    {
        public object GetDefaultValue(Setting setting)
        {
            object defaultValue = null;

            switch (setting)
            {
                case Setting.ClipBoardRememberType:
                    defaultValue = 0;
                    break;
                case Setting.UseNTP:
                    defaultValue = true;
                    break;
                case Setting.NTPTimeout:
                    defaultValue = new TimeSpan(0, 0, 2);
                    break;
            }

            return defaultValue;
        }
    }
}
