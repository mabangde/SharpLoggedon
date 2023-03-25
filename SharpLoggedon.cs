using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Text;
using Microsoft.Win32.SafeHandles;

class SharpLoggedon
{
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ConvertStringSidToSid(string sidString, out IntPtr sid);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, char[] lpName, ref uint cchName, char[] ReferencedDomainName, ref uint cchReferencedDomainName, out int peUse);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int RegQueryInfoKey(
        SafeRegistryHandle hKey,
        [Out] StringBuilder lpClass,
        [In][Out] ref uint lpcbClass,
        IntPtr lpReserved,
        out uint lpcSubKeys,
        [Out] StringBuilder lpMaxSubKeyLen,
        [Out] StringBuilder lpMaxClassLen,
        out uint lpcValues,
        [Out] StringBuilder lpMaxValueNameLen,
        [Out] StringBuilder lpMaxValueLen,
        [Out] StringBuilder lpSecurityDescriptor,
        out long lpftLastWriteTime);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegConnectRegistry(string machineName, IntPtr hKey, out IntPtr phkResult);


    static DateTime GetRegistryKeyLastWriteTime(string machineName, RegistryHive hive, string subKey)
    {
        using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(hive, machineName).OpenSubKey(subKey))
        {
            StringBuilder lpClass = new StringBuilder();
            uint lpcbClass = 0;
            StringBuilder lpMaxSubKeyLen = new StringBuilder();
            StringBuilder lpMaxClassLen = new StringBuilder();
            uint lpcValues = 0;
            StringBuilder lpMaxValueNameLen = new StringBuilder();
            StringBuilder lpMaxValueLen = new StringBuilder();
            StringBuilder lpSecurityDescriptor = new StringBuilder();
            long lpftLastWriteTime;
            int result = RegQueryInfoKey(key.Handle, lpClass, ref lpcbClass, IntPtr.Zero, out uint lpcSubKeys, lpMaxSubKeyLen, lpMaxClassLen, out lpcValues, lpMaxValueNameLen, lpMaxValueLen, lpSecurityDescriptor, out lpftLastWriteTime);
            if (result == 0)
            {
                return DateTime.FromFileTime(lpftLastWriteTime);
            }
            else
            {
                throw new Exception("Error getting last write time: " + result);
            }
        }
    }
    public static void GetLoggedOnUsers(string machineName)
    {
        Console.WriteLine("Users logged on:"+ machineName);
        RegistryKey baseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, machineName);

        foreach (string keyname in baseKey.GetSubKeyNames())
        {
            if (keyname.StartsWith("S-1-5-") && !keyname.EndsWith("_Classes"))
            {
                try
                {
                    IntPtr sid_ptr;
                    ConvertStringSidToSid(keyname, out sid_ptr);

                    uint cchName = 0, cchReferencedDomainName = 0;
                    int peUse = 0;
                    LookupAccountSid(machineName, sid_ptr, null, ref cchName, null, ref cchReferencedDomainName, out peUse);

                    char[] name = new char[cchName];
                    char[] referencedDomainName = new char[cchReferencedDomainName];

                    if (LookupAccountSid(machineName, sid_ptr, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out peUse))
                    {
                        string username = new string(name).Substring(0, (int)cchName);
                        string domainname = new string(referencedDomainName).Substring(0, (int)cchReferencedDomainName);
                        if (!domainname.ToLower().Contains("NT AUTHORITY".ToLower()))
                        {
                            
                            RegistryKey envKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, machineName).OpenSubKey(keyname + @"\Environment");
                            if (envKey != null)
                            {
                                
                                RegistryHive hive = RegistryHive.Users;
                                string subKey = keyname+ @"\Environment";
                                string valueName = "Path";

                                DateTime lastWriteTime = GetRegistryKeyLastWriteTime(machineName, hive, subKey);
                                Console.WriteLine("Name: " + domainname + "\\" + username + "  logged: " + lastWriteTime);
                                envKey.Close();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("LookupAccountSid failed: " + Marshal.GetLastWin32Error());
                    }

                    Marshal.FreeHGlobal(sid_ptr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SharpLoggedon <remote_computer_name>");
            return;
        }

        string remote_computer_name = args[0];

        
        GetLoggedOnUsers(remote_computer_name);
    }
}
   
