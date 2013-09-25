using System;
using System.Reflection;
using System.Diagnostics;


namespace JMS.DVB.DirectShow.UI
{
    partial class TransitionConfiguration
    {
        /// <summary>
        /// Steuert die Protokollierung der Aufrufe an die Arbeitsumgebung.
        /// </summary>
        public static readonly BooleanSwitch InvokeLogger = new BooleanSwitch( Properties.Resources.Switch_Invoke_Name, Properties.Resources.Switch_Invoke_Description );

        /// <summary>
        /// Führt eine Steuermethode aus.
        /// </summary>
        /// <param name="action">Der Name der Methode.</param>
        /// <param name="args">Optional Parameter zur Methode.</param>
        /// <returns>Gesetzt, wenn weitere Aktionen ausgeführt werden sollen.</returns>
        private bool CallAction( string action, params object[] args )
        {
            // Process
            var flag = CallSiteMethod( action, args );

            // Report
            if (InvokeLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_Invoke_Action, flag );

            // Forward
            return ((bool?) flag).GetValueOrDefault( true );
        }

        /// <summary>
        /// Ruft eine Methode der Arbeitsumgebung auf.
        /// </summary>
        /// <param name="methodName">Optional der Name der Methode.</param>
        /// <param name="args">Die für die Methode relevanten Parameter.</param>
        /// <returns>Das Ergebnis des Aufrufs.</returns>
        private object CallSiteMethod( string methodName, params object[] args )
        {
            // Report
            if (InvokeLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_Invoke_Method, methodName );

            // Attach to the site
            if (m_Site == null)
                throw new InvalidOperationException( Properties.Resources.Exception_NoSite );

            // Disabled
            if (string.IsNullOrEmpty( methodName ))
                return null;

            // See if we must dispatch
            if (m_Site.InvokeRequired)
                return m_Site.Invoke( new Func<string, object[], object>( CallSiteMethod ), methodName, args );

            // Load method
            var method = GetSiteMethod( methodName );
            if (method == null)
                throw new ArgumentException( string.Format( Properties.Resources.Exception_NoMethod, methodName ), "methodName" );

            // Process
            return method.Invoke( m_Site, args );
        }

        /// <summary>
        /// Ermittelt eine Methode der Laufzeitumgebung.
        /// </summary>
        /// <param name="methodName">Der Name der Methode.</param>
        /// <returns>Die Methode, sofern bekannt.</returns>
        internal MethodInfo GetSiteMethod( string methodName )
        {
            // Attach to the site
            if (m_Site == null)
                throw new InvalidOperationException( Properties.Resources.Exception_NoSite );
            else
                return m_Site.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
        }

        /// <summary>
        /// Ermittelt eine Eigenschaft der Arbeitsumgebung.
        /// </summary>
        /// <param name="propertyName">Der Name der Eigenschaft.</param>
        /// <returns>Der Wert der Eigenschaft als Zeichenkette.</returns>
        private string ReadSiteProperty( string propertyName )
        {
            // Report
            if (InvokeLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_Invoke_Property, propertyName );

            // Attach to the site
            if (m_Site == null)
                throw new InvalidOperationException( Properties.Resources.Exception_NoSite );

            // See if we have to dispatch
            if (m_Site.InvokeRequired)
                return (string) m_Site.Invoke( new Func<string, string>( ReadSiteProperty ), propertyName );

            // Load property
            var property = GetSiteProperty( propertyName );
            if (property == null)
                throw new ArgumentException( string.Format( Properties.Resources.Exception_NoProperty, propertyName ), "propertyName" );

            // Load the value and report
            var value = property.GetValue( m_Site, null );
            if (value == null)
                return null;
            else
                return value.ToString();
        }

        /// <summary>
        /// Ermittelt eine .NET Eigenschaft der Laufzeitumgebung.
        /// </summary>
        /// <param name="propertyName">Der Name der gewünschten Eigenschaft.</param>
        /// <returns>Die Eigenschaft, sofern vorhanden.</returns>
        internal PropertyInfo GetSiteProperty( string propertyName )
        {
            // Look it up
            if (m_Site == null)
                throw new InvalidOperationException( Properties.Resources.Exception_NoSite );
            else
                return m_Site.GetType().GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
        }

        /// <summary>
        /// Ermittelt einen Wert.
        /// </summary>
        /// <param name="value">Der konfigurierte Wert, entweder eine Konstante oder eine Referenz.</param>
        /// <param name="defaultValue">Der Vorgabewert für den Fall, dass der Wert nicht gesetzt ist.</param>
        /// <returns>Der tatsächlich zu verwendende Wert.</returns>
        internal string Translate( string value, string defaultValue )
        {
            // Check for dynamic value
            var sitePropertyName = IsSiteProperty( value );
            if (!string.IsNullOrEmpty( sitePropertyName ))
                return ReadSiteProperty( sitePropertyName );

            // Check for default
            if (string.IsNullOrEmpty( value ))
                return defaultValue;
            else
                return value;
        }

        /// <summary>
        /// Prüft, ob es sich bei einer Wertereferenz um einen Bezug auf eine Eigenschaft der Laufzeitumgebung handelt.
        /// </summary>
        /// <param name="value">Der verwendete Wert.</param>
        /// <returns>Der Name der Laufzeiteigenschaft oder <i>null</i>.</returns>
        internal string IsSiteProperty( string value )
        {
            // Load default
            if (string.IsNullOrEmpty( value ))
                return null;

            // Check for dynamic value
            if (value.Length > 2)
                if (value.StartsWith( "%" ))
                    if (value.EndsWith( "%" ))
                        return value.Substring( 1, value.Length - 2 );

            // Nope
            return null;
        }
    }
}
