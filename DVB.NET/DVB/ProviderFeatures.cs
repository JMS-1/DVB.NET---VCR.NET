using System;

namespace JMS.DVB
{
	/// <summary>
	/// Will be used by a <see cref="IDeviceProvider"/> to report what special
	/// features it provides.
	/// </summary>
	[Flags] public enum ProviderFeatures
	{
		/// <summary>
		/// Does not support any special feature.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Record video and primary audio in a single MPEG-2 file.
		/// </summary>
		RecordMPEG2 = 0x0001,

		/// <summary>
		/// Record video and primary audio in a single PVA file.
		/// </summary>
		RecordPVA = 0x0002,

		/// <summary>
		/// Can decrypt services.
		/// </summary>
		Decryption = 0x0004,

		/// <summary>
		/// Will create the video window as a standard child window.
		/// </summary>
		VideoAsChild = 0x0008,

		/// <summary>
		/// Supports various video display modes.
		/// </summary>
		VideoDisplayModes = 0x0010,

		/// <summary>
		/// Set if the hardware audio signal enters the PC thorugh line-in.
		/// </summary>
		UsesLineIn = 0x0020,

		/// <summary>
		/// A SIT filter survies a tune request and needs not to be restarted
		/// after changing a channel.
		/// </summary>
		PersistentSITFilter = 0x0040,

		/// <summary>
		/// Calling <see cref="IDeviceProvider.StopFilters(bool)"/> will respect the
		/// actual parameter if it's set.
		/// </summary>
		CanReInitialize = 0x0080
	}
}
