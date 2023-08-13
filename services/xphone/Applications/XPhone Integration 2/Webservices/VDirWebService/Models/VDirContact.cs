using System;
using C4B.Atlas.VDir;
using C4B.Atlas.VDir.Extensions;
using C4B.Atlas.VDir.Mapping;
using C4B.VDir.WebService.Models;
using System.Text;
using System.Drawing;

/// <summary>
/// Wandelt ein Bild in einen Base64-String und zurück
/// </summary>
internal class ImageToString
{
    /// <summary>
    /// Konvertiert ein Bild in einen Base64-String
    /// </summary>
    /// <param name="image">
    /// Zu konvertierendes Bild
    /// </param>
    /// <returns>
    /// Base64 Repräsentation des Bildes
    /// </returns>
    internal static string GetStringFromImage(Image image)
    {
        if (image != null)
        {
            ImageConverter ic = new ImageConverter();
            byte[] buffer = (byte[])ic.ConvertTo(image, typeof(byte[]));
            return Convert.ToBase64String(
                buffer,
                Base64FormattingOptions.InsertLineBreaks);
        }
        else
            return null;
    }
    //---------------------------------------------------------------------
    /// <summary>
    /// Konvertiert einen Base64-String zu einem Bild
    /// </summary>
    /// <param name="base64String">
    /// Zu konvertierender String
    /// </param>
    /// <returns>
    /// Bild das aus dem String erzeugt wird
    /// </returns>
    internal static Image GetImageFromString(string base64String)
    {
        byte[] buffer = Convert.FromBase64String(base64String);

        if (buffer != null)
        {
            ImageConverter ic = new ImageConverter();
            return ic.ConvertFrom(buffer) as Image;
        }
        else
            return null;
    }
}

/// <summary>
/// Summary description for VDirContact
/// </summary>
public class VDirContact : IIndexedPropertyObject
{
    public string DBAPPLICATION { get; set; }
    public string DBNATIVEID { get; set; }
    public string DBPARENTID { get; set; }
    public string DBSERVER { get; set; }
    public string DBDATABASE { get; set; }
    public string DBINSTANCE { get; set; }
    public string DBAPPVERSION { get; set; }
    public string DBENTITYTYPE { get; set; }

    public string ID { get; set; }
    public string NAME { get; set; }
    public string FIRSTNAME { get; set; }
    public string TITLE { get; set; }
    public string SALUTATION { get; set; }
    public string CNOCOMPANY { get; set; }
    public string CNOPRIVATE { get; set; }
    public string CNOMOBILE { get; set; }
    public string CNOMAIN { get; set; }
    public string CNOOTHER { get; set; }
    public string CNOFAX { get; set; }
    public string CNOFAX2 { get; set; }
    public string EMAIL1 { get; set; }
    public string EMAIL2 { get; set; }
    public string EMAIL3 { get; set; }
    public string WEB { get; set; }
    public string COMPANY { get; set; }
    public string COMPANY2 { get; set; }
    public string DEPARTMENT { get; set; }
    public string PRIVATE { get; set; }
    public string STREET1 { get; set; }
    public string ZIP1 { get; set; }
    public string CITY1 { get; set; }
    public string COUNTRY1 { get; set; }
    public string STREET2 { get; set; }
    public string ZIP2 { get; set; }
    public string CITY2 { get; set; }
    public string COUNTRY2 { get; set; }
    public string BIRTHDAY { get; set; }
    public string ACCOUNTCODE { get; set; }
    public string CUSTOMERNO { get; set; }

    public string POSITION { get; set; }
    public string NOTE { get; set; }
    public string CATEGORY { get; set; }
    public string STATE1 { get; set; }
    public string STATE2 { get; set; }
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

    public string ADAPTER { get; set; }
    public string ADAPTERDISPLAYNAME { get; set; }
    public string VDIR { get; set; }
    public string DISTINGUISHEDNAME { get; set; }
    public string ADAPTERCLASSIFICATION { get; set; }
    public string PRIORITY { get; set; }
    public string FUZZYMATCH { get; set; }
    public string HASPHOTO { get; set; }
    public string AGGREGATEDMATCH { get; set; }
    public string ICONINDEX { get; set; }

    public string DN { get; set; }
    public string JPEGPHOTO { get; set; }

    public VDirContact()
    {
    }

