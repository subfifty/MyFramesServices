using System.Reflection;

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:

#if DEBUG
[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyInformationalVersion("9.0.3012.0")]
#elif SIMUL
    [assembly: AssemblyVersion( "2.0.*" )]
#else
    #if PATCH_BUILD
        [assembly: AssemblyVersion("9.0.2000.0")]
        [assembly: AssemblyFileVersion("9.0.2000.4012")]
        [assembly: AssemblyInformationalVersion("9.0.3012.0")]
    #else
        [assembly: AssemblyVersion("9.0.2000.0")]
        [assembly: AssemblyFileVersion("9.0.2000.1012")]
        [assembly: AssemblyInformationalVersion("9.0.3012.0")]
    #endif
#endif

#if NO_COMPANY_INFO
#else
[assembly: AssemblyCompany("C4B Com For Business AG")]
#endif

#if NO_COPYRIGHT_INFO
#else
[assembly: AssemblyCopyright("© C4B Com For Business AG. Alle Rechte vorbehalten.")]
#endif

#if NO_LICENSE_VERSION_DATE
#else
[assembly: C4B.Atlas.License.ATVersionDate(2022, 05, 17)]
#endif
