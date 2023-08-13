#define XPUSER

using C4B.Atlas.VDir;
using C4B.Atlas.VDir.Extensions;
using C4B.Atlas.VDir.Mapping;

namespace C4B.VDir.WebService.Models
{
#if XPUSER
    /// <summary>
    /// Model class for all Xphone User attributes
    /// </summary>
    public class XpUser : IIndexedPropertyObject
    {
        public string CNOCOMPANY { get; set; }
        public string CNOMOBILE { get; set; }
        public string CNOPRIVATE { get; set; }
        public string CNOMAIN { get; set; }
        public string CNOFAX { get; set; }
        public string POSITION { get; set; }
        public string SALUTATION { get; set; }
        public string STATE1 { get; set; }
        public string STREET1 { get; set; }
        public string ZIP1 { get; set; }
        public string CITY1 { get; set; }
        public string EMAIL1 { get; set; }
        public string NAME { get; set; }
        public string FIRSTNAME { get; set; }
        public string DEPARTMENT { get; set; }
        public string COMPANY { get; set; }
        public string USERDEF_1 { get; set; }
        public string USERDEF_2 { get; set; }
        public string USERDEF_3 { get; set; }
        public string USERDEF_4 { get; set; }
        public string USERDEF_5 { get; set; }
        public string USERDEF_6 { get; set; }
        public string USERDEF_7 { get; set; }
        public string USERDEF_8 { get; set; }
        public string USERDEF_9 { get; set; }
        public string USERDEF_10 { get; set; }
        public string USERDEF_11 { get; set; }
        public string USERDEF_12 { get; set; }
        public string USERDEF_13 { get; set; }
        public string USERDEF_14 { get; set; }
        public string USERDEF_15 { get; set; }
        public string USERDEF_16 { get; set; }
        public string USERDEF_17 { get; set; }
        public string USERDEF_18 { get; set; }
        public string USERDEF_19 { get; set; }
        public string USERDEF_20 { get; set; }
        public XpUser()
        {

        }

