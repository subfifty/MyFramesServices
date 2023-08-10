using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace C4B.Atlas.Integration
{
    [DataContract]
    [Flags]
    public enum TelephoneStateFlags
    {
        /// <summary>
        /// Zustand Unbekannt, z.B. Leitung zugewiesen aber diese ist nicht richtig Konfiguriert.
        /// </summary>
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// Benutzer hat keine Leitung (Keine auf die man Zugriff hätte).
        /// </summary>
        [EnumMember]
        NotExisting = 1,
        /// <summary>
        /// Keine aktiven Anrufe.
        /// </summary>
        [EnumMember]
        Free = 2,
        /// <summary>
        /// Es gibt verbundene oder gehende Anrufe.
        /// </summary>
        [EnumMember]
        Busy = 4,
        /// <summary>
        /// Es gibt verbundene oder gehende Anrufe, mind. einer davon ist extern.
        /// BusyExternal ist immer zusätzlich zu Busy gesetzt. 
        /// </summary>
        [EnumMember]
        BusyExternal = 8,
        /// <summary>
        /// Es gibt eingehende signalisierte Anrufe.
        /// </summary>
        [EnumMember]
        Ringing = 16,
        /// <summary>
        /// Sigalisierte Anrufe auf mehreren Leitungen.
        /// Zustäzlich zu Ringing gesetzt.
        /// </summary>
        [EnumMember]
        RingingMultiple = 32,
        /// <summary>
        /// Es gibt externe signalisierte Anrufe.
        /// Zusätzlich zu Ringing gesetz. 
        /// </summary>
        [EnumMember]
        RingingExternal = 64,
        /// <summary>
        /// Es gibt eine Leitung auf der eine Umleitung gesetzt ist.
        /// </summary>
        [EnumMember]
        Forward = 256,
        /// <summary>
        /// Es gibt eine Leitung mit Umleitung zur Mailbox.
        /// Zusätzlich zu Forward gesetzt.
        /// </summary>
        [EnumMember]
        ForwardMailbox = 512,
        /// <summary>
        /// Es gibt eine Leitung mit DND.
        /// </summary>
        [EnumMember]
        DoNotDisturb = 1024,
        /// <summary>
        /// Auf der aktuellen Hauptleitung des User ist DND gesetzt.
        /// Zusätzlich zu DoNotDisturb gesetzt.
        /// </summary>
        [EnumMember]
        MainlineDoNotDisturb = 2048,
        /// <summary>
        /// Auf der aktuellen Hauptleitung des Users ist eine Umleitung gesetzt. 
        /// Zusätzlich zu Forward gesetzt.
        /// </summary>
        [EnumMember]
        MainlineForward = 4096,
    }
}
