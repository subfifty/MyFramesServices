using C4B.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C4B.VDir.WebService.Models
{
    public class DebugInfo
    {
        public enum DebugInfoStatus
        {
            failed,
            success,
            unknown
        }

        public enum DebugInfoSection
        {
            SECTION_REQUEST,
            SECTION_VDIR,
            SECTION_DASHBOARD
        }

        public Dictionary<DebugInfoSection, List<DebugLine>> Sections { get; set; }

        public DebugInfo()
        {
            Sections = new Dictionary<DebugInfoSection, List<DebugLine>>  { 
                { DebugInfoSection.SECTION_REQUEST, new List<DebugLine>() },
                { DebugInfoSection.SECTION_VDIR, new List<DebugLine>() },
                { DebugInfoSection.SECTION_DASHBOARD, new List<DebugLine>() },
            };
        }

        public void Failed(DebugInfoSection section, string message)
        {
            Log(section, message, DebugInfoStatus.failed);
        }
        public void Success(DebugInfoSection section, string message)
        {
            Log(section, message, DebugInfoStatus.success);
        }
        public void Unknown(DebugInfoSection section, string message)
        {
            Log(section, message, DebugInfoStatus.unknown);
        }

        public void Log(DebugInfoSection section, string message, DebugInfoStatus status)
        {
            Sections[section].Add(new DebugLine { Message = message, Status = status });
            TraceExtension.Info( Enum.GetName(typeof(DebugInfoStatus), status) + ": " +  message);
        }
    }

    public class DebugLine
    {
        public string Message { get; set; }
        public DebugInfo.DebugInfoStatus Status { get; set; }
    }
}