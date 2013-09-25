using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Collections.Generic;

using JMS.DVB.DirectShow.RawDevices;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Beschreibt, wie Zahlen zusammengebaut werden sollen.
    /// </summary>
    [Serializable]
    public class NumberComposer
    {
        /// <summary>
        /// Der zugehörige Zustand.
        /// </summary>
        [XmlIgnore]
        public TransitionState State { get; internal set; }

        /// <summary>
        /// Der kleinste erlaubte Wert.
        /// </summary>
        [XmlAttribute( "min" )]
        public string SmallestAllowedValue { get; set; }

        /// <summary>
        /// Der maximal erlaubte Wert.
        /// </summary>
        [XmlAttribute( "max" )]
        public string LargestAllowedValue { get; set; }

        /// <summary>
        /// Die Zeit, nach der die Eingabe automatisch übernommen wird.
        /// </summary>
        [XmlAttribute( "autoAccept" )]
        public int AutoAcceptTimeOut { get; set; }

        /// <summary>
        /// Die Zeit, nach der die Eingabe einer Zahl verworfen wird.
        /// </summary>
        [XmlAttribute( "lifetime" )]
        public int DiscardTimeOut { get; set; }

        /// <summary>
        /// Eine Taste, die zum vorzeitigen Abschluss der Eingabe verwendet werden kann.
        /// </summary>
        public InputKey? EndKey { get; set; }

        /// <summary>
        /// Beendet die Eingabe.
        /// </summary>
        public string FeedbackOff { get; set; }

        /// <summary>
        /// Meldet eine neue Eingabe.
        /// </summary>
        public string FeedbackOn { get; set; }

        /// <summary>
        /// Die aktuell zusammengestellte Zahl.
        /// </summary>
        private uint m_Number;

        /// <summary>
        /// Der Zeitpunkt, an dem die letzte Ziffer akzeptiert wurde.
        /// </summary>
        private DateTime m_LastKey;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public NumberComposer()
        {
        }

        /// <summary>
        /// Ermittelt den kleinsten erlaubten Wert.
        /// </summary>
        private uint MinimumValue
        {
            get
            {
                // Validate
                if (State == null)
                    throw new InvalidOperationException( Properties.Resources.Exception_NoSite );

                // Load
                var value = State.Configuration.Translate( SmallestAllowedValue, "1" );

                // Convert
                uint min;
                if (uint.TryParse( value, out min ))
                    return min;

                // Ups
                throw new ArgumentException( string.Format( Properties.Resources.Exception_BadNumber, "min", value ), "min" );
            }
        }

        /// <summary>
        /// Ermittelt den größten erlaubten Wert.
        /// </summary>
        private uint MaximumValue
        {
            get
            {
                // Validate
                if (State == null)
                    throw new InvalidOperationException( Properties.Resources.Exception_NoSite );

                // Load
                var value = State.Configuration.Translate( LargestAllowedValue, null );

                // Convert
                uint max;
                if (uint.TryParse( value, out max ))
                    return max;

                // Ups
                throw new ArgumentException( string.Format( Properties.Resources.Exception_BadNumber, "max", value ), "max" );
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Load bounds
            var min = MinimumValue;
            var max = MaximumValue;
            if (min > max)
                return "<invalid number range>";

            // Get bounds
            var size = max.ToString().Length;
            var fmt = new string( '0', size );

            // Report
            return string.Format( "[{0}..{1}]", min.ToString( fmt ), max.ToString( fmt ) );
        }

        /// <summary>
        /// Beginnt mit dem Aufbau einer Zahl.
        /// </summary>
        internal void Reset()
        {
            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceWarning( string.Format( Properties.Resources.Trace_LogInput_StartNumber, State.UniqueIdentifier ) );

            // Reset
            m_LastKey = DateTime.MinValue;
            m_Number = 0;
        }

        /// <summary>
        /// Wird periodisch zur Überprung von Ablaufzeiten aufgerufen.
        /// </summary>
        internal void CheckTimeOut()
        {
            // Check for auto accept
            if (AutoAcceptTimeOut > 0)
                if (DateTime.UtcNow >= m_LastKey.AddMilliseconds( AutoAcceptTimeOut ))
                {
                    // Accept if number is ok
                    Finish( true );

                    // Done
                    return;
                }

            // Check for auto fail
            if (DiscardTimeOut > 0)
                if (DateTime.UtcNow >= m_LastKey.AddMilliseconds( DiscardTimeOut ))
                {
                    // Forget that we tried to enter a number
                    Finish( false );

                    // Done
                    return;
                }
        }

        /// <summary>
        /// Bearbeitet eine Eingabe.
        /// </summary>
        /// <param name="key">Das beobachtete Zeichen.</param>
        /// <returns>Gesetzt, wenn das Zeichen bearbeitet wurde.</returns>
        internal bool Process( InputKey key )
        {
            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceWarning( string.Format( Properties.Resources.Trace_LogInput_NextDigit, key, State.UniqueIdentifier, m_Number ) );

            // Check for digit
            if (key >= InputKey.Digit0)
                if (key <= InputKey.Digit9)
                {
                    // Do the compose and remember the time the user entered the last key
                    m_Number = 10 * m_Number + (uint) (key - InputKey.Digit0);
                    m_LastKey = DateTime.UtcNow;

                    // Check the number of digits allowed
                    var digits = MaximumValue.ToString().Length;
                    var fmt = new string( '0', digits );

                    // Clip if necessary
                    var number = m_Number.ToString( fmt );
                    if (number.Length > digits)
                        m_Number = uint.Parse( number = number.Substring( number.Length - digits, digits ) );

                    // See if we end up on the first valid number and there is no other way to end the number successfully
                    if (AutoAcceptTimeOut < 1)
                        if (!EndKey.HasValue)
                            if (IsValid)
                            {
                                // Finish
                                State.Configuration.DoNumberFeedback( checked( (int) m_Number ), this, true );

                                // We did it all
                                return true;
                            }

                    // Do the feedback
                    State.Configuration.DoNumberFeedback( number, this, false );

                    // Got it
                    return true;
                }

            // See if this is our termination key
            var regular = (EndKey.GetValueOrDefault( InputKey.Digit0 ) == key);

            // Use helper
            Finish( regular );

            // Not us
            return regular;
        }

        /// <summary>
        /// Meldet, ob die aktuelle Eingabe gültig ist.
        /// </summary>
        private bool IsValid
        {
            get
            {
                // Report
                return (m_Number >= MinimumValue) && (m_Number <= MaximumValue);
            }
        }

        /// <summary>
        /// Beendet die Eingabe endgültig.
        /// </summary>
        /// <param name="regular">Gesetzt, wenn eine Übernahme theoretisch möglich wäre.</param>
        private void Finish( bool regular )
        {
            // Merge in flag
            if (regular)
                regular = IsValid;

            // Do report
            State.Configuration.DoNumberFeedback( regular ? checked( (int) m_Number ) : default( int? ), this, true );
        }

        /// <summary>
        /// Meldet alle Konfigurationsfehler dieser Instanz.
        /// </summary>
        internal IEnumerable<Exception> ValidationExceptions
        {
            get
            {
                // Helper
                var configuration = State.Configuration;
                uint scratch;

                // Lower bound
                var propertyName = configuration.IsSiteProperty( SmallestAllowedValue );
                var valid = (propertyName == null) ? uint.TryParse( SmallestAllowedValue, out scratch ) : (configuration.GetSiteProperty( propertyName ) != null);
                if (!valid)
                    yield return new ArgumentException( SmallestAllowedValue, "SmallestAllowedValue" );

                // Upper bound
                propertyName = configuration.IsSiteProperty( LargestAllowedValue );
                valid = (propertyName == null) ? uint.TryParse( LargestAllowedValue, out scratch ) : (configuration.GetSiteProperty( propertyName ) != null);
                if (!valid)
                    yield return new ArgumentException( LargestAllowedValue, "LargestAllowedValue" );

                // Feedback actions
                if (!string.IsNullOrEmpty( FeedbackOn ))
                    if (configuration.GetSiteMethod( FeedbackOn ) == null)
                        yield return new ArgumentException( FeedbackOn, "FeedbackOn" );
                if (!string.IsNullOrEmpty( FeedbackOff ))
                    if (configuration.GetSiteMethod( FeedbackOff ) == null)
                        yield return new ArgumentException( FeedbackOff, "FeedbackOff" );
            }
        }
    }
}
