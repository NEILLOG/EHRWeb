using System.Collections.ObjectModel;

namespace BASE.Models
{
    public static class CommonParameters
    {
        /// <summary>
        /// 潛在危險的附檔名
        /// </summary>
        public static readonly IList<string> dangerous_ext = new ReadOnlyCollection<string>
        (
            new List<string> { "exe", "pif", "application", "gadget", "msi", "msp", "com", "scr", "hta", "cpl", "msc", "jar", "bat", "cmd", "vb", "vbs", "vbe", "js", "jse", "ws", "wsf", "wsc", "wsh", "ps1", "ps1xml", "ps2", "ps2xml", "psc1", "psc2", "msh", "msh1", "msh2", "mshxml", "msh1xml", "msh2xml", "scf", "lnk", "inf", "reg" }
        );

    }
}