    public VDirContact(VDRecord a_record, bool withPhoto = false)
    {
        this.ID = a_record[VDirCltDefines.AppLinkColumns.ID].GetFirstStringValue() ?? "";
        this.NAME = a_record[VDirCltDefines.AppLinkColumns.Name].GetFirstStringValue() ?? "";
        this.FIRSTNAME = a_record[VDirCltDefines.AppLinkColumns.FirstName].GetFirstStringValue() ?? "";
        this.TITLE = a_record[VDirCltDefines.AppLinkColumns.Title].GetFirstStringValue() ?? "";
        this.SALUTATION = a_record[VDirCltDefines.AppLinkColumns.Salutation].GetFirstStringValue() ?? "";
        this.CNOCOMPANY = a_record[VDirCltDefines.AppLinkColumns.CnoCompany].GetFirstStringValue() ?? "";
        this.CNOPRIVATE = a_record[VDirCltDefines.AppLinkColumns.CnoPrivate].GetFirstStringValue() ?? "";
        this.CNOMOBILE = a_record[VDirCltDefines.AppLinkColumns.CnoPrivate].GetFirstStringValue() ?? "";
        this.CNOMAIN = a_record[VDirCltDefines.AppLinkColumns.CnoMain].GetFirstStringValue() ?? "";
        this.CNOOTHER = a_record[VDirCltDefines.AppLinkColumns.CnoOther].GetFirstStringValue() ?? "";
        this.CNOFAX = a_record[VDirCltDefines.AppLinkColumns.CnoFax].GetFirstStringValue() ?? "";
        this.CNOFAX2 = a_record[VDirCltDefines.AppLinkColumns.CnoFax2].GetFirstStringValue() ?? "";
        this.EMAIL1 = a_record[VDirCltDefines.AppLinkColumns.Email1].GetFirstStringValue() ?? "";
        this.EMAIL2 = a_record[VDirCltDefines.AppLinkColumns.Email2].GetFirstStringValue() ?? "";
        this.EMAIL3 = a_record[VDirCltDefines.AppLinkColumns.Email3].GetFirstStringValue() ?? "";
        this.WEB = a_record[VDirCltDefines.AppLinkColumns.Web].GetFirstStringValue() ?? "";
        this.COMPANY = a_record[VDirCltDefines.AppLinkColumns.Company].GetFirstStringValue() ?? "";
        this.COMPANY2 = a_record[VDirCltDefines.AppLinkColumns.Company2].GetFirstStringValue() ?? "";
        this.DEPARTMENT = a_record[VDirCltDefines.AppLinkColumns.Department].GetFirstStringValue() ?? "";
        this.PRIVATE = a_record[VDirCltDefines.AppLinkColumns.Private].GetFirstStringValue() ?? "";
        this.STREET1 = a_record[VDirCltDefines.AppLinkColumns.Street1].GetFirstStringValue() ?? "";
        this.ZIP1 = a_record[VDirCltDefines.AppLinkColumns.Zip1].GetFirstStringValue() ?? "";
        this.CITY1 = a_record[VDirCltDefines.AppLinkColumns.City1].GetFirstStringValue() ?? "";
        this.COUNTRY1 = a_record[VDirCltDefines.AppLinkColumns.Country1].GetFirstStringValue() ?? "";
        this.STREET2 = a_record[VDirCltDefines.AppLinkColumns.Street2].GetFirstStringValue() ?? "";
        this.ZIP2 = a_record[VDirCltDefines.AppLinkColumns.Zip2].GetFirstStringValue() ?? "";
        this.CITY2 = a_record[VDirCltDefines.AppLinkColumns.City2].GetFirstStringValue() ?? "";
        this.COUNTRY2 = a_record[VDirCltDefines.AppLinkColumns.Country2].GetFirstStringValue() ?? "";
        this.BIRTHDAY = a_record[VDirCltDefines.AppLinkColumns.Birthday].GetFirstStringValue() ?? "";
        this.ACCOUNTCODE = a_record[VDirCltDefines.AppLinkColumns.AccountCode].GetFirstStringValue() ?? "";
        this.CUSTOMERNO = a_record[VDirCltDefines.AppLinkColumns.CustomerNo].GetFirstStringValue() ?? "";

        this.POSITION = a_record[VDirCltDefines.AppLinkColumns.Position].GetFirstStringValue() ?? "";
        this.NOTE = a_record[VDirCltDefines.AppLinkColumns.Note].GetFirstStringValue() ?? "";
        this.CATEGORY = a_record[VDirCltDefines.AppLinkColumns.Category].GetFirstStringValue() ?? "";
        this.STATE1 = a_record[VDirCltDefines.AppLinkColumns.State1].GetFirstStringValue() ?? "";
        this.STATE2 = a_record[VDirCltDefines.AppLinkColumns.State2].GetFirstStringValue() ?? "";
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
        this.USERDEF_18 = a_record[VDirCltDefines.AppLinkColumns.UserDef18].GetFirstStringValue() ?? ""; // photo source
        this.USERDEF_19 = a_record[VDirCltDefines.AppLinkColumns.UserDef19].GetFirstStringValue() ?? ""; // sip address
        this.USERDEF_20 = a_record[VDirCltDefines.AppLinkColumns.UserDef20].GetFirstStringValue() ?? ""; // xmpp address

        this.ADAPTER = a_record[VDirCltDefines.AppLinkColumns.Adapter].GetFirstStringValue() ?? "";
        this.ADAPTERDISPLAYNAME = a_record[VDirCltDefines.AppLinkColumns.AdapterDisplayName].GetFirstStringValue() ?? "";
        this.VDIR = a_record[VDirCltDefines.AppLinkColumns.VDir].GetFirstStringValue() ?? "";
        this.DISTINGUISHEDNAME = a_record[VDirCltDefines.AppLinkColumns.DisplayName].GetFirstStringValue() ?? "";
        this.ADAPTERCLASSIFICATION = a_record[VDirCltDefines.AppLinkColumns.AdapterClassification].GetFirstStringValue() ?? "";
        this.PRIORITY = a_record[VDirCltDefines.AppLinkColumns.Priority].GetFirstStringValue() ?? "";
        this.FUZZYMATCH = a_record[VDirCltDefines.AppLinkColumns.FuzzyMatch].GetFirstStringValue() ?? "";
        this.HASPHOTO = a_record[VDirCltDefines.AppLinkColumns.HasPhoto].GetFirstStringValue() ?? "";
        this.AGGREGATEDMATCH = a_record[VDirCltDefines.AppLinkColumns.AggregatedMatch].GetFirstStringValue() ?? "";
        this.ICONINDEX = a_record[VDirCltDefines.AppLinkColumns.IconIndex].GetFirstStringValue() ?? "";

        this.DBAPPLICATION = a_record[VDirCltDefines.AppLinkColumns.dbApplication].GetFirstStringValue() ?? "";
        this.DBNATIVEID = a_record[VDirCltDefines.AppLinkColumns.dbNativeId].GetFirstStringValue() ?? "";
        this.DBPARENTID = a_record[VDirCltDefines.AppLinkColumns.dbParentId].GetFirstStringValue() ?? "";
        this.DBSERVER = a_record[VDirCltDefines.AppLinkColumns.dbServer].GetFirstStringValue() ?? "";
        this.DBDATABASE = a_record[VDirCltDefines.AppLinkColumns.dbDataBase].GetFirstStringValue() ?? "";
        this.DBINSTANCE = a_record[VDirCltDefines.AppLinkColumns.dbInstance].GetFirstStringValue() ?? "";
        this.DBAPPVERSION = a_record[VDirCltDefines.AppLinkColumns.dbAppVersion].GetFirstStringValue() ?? "";
        this.DBENTITYTYPE = a_record[VDirCltDefines.AppLinkColumns.dbEntityType].GetFirstStringValue() ?? "";

        this.DN = a_record[VDirCltDefines.AppLinkColumns.DistinguishedName].GetFirstStringValue() ?? "";

        if ( this.USERDEF_18.ToLower() == "adapter" )
        {
            this.JPEGPHOTO = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAB4AHgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3+iiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAoopCQBknAHegBaK4rV/FczTNDp7BI1ODLjJb6Z6CsU61qZbd9vuM/75x+VAHp9FcRpXiu4jlWK/YSwscF8YZffjqP1rtVZXUMpBUjII6GgB1FFFABRRRQAUUUUAFFFFABRRRQAVjeJ7prXQ5dpw0pEYPpnr+gNbNcx41YjTrdexmyfwB/xoA4miiigAr0DwpdNcaKquQTCxjHrjgj+ePwrz+uy8Et+4vF7B1I/I/4UAdXRRRQAUUUUAFFFFABRRRQAUUUUAFc/4uhaXRN4GfKkVj7Dkf1FdBUNzbpc20kEgykilT+NAHlFFTXds9pdy28n3o2IPv71DQAV3Xg63MWkvKy4MshIPqAAP55rireB7m4jhjGXkYKufU16ja26WlrHbxj5I1Cj3oAnooooAKKKKACiiigAooooAKKKp3up2dgubm4VD2XOSfwHNAFyo5p4reMyTSLGg6sxwK5O+8Zk5Sxgx/00l/oB/jXNXd9c30ge5naUjpuPA+g6CgC3r91Dd6zNPbsGjbaAwGMkAA1mUUUAafh+eC11qCa4cJGu75iOASCB/OvRop4riMSQyLIh6FSCDXk1T2l7c2Um+2neJj12ng/UdDQB6tRXHWHjNhhL6HI/56RcH8VNdJZanZX65trhXPdc4YfgeaALtFFFABRRRQAUUUUAZ+sX39naZLcDG8DCA92PT/H8K81lleeVpJXZnc5ZmOSTXe+Kraa70uOK3ieVxMCVUZOMNzXHf2Lqn/Phcf8AfBoAoUVf/sXVP+fC4/74NH9i6p/z4XH/AHwaAKFFX/7F1T/nwuP++DR/Yuqf8+Fx/wB8GgChRV/+xdU/58Lj/vg0f2Lqn/Phcf8AfBoAoU+KR4ZFkjdldTlWU4INXP7F1T/nwuP++DR/Yuqf8+Fx/wB8GgDvdF1D+09MinON/wB1wP7w6/0P41o1g+FLaa00uSO4ieNzMSFcYOMLzW9QAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFAH/9k=";
        }
        else
        {
            this.JPEGPHOTO = withPhoto ? Convert.ToBase64String(a_record[VDirCltDefines.AppLinkColumns.JpegPhoto].GetByteValue(0)) : "";
        }
    }


