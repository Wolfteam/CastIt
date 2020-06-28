using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;
using System.Linq;

namespace CastIt.Setup.CustomActions
{
    public static class CustomActions
    {
        [CustomAction]
        public static ActionResult DeleteUserData(Session session)
        {
            try
            {
                var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var fullPath = Path.Combine(baseFolder, "CastIt");

                Directory.Delete(fullPath, true);
            }
            catch (Exception)
            {
            }

            return ActionResult.Success;
        }
    }
}
