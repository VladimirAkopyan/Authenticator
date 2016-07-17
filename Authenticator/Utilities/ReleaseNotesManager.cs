using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;

namespace Authenticator_for_Windows.Utilities
{
    class ReleaseNotesManager
    {
        public static bool ShowReleaseNotes
        {
            get
            {
                bool result = false;

                if (Properties.SHOW_RELEASENOTES)
                {
                    string lastKnownVersion = SettingsManager.Get<string>(Setting.LastKnownVersionNumber);
                    result = lastKnownVersion != AppVersion;
                }

                return result;
            }
        }

        private static string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;

                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public static void ShownReleaseNotes()
        {
            SettingsManager.Save(Setting.LastKnownVersionNumber, AppVersion);
        }
    }
}
