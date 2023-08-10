using System.Runtime.InteropServices;
using System.Text;

namespace XPhoneRestApi
{
    #region class IniFile
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        private string m_path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="a_path"></PARAM>
        public IniFile(string a_path)
        {
            m_path = a_path;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public long WriteString(string Section, string Key, string Value)
        {
            long ret = WritePrivateProfileString(Section, Key, Value, m_path);
            WritePrivateProfileString(null, null, null, null);
            return ret;
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <returns></returns>
        public string ReadString(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.m_path);
            return temp.ToString();
        }

        public bool IsValid(string clip)
        {
            if (ReadString("Filter", "Blacklist") == "Enable")
                if (ReadString("Filter", clip) == "Blacklist")
                    return false;

            if (ReadString("Filter", "Whitelist") == "Enable")
                if (ReadString("Filter", clip) != "Whitelist")
                    return false;

            return true;
        }
    }
    #endregion

}
