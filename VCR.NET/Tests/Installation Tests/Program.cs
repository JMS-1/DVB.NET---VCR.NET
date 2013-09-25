using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using JMS.DVBVCR.Installation;
using System.Collections.Generic;
using System.Configuration.Install;

namespace Installation_Tests
{
    class Program
    {
        static void Main( string[] args )
        {
            InstallContext context = new InstallContext();

            context.Parameters["assemblypath"] = typeof( ProjectInstaller ).Assembly.CodeBase.Substring( 8 ).Replace( '/', '\\' );
            context.Parameters["Mode"] = "1";

            using (ProjectInstaller installer = new ProjectInstaller { AutoTestMode = true, Context = context })
            {
                Dictionary<string, string> items = new Dictionary<string, string>();

                installer.Install( items );
            }
        }

        static void Main1( string[] args )
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );

            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load( config.FilePath );

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( true );
            Application.Run( new ProfileInstaller( xmlConfig ) );
        }
    }
}
