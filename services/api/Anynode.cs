namespace XPhoneRestApi
{
    public enum AnynodeRejectReason
    {
        success,
        dialString,
        networkPermission,
        networkCongestion,
        networkEquipment,
        busy,
        redirected,
        notResponding,
        notSelected,
        rejected,
        userTerminated,
        mediaNegotiation,
        error,
        domainSpecific0, domainSpecific1, domainSpecific2,
        domainSpecific3, domainSpecific4, domainSpecific5,
        domainSpecific6, domainSpecific7, domainSpecific8,
        domainSpecific9, domainSpecific10, domainSpecific11,
        domainSpecific12, domainSpecific13, domainSpecific14,
        domainSpecific15, domainSpecific16, domainSpecific17,
        domainSpecific18, domainSpecific19
    }

    public class SourceAddress
    {
        public string dialString { get; set; }
        public string displayName { get; set; }
        public string tagSet { get; set; }
    }
    public class DestinationAddress
    {
        public string dialString { get; set; }
        public string displayName { get; set; }
        public string tagSet { get; set; }
    }

    public class AnynodeRoutingRequest
    {
        public string identifier { get; set; }
        public SourceAddress sourceAddress { get; set; }
        public DestinationAddress destinationAddress { get; set; }
    }

    public class AnynodeRoutingResponse
    {
        public bool routeContinue { get; set; }
        public bool routeIgnore { get; set; }
        public bool routeReject { get; set; }
        public string rejectReason { get; set; }
        public SourceAddress sourceAddress { get; set; }
        //public DestinationAddress destinationAddress { get; set; }
    }

}



/*

{
"identifier": "GUID" ,
"sourceAddress" : {
"dialString": "498984079812091" ,
"displayName": "Clip"
},
"destinationAddress": {
"dialString": "4915155661200" ,
"displayName": "Dirk Mobile"
}
}

{
"routeContinue": true ,
"routeIgnore": false ,
"routeReject": false ,
"rejectReason": "success",
"sourceAddress" : {
"dialString": "498984079812091" ,
"displayName": "Clip"
},
"destinationAddress": {
"dialString": "4915155661200" ,
"displayName": "Dirk Mobile"
}
}

 */
