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
    public enum TeamDeskAgentState
    {
        [EnumMember]
        Unknown = 0,

        /// <summary>
        /// Agent ist angemeldet. Agent ist nach Beendigung eines Anrufs sofort wieder verfügbar
        /// </summary>
        [EnumMember]
        Available,

        /// <summary>
        /// Agent ist angemeldet. Agent ist nach einem Anruf automatisch in Bearbeitungszeit
        /// </summary>
        [EnumMember]
        AvailableWithWrapUp,

        /// <summary>
        /// Agent ist ausgelogged
        /// </summary>
        [EnumMember]
        LoggedOut,

        /// <summary>
        /// Agent befindet sich in Pause
        /// </summary>
        [EnumMember]
        OnBreak,

        /// <summary>
        /// Agent befindet sich in Nachbearbeitungszeit
        /// </summary>
        [EnumMember]
        WrapUp,

        /// <summary>
        /// Agent empfängt einen Queue Anruf
        /// </summary>
        [EnumMember]
        Receiving,

        /// <summary>
        /// Agent befindet sich in einem Queue Anruf
        /// </summary>
        [EnumMember]
        InAQueueCall,
    }
}
