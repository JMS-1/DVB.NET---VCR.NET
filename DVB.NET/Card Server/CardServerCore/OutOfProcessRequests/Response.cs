using System;
using System.IO.Pipes;
using System.Xml.Serialization;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Basisklasse zur Implementierung von Antworten aus einem <i>Card Server</i>.
    /// </summary>
    [Serializable]
    public class Response
    {
        /// <summary>
        /// Setzt oder meldet eine Fehlersituation.
        /// </summary>
        [XmlElement( typeof( CardServerFault ) )]
        [XmlElement( typeof( EPGActiveFault ) )]
        [XmlElement( typeof( EPGNotActiveFault ) )]
        [XmlElement( typeof( NoProfileFault ) )]
        [XmlElement( typeof( NoSourceFault ) )]
        [XmlElement( typeof( NoSourceListFault ) )]
        [XmlElement( typeof( ProfileAlreadyAttachedFault ) )]
        [XmlElement( typeof( ProfileMismatchFault ) )]
        [XmlElement( typeof( ServerBusyFault ) )]
        [XmlElement( typeof( SourceInUseFault ) )]
        [XmlElement( typeof( SourceUpdateActiveFault ) )]
        [XmlElement( typeof( SourceUpdateNotActiveFault ) )]
        [XmlElement( typeof( NoSuchActionFault ) )]
        public CardServerFault Fault { get; set; }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Meldet die Antwortdaten der Anfrage.
        /// </summary>
        [XmlIgnore]
        public virtual object Data
        {
            get
            {
                // Report
                return null;
            }
            set
            {
            }
        }

        /// <summary>
        /// Überträgt diese Antwort in der XML Repräsentation in einen Datenstrom.
        /// </summary>
        /// <param name="stream">Der gewünschte Datenstrom.</param>
        public void SendResponse( AnonymousPipeClientStream stream )
        {
            // Forward
            Request.SendToPipe( stream, new XmlSerializer( GetType() ), this );
        }
    }

    /// <summary>
    /// Basisklasse zur Implementierung von Antworten mit einem
    /// zusätzlichen Rückgabewert.
    /// </summary>
    /// <typeparam name="T">Die Art des Rückgabewertes.</typeparam>
    [Serializable]
    public class Response<T> : Response
    {
        /// <summary>
        /// Der Rückgabewert für diese Anfrage.
        /// </summary>
        public T ResponseData { get; set; }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Meldet die Antwortdaten der Anfrage.
        /// </summary>
        [XmlIgnore]
        public override object Data
        {
            get
            {
                // Report
                return ResponseData;
            }
            set
            {
                // Change
                ResponseData = (T) value;
            }
        }
    }
}
