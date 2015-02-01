using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using JMS.DVBVCR.RecordingService.WebServer;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle( "VCR.NET Recording Service Core Functionality" )]
[assembly: AssemblyDescription( "VCR.NET Recording Service Core Functionality Implementation" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "" )]
[assembly: AssemblyCopyright( "Copyright © 2003-15" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "2979d97e-7a70-441b-a300-96df2005d86c" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion( "4.3.0.0" )]

// Startcode für die Web Anwendung
[assembly: PreApplicationStartMethod( typeof( ServerRuntime ), "WebStartup" )]

