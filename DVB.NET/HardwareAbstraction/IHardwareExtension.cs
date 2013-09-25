using System;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.DeviceAccess;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Erweiterung einer Geräteabstraktion.
    /// </summary>
    public interface IHardwareExtension
    {
    }

    /// <summary>
    /// Ist in der Lage, die Aktionslisten des BDA Empfängergraphen zu konfigurieren.
    /// </summary>
    public interface IPipelineExtension : IHardwareExtension
    {
        /// <summary>
        /// Aktiviert Aktionen in der Liste.
        /// </summary>
        /// <param name="graph">Der DirectShow Graph, der den BDA Empfang übernehmen wird.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <param name="types">Die gewünschte Art der Aktivierung.</param>
        void Install( DataGraph graph, Profile profile, PipelineTypes types );
    }

    /// <summary>
    /// Erlaubt die zusätzliche Pflege von Detailparametern.
    /// </summary>
    public interface IPipelineParameterExtension : IPipelineExtension
    {
        /// <summary>
        /// Ruft den Pflegedialog auf.
        /// </summary>
        /// <param name="parent">Der Oberflächenkontext des Aufrufs.</param>
        /// <param name="parameters">Die Liste der aktuellen Parameter.</param>
        /// <param name="type">Die Art der Erweiterung, deren Parameter gepflegt werden sollen.</param>
        void Edit( Control parent, List<ProfileParameter> parameters, PipelineTypes type );
    }
}
