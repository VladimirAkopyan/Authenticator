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
            }

            return defaultValue;
        }
    }
}
