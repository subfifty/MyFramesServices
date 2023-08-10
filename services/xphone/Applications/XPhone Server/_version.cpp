using namespace System::Reflection;

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

#ifdef DEBUG
	[assembly:AssemblyVersion("1.0.*")];
#else
	#ifdef SIMUL
		[assembly:AssemblyVersion("2.0.*")];
	#else
		#ifdef PATCH_BUILD
			[assembly:AssemblyVersion("9.0.0.0")];
			[assembly:AssemblyFileVersion("9.0.150.1167")];
			[assembly:AssemblyInformationalVersion("9.0.167.0")];
		#else
			[assembly:AssemblyVersion("9.0.0.0")];
			[assembly:AssemblyFileVersion("9.0.150.167")];
			[assembly:AssemblyInformationalVersion("9.0.167.0")];
		#endif
	#endif
#endif

#ifdef NO_COMPANY_INFO
#else
	[assembly:AssemblyCompany("C4B Com For Business AG")];
#endif

#ifdef NO_COPYRIGHT_INFO
#else
	[assembly:AssemblyCopyright("© C4B Com For Business AG. Alle Rechte vorbehalten.")];
#endif

#ifdef NO_LICENSE_VERSION_DATE
#else
	[assembly:C4B::Atlas::License::ATVersionDate(2020, 01, 31)];
#endif
