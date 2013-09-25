using System;
using System.Diagnostics;
using System.Windows.Forms;


namespace JMS.DVB.DirectShow.UI
{
    partial class TransitionConfiguration
    {
        /// <summary>
        /// Konfiguriert die Protokollierung beid er Eingabe von Zahlen.
        /// </summary>
        public static readonly BooleanSwitch NumberLogger = new BooleanSwitch( Properties.Resources.Switch_LogNumber_Name, Properties.Resources.Switch_LogNumber_Description );

        /// <summary>
        /// Optional eine Komponente zur Zusammenstellung einer Zahl.
        /// </summary>
        private NumberComposer m_Composer;

        /// <summary>
        /// Ein Zeitgeber.
        /// </summary>
        private Timer m_Timer;

        /// <summary>
        /// Wird zehn mal pro Sekunde ausgelöst.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void OnTimer( object sender, EventArgs e )
        {
            // Forward to number collector
            var composer = m_Composer;
            if (composer != null)
                composer.CheckTimeOut();
        }

        /// <summary>
        /// Meldet eine aktuell eingegebene Zahl.
        /// </summary>
        /// <param name="number">Die darzustellende Zahl.</param>
        /// <param name="composer">Die Komponente zur Zusammenstellung der Zahl.</param>
        /// <param name="done">Gesetzt, wenn die Zusammenstellung abegschlossen wurde.</param>
        internal void DoNumberFeedback( object number, NumberComposer composer, bool done )
        {
            // Be sage
            try
            {
                // First do the feedback
                CallSiteMethod( done ? composer.FeedbackOff : composer.FeedbackOn, number );

                // Remember composer or forget it
                m_Composer = done ? null : composer;

                // Report
                if (NumberLogger.Enabled)
                    if (m_Composer == null)
                        Trace.TraceInformation( Properties.Resources.Trace_Number_Off );
                    else
                        Trace.TraceInformation( Properties.Resources.Trace_Number_On );
            }
            catch (Exception e)
            {
                // Terminate
                m_Composer = null;

                // Report
                ReportException( e );
            }
        }
    }
}