        public XpUser(VDRecord a_record)
        {
            this.NAME = a_record[VDirCltDefines.AppLinkColumns.Name].GetFirstStringValue() ?? "";
            this.FIRSTNAME = a_record[VDirCltDefines.AppLinkColumns.FirstName].GetFirstStringValue() ?? "";
            this.SALUTATION = a_record[VDirCltDefines.AppLinkColumns.Salutation].GetFirstStringValue() ?? "";
            this.CNOCOMPANY = a_record[VDirCltDefines.AppLinkColumns.CnoCompany].GetFirstStringValue() ?? "";
            this.CNOPRIVATE = a_record[VDirCltDefines.AppLinkColumns.CnoPrivate].GetFirstStringValue() ?? "";
            this.CNOMOBILE = a_record[VDirCltDefines.AppLinkColumns.CnoMobile].GetFirstStringValue() ?? "";
            this.CNOMAIN = a_record[VDirCltDefines.AppLinkColumns.CnoMain].GetFirstStringValue() ?? "";
            this.CNOFAX = a_record[VDirCltDefines.AppLinkColumns.CnoFax].GetFirstStringValue() ?? "";
            this.EMAIL1 = a_record[VDirCltDefines.AppLinkColumns.Email1].GetFirstStringValue() ?? "";
            this.COMPANY = a_record[VDirCltDefines.AppLinkColumns.Company].GetFirstStringValue() ?? "";
            this.DEPARTMENT = a_record[VDirCltDefines.AppLinkColumns.Department].GetFirstStringValue() ?? "";
            this.STREET1 = a_record[VDirCltDefines.AppLinkColumns.Street1].GetFirstStringValue() ?? "";
            this.ZIP1 = a_record[VDirCltDefines.AppLinkColumns.Zip1].GetFirstStringValue() ?? "";
            this.CITY1 = a_record[VDirCltDefines.AppLinkColumns.City1].GetFirstStringValue() ?? "";
            this.POSITION = a_record[VDirCltDefines.AppLinkColumns.Position].GetFirstStringValue() ?? "";
            this.STATE1 = a_record[VDirCltDefines.AppLinkColumns.State1].GetFirstStringValue() ?? "";
            this.USERDEF_1 = a_record[VDirCltDefines.AppLinkColumns.UserDef1].GetFirstStringValue() ?? "";
            this.USERDEF_2 = a_record[VDirCltDefines.AppLinkColumns.UserDef2].GetFirstStringValue() ?? "";
            this.USERDEF_3 = a_record[VDirCltDefines.AppLinkColumns.UserDef3].GetFirstStringValue() ?? "";
            this.USERDEF_4 = a_record[VDirCltDefines.AppLinkColumns.UserDef4].GetFirstStringValue() ?? "";
            this.USERDEF_5 = a_record[VDirCltDefines.AppLinkColumns.UserDef5].GetFirstStringValue() ?? "";
            this.USERDEF_6 = a_record[VDirCltDefines.AppLinkColumns.UserDef6].GetFirstStringValue() ?? "";
            this.USERDEF_7 = a_record[VDirCltDefines.AppLinkColumns.UserDef7].GetFirstStringValue() ?? "";
            this.USERDEF_8 = a_record[VDirCltDefines.AppLinkColumns.UserDef8].GetFirstStringValue() ?? "";
            this.USERDEF_9 = a_record[VDirCltDefines.AppLinkColumns.UserDef9].GetFirstStringValue() ?? "";
            this.USERDEF_10 = a_record[VDirCltDefines.AppLinkColumns.UserDef10].GetFirstStringValue() ?? "";
            this.USERDEF_11 = a_record[VDirCltDefines.AppLinkColumns.UserDef11].GetFirstStringValue() ?? "";
            this.USERDEF_12 = a_record[VDirCltDefines.AppLinkColumns.UserDef12].GetFirstStringValue() ?? "";
            this.USERDEF_13 = a_record[VDirCltDefines.AppLinkColumns.UserDef13].GetFirstStringValue() ?? "";
            this.USERDEF_14 = a_record[VDirCltDefines.AppLinkColumns.UserDef14].GetFirstStringValue() ?? "";
            this.USERDEF_15 = a_record[VDirCltDefines.AppLinkColumns.UserDef15].GetFirstStringValue() ?? "";
            this.USERDEF_16 = a_record[VDirCltDefines.AppLinkColumns.UserDef16].GetFirstStringValue() ?? "";
            this.USERDEF_17 = a_record[VDirCltDefines.AppLinkColumns.UserDef17].GetFirstStringValue() ?? "";
            this.USERDEF_18 = a_record[VDirCltDefines.AppLinkColumns.UserDef18].GetFirstStringValue() ?? "";
            this.USERDEF_19 = a_record[VDirCltDefines.AppLinkColumns.UserDef19].GetFirstStringValue() ?? "";
            this.USERDEF_20 = a_record[VDirCltDefines.AppLinkColumns.UserDef20].GetFirstStringValue() ?? "";
        }
        public string this[string key]
        {
            get
            {
                switch (key)
                {
                    case "NAME": return this.NAME;
                    case "FIRSTNAME": return this.FIRSTNAME;
                    case "SALUTATION": return this.SALUTATION;
                    case "CNOCOMPANY": return this.CNOCOMPANY;
                    case "CNOPRIVATE": return this.CNOPRIVATE;
                    case "CNOMOBILE": return this.CNOMOBILE;
                    case "CNOMAIN": return this.CNOMAIN;
                    case "CNOFAX": return this.CNOFAX;
                    case "EMAIL1": return this.EMAIL1;
                    case "COMPANY": return this.COMPANY;
                    case "DEPARTMENT": return this.DEPARTMENT;
                    case "STREET1": return this.STREET1;
                    case "ZIP1": return this.ZIP1;
                    case "CITY1": return this.CITY1;
                    case "POSITION": return this.POSITION;
                    case "STATE1": return this.STATE1;
                    case "USERDEF_1": return this.USERDEF_1;
                    case "USERDEF_2": return this.USERDEF_2;
                    case "USERDEF_3": return this.USERDEF_3;
                    case "USERDEF_4": return this.USERDEF_4;
                    case "USERDEF_5": return this.USERDEF_5;
                    case "USERDEF_6": return this.USERDEF_6;
                    case "USERDEF_7": return this.USERDEF_7;
                    case "USERDEF_8": return this.USERDEF_8;
                    case "USERDEF_9": return this.USERDEF_9;
                    case "USERDEF_10": return this.USERDEF_10;
                    case "USERDEF_11": return this.USERDEF_11;
                    case "USERDEF_12": return this.USERDEF_12;
                    case "USERDEF_13": return this.USERDEF_13;
                    case "USERDEF_14": return this.USERDEF_14;
                    case "USERDEF_15": return this.USERDEF_15;
                    case "USERDEF_16": return this.USERDEF_16;
                    case "USERDEF_17": return this.USERDEF_17;
                    case "USERDEF_18": return this.USERDEF_18;
                    case "USERDEF_19": return this.USERDEF_19;
                    case "USERDEF_20": return this.USERDEF_20;
                }
                return null;
            }
        }
    }
#endif
}