    //private string ByteArrayToString(byte[] ba)
    //{
    //    StringBuilder hex = new StringBuilder(ba.Length * 2);
    //    foreach (byte b in ba)
    //        hex.AppendFormat("{0:x2}", b);
    //    return hex.ToString();
    //}

    //private byte[] StringToByteArray(String hex)
    //{
    //    int NumberChars = hex.Length;
    //    byte[] bytes = new byte[NumberChars / 2];
    //    for (int i = 0; i < NumberChars; i += 2)
    //        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //    return bytes;
    //}

    public string this[string key]
    {
        get
        {
            switch (key)
            {
                case "DBAPPLICATION": return this.DBAPPLICATION;
                case "DBNATIVEID": return this.DBNATIVEID;
                case "DBPARENTID": return this.DBPARENTID;
                case "DBSERVER": return this.DBSERVER;
                case "DBDATABASE": return this.DBDATABASE;
                case "DBINSTANCE": return this.DBINSTANCE;
                case "DBAPPVERSION": return this.DBAPPVERSION;
                case "DBENTITYTYPE": return this.DBENTITYTYPE;
                case "ID": return this.ID;
                case "NAME": return this.NAME;
                case "FIRSTNAME": return this.FIRSTNAME;
                case "TITLE": return this.TITLE;
                case "SALUTATION": return this.SALUTATION;
                case "CNOCOMPANY": return this.CNOCOMPANY;
                case "CNOPRIVATE": return this.CNOPRIVATE;
                case "CNOMOBILE": return this.CNOMOBILE;
                case "CNOMAIN": return this.CNOMAIN;
                case "CNOOTHER": return this.CNOOTHER;
                case "CNOFAX": return this.CNOFAX;
                case "CNOFAX2": return this.CNOFAX2;
                case "EMAIL1": return this.EMAIL1;
                case "EMAIL2": return this.EMAIL2;
                case "EMAIL3": return this.EMAIL3;
                case "WEB": return this.WEB;
                case "COMPANY": return this.COMPANY;
                case "COMPANY2": return this.COMPANY2;
                case "DEPARTMENT": return this.DEPARTMENT;
                case "PRIVATE": return this.PRIVATE;
                case "STREET1": return this.STREET1;
                case "ZIP1": return this.ZIP1;
                case "CITY1": return this.CITY1;
                case "COUNTRY1": return this.COUNTRY1;
                case "STREET2": return this.STREET2;
                case "ZIP2": return this.ZIP2;
                case "CITY2": return this.CITY2;
                case "COUNTRY2": return this.COUNTRY2;
                case "BIRTHDAY": return this.BIRTHDAY;
                case "ACCOUNTCODE": return this.ACCOUNTCODE;
                case "CUSTOMERNO": return this.CUSTOMERNO;
                case "POSITION": return this.POSITION;
                case "NOTE": return this.NOTE;
                case "CATEGORY": return this.CATEGORY;
                case "STATE1": return this.STATE1;
                case "STATE2": return this.STATE2;
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
                case "ADAPTER": return this.ADAPTER;
                case "VDIR": return this.VDIR;
                case "DISTINGUISHEDNAME": return this.DISTINGUISHEDNAME;
                case "ADAPTERCLASSIFICATION": return this.ADAPTERCLASSIFICATION;
                case "PRIORITY": return this.PRIORITY;
                case "FUZZYMATCH": return this.FUZZYMATCH;
                case "HASPHOTO": return this.HASPHOTO;
                case "AGGREGATEDMATCH": return this.AGGREGATEDMATCH;
                case "ICONINDEX": return this.ICONINDEX;

                case "DN": return this.DN;
                case "JPEGPHOTO": return this.JPEGPHOTO;
                default: return null;
            }
        }
    }

}