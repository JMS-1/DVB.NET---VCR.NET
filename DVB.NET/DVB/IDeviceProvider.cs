using System;


namespace JMS.DVB
{
    /// <summary>
    /// Callback interface when using PID filters.
    /// <seealso cref="IDeviceProvider.StartSectionFilter"/>
    /// </summary>
    public delegate void FilterHandler( byte[] data );

    /// <summary>
    /// Abstraction of the DVB hardware access.
    /// </summary>
    public interface IDeviceProvider : IDisposable
    {
        /// <summary>
        /// Stop all PID filters.
        /// </summary>
        void StopFilters();

        /// <summary>
        /// Tune the DVB device to the indicated transponder with predefined DiSEqC
        /// configuration.
        /// </summary>
        /// <param name="transponder">Opaque information of the transponder to use.</param>
        /// <param name="diseqc">Optional DiSEqC configuration which will be used only
        /// for DVB-S.</param>
        void Tune( Transponder transponder, Satellite.DiSEqC diseqc );

        /// <summary>
        /// Set the video and primary audio PID for display and single
        /// channel recording.
        /// </summary>
        /// <param name="videoPID">The video signal to show.</param>
        /// <param name="audioPID">The audio signal to activate.</param>
        /// <param name="ac3PID">The Dolby Digital (AC3) audio signal to activate.</param>
        void SetVideoAudio( ushort videoPID, ushort audioPID, ushort ac3PID );

        /// <summary>
        /// Start a section filter.
        /// </summary>
        /// <param name="pid">PID to filter upon.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        /// <param name="filterData">Filter data for pre-selection.</param>
        /// <param name="filterMask">Masks those bits in the filter data for pre-selection
        /// which are relevant for comparision.</param>
        void StartSectionFilter( ushort pid, FilterHandler callback, byte[] filterData, byte[] filterMask );

        /// <summary>
        /// Prepare filtering a DVB stream.
        /// </summary>
        /// <remarks>
        /// Use <see cref="StartFilter"/> to start filtering.
        /// </remarks>
        /// <param name="pid">PID to filter upon.</param>
        /// <param name="video">Set if a video stream is used.</param>
        /// <param name="smallBuffer">Unset if the largest possible buffer should be used.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, FilterHandler callback );

        /// <summary>
        /// Start filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        void StartFilter( ushort pid );

        /// <summary>
        /// Suspend filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        void StopFilter( ushort pid );

        /// <summary>
        /// Activate decrypting the indicated station.
        /// </summary>
        /// <param name="station">Some station.</param>
        void Decrypt( Station station );

        /// <summary>
        /// Called after a wakeup from hibernation prior to the first tune request.
        /// </summary>
        void WakeUp();

        /// <summary>
        /// Report the current status of the signal.
        /// </summary>
        SignalStatus SignalStatus { get; }
    }
}
