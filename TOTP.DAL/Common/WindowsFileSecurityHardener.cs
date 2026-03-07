using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace TOTP.DAL.Common;

internal static class WindowsFileSecurityHardener
{
    public static void RestrictDirectoryToCurrentUser(string directoryPath)
    {
        if (!OperatingSystem.IsWindows() || !Directory.Exists(directoryPath))
        {
            return;
        }

        var userSid = WindowsIdentity.GetCurrent().User;
        if (userSid == null)
        {
            return;
        }

        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.SetOwner(userSid);
        security.AddAccessRule(new FileSystemAccessRule(
            userSid,
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow));

        new DirectoryInfo(directoryPath).SetAccessControl(security);
    }

    public static void RestrictFileToCurrentUser(string filePath)
    {
        if (!OperatingSystem.IsWindows() || !File.Exists(filePath))
        {
            return;
        }

        var userSid = WindowsIdentity.GetCurrent().User;
        if (userSid == null)
        {
            return;
        }

        var security = new FileSecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.SetOwner(userSid);
        security.AddAccessRule(new FileSystemAccessRule(
            userSid,
            FileSystemRights.FullControl,
            InheritanceFlags.None,
            PropagationFlags.None,
            AccessControlType.Allow));

        new FileInfo(filePath).SetAccessControl(security);
    }
}
