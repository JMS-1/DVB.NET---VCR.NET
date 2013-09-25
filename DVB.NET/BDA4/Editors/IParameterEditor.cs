using System;
using System.Windows.Forms;


namespace JMS.DVB.DeviceAccess.Editors
{
    /// <summary>
    /// Erlaubt eine Pflege durch die Angabe eines Paares von Anzeigewert
    /// und tatsächlich gespeichertem Wert.
    /// </summary>
    public interface IParameterEditor : IDisposable
    {
        /// <summary>
        /// Ruft einen Änderungsdialog auf.
        /// </summary>
        /// <param name="dialog">Der Dialog, von dem aus der Aufruf erfolgt.</param>
        /// <param name="parameterName">Der Name eines Parameters in einem DVB.NET Geräteprofil.</param>
        /// <param name="parameterValue">Der Wert des Parameter als Anzeigewert und zu speicherndem Wert.</param>
        /// <returns>Gesetzt, wenn irgend eine Ver#nderung vorgenommen wurde.</returns>
        bool Edit( IWin32Window dialog, string parameterName, ref ParameterValue parameterValue );
    }
}
