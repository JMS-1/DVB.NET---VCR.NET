using System;
using System.Windows.Forms;

namespace StandByTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            if (MessageBox.Show( new Form(), "Soll dieser Rechner in den Schlafzustand versetzt werden?", "VCR.NET Schlafzustand", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1 ) == DialogResult.Yes)
                VCRControlCenter.VCRNETRestProxy.TryHibernate( "http://localhost/vcr.net" );
        }
    }
}

namespace VCRControlCenter
{
    static class VCRNETControl
    {
        public static void Log( string format, params string[] args )
        {
        }
    }
}
