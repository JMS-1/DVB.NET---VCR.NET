using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB.SI;
using JMS.DVB.EPG.Descriptors;

namespace JMS.DVB
{
    /// <summary>
    /// Erweiterungsmethoden zur einfacheren Nutzung der Hardware Abstraktionen
    /// im Zusammenspiel mit SI Tabellen.
    /// </summary>
    public static class TableExtensions
    {
        /// <summary>
        /// Meldet einen Verbraucher für die Auswertung von SI Tabellen einer bestimmten Art an.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Art von SI Tabellen.</typeparam>
        /// <param name="provider">Die zu aktuelle Hardware Abstraktion.</param>
        /// <param name="stream">Die eindeutige Nummer (PID) des Datenstroms in der aktuellen <see cref="SourceGroup"/>.</param>
        /// <param name="consumer">Die Methode, an die alle erkannten SI Tabellen der gewünschten Art gemeldet werden.</param>
        /// <returns>Die eindeutige Kennung des neu angemeldeten Verbrauchers.</returns>
        /// <exception cref="ArgumentNullException">Die Hardware Abstraktion und / oder der Verbraucher sind nicht gesetzt.</exception>
        public static Guid AddConsumer<T>(this Hardware provider, ushort stream, Action<T> consumer) where T : Table
        {
            // Validate
            if (null == provider)
                throw new ArgumentNullException("provider");
            if (null == consumer)
                throw new ArgumentNullException("consumer");

            // Forward
            return provider.AddConsumer(stream, TableParser.Create(consumer), Table.GetIsExtendedTable<T>() ? StreamTypes.ExtendedTable : StreamTypes.StandardTable);
        }

        /// <summary>
        /// Meldet einen Verbraucher für eine SI Tabellenart an, die fest an einen Datenstrom
        /// gebunden ist.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Art von SI Tabelle.</typeparam>
        /// <param name="provider">Die zu aktuelle Hardware Abstraktion.</param>
        /// <param name="consumer">Die Methode, an die alle erkannten SI Tabellen der gewünschten Art gemeldet werden.</param>
        /// <returns>Die eindeutige Kennung des neu angemeldeten Verbrauchers.</returns>
        /// <exception cref="ArgumentNullException">Die Hardware Abstraktion und / oder der Verbraucher sind nicht gesetzt.</exception>
        public static Guid AddConsumer<T>(this Hardware provider, Action<T> consumer) where T : WellKnownTable
        {
            // Validate
            if (null == provider)
                throw new ArgumentNullException("provider");
            if (null == consumer)
                throw new ArgumentNullException("consumer");

            // Read the stream
            ushort stream = WellKnownTable.GetWellKnownStream<T>();

            // Forward
            return provider.AddConsumer(stream, TableParser.Create(consumer), Table.GetIsExtendedTable<T>() ? StreamTypes.ExtendedTable : StreamTypes.StandardTable);
        }

        /// <summary>
        /// Meldet eine Analyseinstanz auf einem Datenstrom von SI Tabellen an.
        /// </summary>
        /// <param name="provider">Die zu aktuelle Hardware Abstraktion.</param>
        /// <param name="stream">Die eindeutige Nummer (PID) des Datenstroms in der aktuellen <see cref="SourceGroup"/>.</param>
        /// <param name="parser">Die zu verwendene Analyseeinheit.</param>
        /// <returns>Die eindeutige Kennung des neu angemeldeten Verbrauchers.</returns>
        /// <exception cref="ArgumentNullException">Die Hardware Abstraktion und / oder die Analyseinstanz
        /// sind nicht gesetzt.</exception>
        public static Guid AddConsumer(this Hardware provider, ushort stream, TableParser parser)
        {
            // Forward
            return provider.AddConsumer(stream, parser, StreamTypes.StandardTable);
        }

        /// <summary>
        /// Meldet eine Analyseinstanz auf einem Datenstrom von SI Tabellen an.
        /// </summary>
        /// <param name="provider">Die zu aktuelle Hardware Abstraktion.</param>
        /// <param name="stream">Die eindeutige Nummer (PID) des Datenstroms in der aktuellen <see cref="SourceGroup"/>.</param>
        /// <param name="parser">Die zu verwendene Analyseeinheit.</param>
        /// <param name="streamType">Die Art der Tabellenkennungen, die verwendet werden.</param>
        /// <returns>Die eindeutige Kennung der neu angemeldeten Analyseeinheit.</returns>
        /// <exception cref="ArgumentNullException">Die Hardware Abstraktion und / oder die Analyseinstanz
        /// sind nicht gesetzt.</exception>
        public static Guid AddConsumer(this Hardware provider, ushort stream, TableParser parser, StreamTypes streamType)
        {
            // Validate
            if (null == provider)
                throw new ArgumentNullException("provider");
            if (null == parser)
                throw new ArgumentNullException("parser");

            // Register
            return provider.AddConsumer(stream, streamType, parser.AddPayload);
        }

        /// <summary>
        /// Ermittelt die Informationen zu dem gerade aktiven Ursprung.
        /// </summary>
        /// <param name="provider">Das verwendete DVB.NET Gerät.</param>
        /// <param name="milliSeconds">Die maximale Wartezeit auf die Informationen in Millisekunden.</param>
        /// <returns>Die gewünschten Informationen oder <i>null</i>, wenn diese in der angegebenen
        /// Zeit nicht bereit gestellt werden konnten.</returns>
        public static SatelliteLocationInformation GetLocationInformation(this SatelliteHardware provider, int milliSeconds)
        {
            // Forward
            return (SatelliteLocationInformation)((Hardware)provider).GetLocationInformation(milliSeconds);
        }

        /// <summary>
        /// Ermittelt die Informationen zu dem gerade aktiven Ursprung.
        /// </summary>
        /// <param name="provider">Das verwendete DVB.NET Gerät.</param>
        /// <param name="milliSeconds">Die maximale Wartezeit auf die Informationen in Millisekunden.</param>
        /// <returns>Die gewünschten Informationen oder <i>null</i>, wenn diese in der angegebenen
        /// Zeit nicht bereit gestellt werden konnten.</returns>
        public static CableLocationInformation GetLocationInformation(this CableHardware provider, int milliSeconds)
        {
            // Forward
            return (CableLocationInformation)((Hardware)provider).GetLocationInformation(milliSeconds);
        }

        /// <summary>
        /// Ermittelt die Informationen zu dem gerade aktiven Ursprung.
        /// </summary>
        /// <param name="provider">Das verwendete DVB.NET Gerät.</param>
        /// <param name="milliSeconds">Die maximale Wartezeit auf die Informationen in Millisekunden.</param>
        /// <returns>Die gewünschten Informationen oder <i>null</i>, wenn diese in der angegebenen
        /// Zeit nicht bereit gestellt werden konnten.</returns>
        public static TerrestrialLocationInformation GetLocationInformation(this TerrestrialHardware provider, int milliSeconds)
        {
            // Forward
            return (TerrestrialLocationInformation)((Hardware)provider).GetLocationInformation(milliSeconds);
        }

        /// <summary>
        /// Wandelt einen zusammengehörigen Block von SI NIT Tabellen in Informationen über einen
        /// Ursprung um.
        /// </summary>
        /// <param name="tables">Die Rohinformationen als SI NIT Tabellen.</param>
        /// <param name="provider">Das zugehörige DVB.NET Gerät, das noch auf den Ursprung eingestellt ist.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public static LocationInformation ToLocationInformation(this NIT[] tables, Hardware provider)
        {
            // Validate
            if (null == provider)
                throw new ArgumentNullException("provider");

            // None
            if (null == tables)
                return null;

            // Check DVB-S(2)
            Hardware<SatelliteProfile, SatelliteLocation, SatelliteGroup> sat = provider as Hardware<SatelliteProfile, SatelliteLocation, SatelliteGroup>;
            if (null != sat)
                return tables.ToLocationInformation(sat);

            // Check DVB-C
            Hardware<CableProfile, CableLocation, CableGroup> cab = provider as Hardware<CableProfile, CableLocation, CableGroup>;
            if (null != cab)
                return tables.ToLocationInformation(cab);

            // Check DVB-T
            Hardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup> ter = provider as Hardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup>;
            if (null != ter)
                return tables.ToLocationInformation(ter);

            // Not supported
            throw new ArgumentException(provider.GetType().FullName, "provider");
        }

        /// <summary>
        /// Wandelt einen zusammengehörigen Block von SI NIT Tabellen in Informationen über einen
        /// Ursprung um.
        /// </summary>
        /// <param name="tables">Die Rohinformationen als SI NIT Tabellen.</param>
        /// <param name="provider">Das zugehörige DVB.NET Gerät, das noch auf den Ursprung eingestellt ist.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public static SatelliteLocationInformation ToLocationInformation(this NIT[] tables, Hardware<SatelliteProfile, SatelliteLocation, SatelliteGroup> provider)
        {
            // Forward
            if (null == provider)
                return CreateLocationInformation<SatelliteGroup, SatelliteLocationInformation>(new SatelliteLocation(), tables);
            else
                return CreateLocationInformation<SatelliteGroup, SatelliteLocationInformation>(SatelliteLocation.Parse(provider.CurrentLocation.ToString()), tables);
        }

        /// <summary>
        /// Erzeugt eine Informationsinstanz zu einem Ursprung aus eine zusammengehörigen Gruppe von
        /// SI NIT Tabellen eines DVB.NET Gerätes.
        /// </summary>
        /// <typeparam name="G">Die konkrete Art der Quellgruppen.</typeparam>
        /// <typeparam name="L">Die konkrete Art der Information.</typeparam>
        /// <param name="location">Der Usprung, zu dem die Tabellen ermittelt wurden.</param>
        /// <param name="tables">Die SI NIT Tabellengruppe zum noch angewählten Ursprung.</param>
        /// <returns>Eine Informationsisntanz passend zur SI NIT Tabellengruppe oder <i>null</i>.</returns>
        private static L CreateLocationInformation<G, L>(GroupLocation<G> location, NIT[] tables)
            where G : SourceGroup, new()
            where L : LocationInformation<G>, new()
        {
            // None
            if (null == tables)
                return null;

            // Create the new instance
            L info = new L { Location = location };

            // Process all groups in all tables
            foreach (var table in tables)
                foreach (var entry in table.Table.NetworkEntries)
                    foreach (var descriptor in entry.Descriptors)
                    {
                        // Start with DVB-S(2)
                        var sat = descriptor as EPG.Descriptors.SatelliteDelivery;
                        if (null != sat)
                        {
                            // Process
                            G group = sat.ToGroup() as G;

                            // Add if possible
                            if (null != group)
                                info.SourceGroups.Add(group);

                            // Process next group
                            break;
                        }

                        // Then DVB-C
                        var cab = descriptor as EPG.Descriptors.CableDelivery;
                        if (null != cab)
                        {
                            // Process
                            G group = cab.ToGroup() as G;

                            // Add if possible
                            if (null != group)
                                info.SourceGroups.Add(group);

                            // Process next group
                            break;
                        }

                        // Finally DVB-T
                        var ter = descriptor as EPG.Descriptors.TerrestrialDelivery;
                        if (null != ter)
                        {
                            // Process
                            G group = ter.ToGroup() as G;

                            // Add if possible
                            if (null != group)
                                info.SourceGroups.Add(group);

                            // Process next group
                            break;
                        }
                    }

            // Report
            return info;
        }

        /// <summary>
        /// Erzeugt die Beschreibung einer Quellgruppe aus einer SI Beschreibung eines
        /// NIT Eintrags.
        /// </summary>
        /// <param name="descriptor">Die Daten zur Quellgruppe.</param>
        /// <returns>Die Quellgruppe.</returns>
        private static CableGroup ToGroup(this EPG.Descriptors.CableDelivery descriptor)
        {
            // Create core
            CableGroup group = new CableGroup
            {
                Frequency = descriptor.Frequency,
                SpectrumInversion = SpectrumInversions.Auto,
                SymbolRate = descriptor.SymbolRate * 1000,
                Bandwidth = Bandwidths.NotDefined
            };

            // Modulation
            switch (descriptor.Modulation)
            {
                case EPG.CableModulations.QAM16: group.Modulation = CableModulations.QAM16; break;
                case EPG.CableModulations.QAM32: group.Modulation = CableModulations.QAM32; break;
                case EPG.CableModulations.QAM64: group.Modulation = CableModulations.QAM64; break;
                case EPG.CableModulations.QAM128: group.Modulation = CableModulations.QAM128; break;
                case EPG.CableModulations.QAM256: group.Modulation = CableModulations.QAM256; break;
            }

            // Report
            return group;
        }

        /// <summary>
        /// Erzeugt die Beschreibung einer Quellgruppe aus einer SI Beschreibung eines
        /// NIT Eintrags.
        /// </summary>
        /// <param name="descriptor">Die Daten zur Quellgruppe.</param>
        /// <returns>Die Quellgruppe.</returns>
        private static TerrestrialGroup ToGroup(this EPG.Descriptors.TerrestrialDelivery descriptor)
        {
            // Create core
            TerrestrialGroup group =
                new TerrestrialGroup
                {
                    Frequency = descriptor.Frequency
                };

            // Bandwidth
            switch (descriptor.Bandwidth)
            {
                case EPG.TerrestrialBandwidths.Five: group.Bandwidth = Bandwidths.Five; break;
                case EPG.TerrestrialBandwidths.Six: group.Bandwidth = Bandwidths.Six; break;
                case EPG.TerrestrialBandwidths.Seven: group.Bandwidth = Bandwidths.Seven; break;
                case EPG.TerrestrialBandwidths.Eight: group.Bandwidth = Bandwidths.Eight; break;
                case EPG.TerrestrialBandwidths.Reserved100: group.Bandwidth = Bandwidths.Reserved100; break;
                case EPG.TerrestrialBandwidths.Reserved101: group.Bandwidth = Bandwidths.Reserved101; break;
                case EPG.TerrestrialBandwidths.Reserved110: group.Bandwidth = Bandwidths.Reserved110; break;
                case EPG.TerrestrialBandwidths.Reserved111: group.Bandwidth = Bandwidths.Reserved111; break;
            }

            // Report
            return group;
        }

        /// <summary>
        /// Erzeugt die Beschreibung einer Quellgruppe aus einer SI Beschreibung eines
        /// NIT Eintrags.
        /// </summary>
        /// <param name="descriptor">Die Daten zur Quellgruppe.</param>
        /// <returns>Die Quellgruppe.</returns>
        private static SatelliteGroup ToGroup(this EPG.Descriptors.SatelliteDelivery descriptor)
        {
            // Create core
            var group = new SatelliteGroup
            {
                Frequency = descriptor.Frequency,
                IsWestPosition = descriptor.West,
                OrbitalPosition = descriptor.OrbitalPosition.ToString("0000"),
                SymbolRate = descriptor.SymbolRate * 1000
            };

            // DVB-S2 modulation
            group.UsesS2Modulation = (0 != (0x04 & descriptor.Modulation));

            // Roll-Off if using DVB-S2
            if (group.UsesS2Modulation)
                switch (0x18 & descriptor.Modulation)
                {
                    case 0x00: group.RollOff = S2RollOffs.Alpha35; break;
                    case 0x08: group.RollOff = S2RollOffs.Alpha25; break;
                    case 0x10: group.RollOff = S2RollOffs.Alpha20; break;
                    case 0x18: group.RollOff = S2RollOffs.Reserved; break;
                }

            // Modulation
            switch (0x03 & descriptor.Modulation)
            {
                case 0: group.Modulation = SatelliteModulations.Auto; break;
                case 1: group.Modulation = SatelliteModulations.QPSK; break;
                case 2: group.Modulation = SatelliteModulations.PSK8; break;
                case 3: group.Modulation = SatelliteModulations.QAM16; break;
            }

            // Polarisation
            switch (descriptor.Polarization)
            {
                case EPG.Polarizations.Horizontal: group.Polarization = Polarizations.Horizontal; break;
                case EPG.Polarizations.Vertical: group.Polarization = Polarizations.Vertical; break;
                case EPG.Polarizations.Left: group.Polarization = Polarizations.Left; break;
                case EPG.Polarizations.Right: group.Polarization = Polarizations.Right; break;
            }

            // Error correction
            switch (descriptor.InnerFEC)
            {
                case EPG.InnerFECs.NotDefined: group.InnerFEC = InnerForwardErrorCorrectionModes.NotDefined; break;
                case EPG.InnerFECs.Conv1_2: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv1_2; break;
                case EPG.InnerFECs.Conv2_3: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv2_3; break;
                case EPG.InnerFECs.Conv3_4: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv3_4; break;
                case EPG.InnerFECs.Conv5_6: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv5_6; break;
                case EPG.InnerFECs.Conv7_8: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv7_8; break;
                case EPG.InnerFECs.Conv8_9: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv8_9; break;
                case EPG.InnerFECs.Reserved0111: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv3_5; break;
                case EPG.InnerFECs.Reserved1000: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv4_5; break;
                case EPG.InnerFECs.Reserved1001: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv9_10; break;
                case EPG.InnerFECs.Reserved1010: group.InnerFEC = InnerForwardErrorCorrectionModes.Reserved1010; break;
                case EPG.InnerFECs.Reserved1011: group.InnerFEC = InnerForwardErrorCorrectionModes.Reserved1011; break;
                case EPG.InnerFECs.Reserved1100: group.InnerFEC = InnerForwardErrorCorrectionModes.Reserved1100; break;
                case EPG.InnerFECs.Reserved1101: group.InnerFEC = InnerForwardErrorCorrectionModes.Reserved1101; break;
                case EPG.InnerFECs.Reserved1110: group.InnerFEC = InnerForwardErrorCorrectionModes.Reserved1110; break;
                case EPG.InnerFECs.NoConv: group.InnerFEC = InnerForwardErrorCorrectionModes.NoConv; break;
            }

            // Report
            return group;
        }

        /// <summary>
        /// Wandelt einen zusammengehörigen Block von SI NIT Tabellen in Informationen über einen
        /// Ursprung um.
        /// </summary>
        /// <param name="tables">Die Rohinformationen als SI NIT Tabellen.</param>
        /// <param name="provider">Das zugehörige DVB.NET Gerät, das noch auf den Ursprung eingestellt ist.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public static CableLocationInformation ToLocationInformation(this NIT[] tables, Hardware<CableProfile, CableLocation, CableGroup> provider)
        {
            // Forward
            return CreateLocationInformation<CableGroup, CableLocationInformation>(new CableLocation(), tables);
        }

        /// <summary>
        /// Wandelt einen zusammengehörigen Block von SI NIT Tabellen in Informationen über einen
        /// Ursprung um.
        /// </summary>
        /// <param name="tables">Die Rohinformationen als SI NIT Tabellen.</param>
        /// <param name="provider">Das zugehörige DVB.NET Gerät, das noch auf den Ursprung eingestellt ist.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public static TerrestrialLocationInformation ToLocationInformation(this NIT[] tables, Hardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup> provider)
        {
            // Forward
            return CreateLocationInformation<TerrestrialGroup, TerrestrialLocationInformation>(new TerrestrialLocation(), tables);
        }

        /// <summary>
        /// Meldet zu den SI Tabellen STD und PAT die Liste aller Dienste, die von einer
        /// Quellgruppe angeboten werden.
        /// </summary>
        /// <param name="services">Die SI Informationen über die Dienste.</param>
        /// <param name="associations">Die SI Informationen mit der Zuordnung der Dienste
        /// zu den <i>Program Mapping Table (PMT)</i> Tabellen.</param>
        /// <returns>Die gewünschte Informationsinstanz.</returns>
        public static GroupInformation ToGroupInformation(this SDT[] services, PAT[] associations)
        {
            // Create new
            var info = new GroupInformation();

            // See what we need
            if (services == null)
                return info;
            if (services.Length < 1)
                return info;
            if (associations == null)
                return info;
            if (associations.Length < 1)
                return info;

            // All services we processed
            var processed = new HashSet<ushort>();

            // Get the first (and normally only) PAT as a reference
            PAT pat = associations[0];

            // Process all service tables
            foreach (var service in services)
                if (service.TransportStream == pat.TransportStream)
                    foreach (var entry in service.Table.Services)
                        if (associations[0][entry.ServiceIdentifier].HasValue)
                        {
                            // Find the description
                            var descr = EPG.DescriptorExtensions.Find<EPG.Descriptors.Service>(entry.Descriptors);
                            if (descr == null)
                                continue;

                            // Get the type
                            SourceTypes type;

                            // Check mode
                            if (descr.ServiceType == EPG.ServiceTypes.DigitalTelevision)
                                type = SourceTypes.TV;
                            else if (descr.ServiceType == EPG.ServiceTypes.DigitalRadio)
                                type = SourceTypes.Radio;
                            else if (descr.ServiceType == EPG.ServiceTypes.FMRadio)
                                type = SourceTypes.Radio;
                            else if (descr.ServiceType == EPG.ServiceTypes.SDTVDigitalTelevision)
                                type = SourceTypes.TV;
                            else if (descr.ServiceType == EPG.ServiceTypes.HDTVDigitalTelevision)
                                type = SourceTypes.TV;
                            else if (descr.ServiceType == EPG.ServiceTypes.SkyHDTV)
                                type = SourceTypes.TV;
                            else if (descr.ServiceType == EPG.ServiceTypes.SkyNVOD)
                                type = SourceTypes.TV;
                            else
                                type = SourceTypes.Unknown;

                            // Add the station
                            info.Sources.Add(
                                new Station
                                {
                                    TransportStream = service.TransportStream,
                                    Service = entry.ServiceIdentifier,
                                    Provider = descr.ProviderName,
                                    IsEncrypted = entry.Scrambled,
                                    Network = service.Network,
                                    Name = descr.ServiceName,
                                    SourceType = type,
                                });

                            // Mark as done
                            processed.Add(entry.ServiceIdentifier);
                        }

            // Get the first SDT as a reference
            var sdt = services[0];

            // No check all services
            foreach (var serviceIdentifier in associations[0].Services)
                if (!processed.Contains(serviceIdentifier))
                    info.Sources.Add(
                        new Station
                        {
                            Name = string.Format("Service {0}", serviceIdentifier),
                            TransportStream = sdt.TransportStream,
                            SourceType = SourceTypes.Unknown,
                            Service = serviceIdentifier,
                            Network = sdt.Network,
                            Provider = "Service",
                            IsService = true,
                        });

            // Report
            return info;
        }

        /// <summary>
        /// Sucht in einer Liste von Assoziationen nach einem Dienst.
        /// </summary>
        /// <param name="associations">Die Liste der Assoziationen.</param>
        /// <param name="serviceIdentifier">Die eindeutige Kennung des gewünschten Dienstes.</param>
        /// <returns>Die Datenstromkennung der SI Tabelle PMT des gewünschten Dienstes.</returns>
        public static ushort? FindService(this PAT[] associations, ushort serviceIdentifier)
        {
            return
                (associations ?? Enumerable.Empty<PAT>())
                    .Select(association => association[serviceIdentifier])
                    .FirstOrDefault(pmt => pmt.HasValue);
        }

        /// <summary>
        /// Übernimmt Informationen aus einer SI Programmbeschreibung in die Informationen
        /// eines Senders.
        /// </summary>
        /// <param name="source">Die Daten zu einer Quelle, die vervollständigt werden sollen.</param>
        /// <param name="program">Die SI Programmbeschreibung.</param>
        public static void Update(this SourceInformation source, EPG.ProgramEntry program)
        {
            // MPEG-2 Video
            if (EPG.StreamTypes.Video13818 == program.StreamType)
            {
                // Remember
                source.VideoType = VideoTypes.MPEG2;
                source.VideoStream = program.ElementaryPID;

                // Done
                return;
            }

            // H.264 Video
            if (EPG.StreamTypes.H264 == program.StreamType)
            {
                // Remember
                source.VideoType = VideoTypes.H264;
                source.VideoStream = program.ElementaryPID;

                // Done
                return;
            }

            // MP2 Audio
            if ((EPG.StreamTypes.Audio11172 == program.StreamType) || (EPG.StreamTypes.Audio13818 == program.StreamType))
            {
                // Create new entry
                var audio = new AudioInformation { AudioType = AudioTypes.MP2, AudioStream = program.ElementaryPID, Language = program.ProgrammeName.Trim() };

                // Remember it
                source.AudioTracks.Add(audio);

                // Done 
                return;
            }

            // AAC Audio
            if (EPG.StreamTypes.AAC == program.StreamType)
            {
                // The AAC information.
                int? aac = null;

                // Find corresponding AAC descriptor.
                var aacDescriptor = program.Descriptors.OfType<AAC>().FirstOrDefault();

                if (aacDescriptor != null)
                {
                    // Optional type.
                    aac = aacDescriptor.Type ?? 255;

                    // Profile and level.
                    aac = (aac << 8) + aacDescriptor.ProfileAndLevel;
                }

                // Create new entry
                var audio = new AudioInformation
                {
                    AAC = aac.HasValue ? (ushort)aac : (ushort?)null,
                    Language = program.ProgrammeName.Trim(),
                    AudioType = AudioTypes.MP2,
                    AudioStream = program.ElementaryPID,
                };

                // Remember it
                source.AudioTracks.Add(audio);

                // Done 
                return;
            }

            // AC3, TTX or DVB subtitles
            if (EPG.StreamTypes.PrivateData != program.StreamType)
                return;

            // Direct processing of descriptor list
            foreach (var descriptor in program.Descriptors)
            {
                // Check for AC3
                var ac3 = descriptor as EPG.Descriptors.AC3;
                if (null != ac3)
                {
                    // Create new entry
                    AudioInformation audio = new AudioInformation { AudioType = AudioTypes.AC3, AudioStream = program.ElementaryPID, Language = program.ProgrammeName.Trim() };

                    // Remember it
                    source.AudioTracks.Add(audio);

                    // Done
                    return;
                }

                // Check for videotext
                var ttx = descriptor as EPG.Descriptors.Teletext;
                if (null != ttx)
                {
                    // Remember
                    source.TextStream = program.ElementaryPID;

                    // Done
                    return;
                }

                // Check for DVB sub-titles
                var sub = descriptor as EPG.Descriptors.Subtitle;
                if (null != sub)
                {
                    // Process all items
                    foreach (var subTitle in sub.Subtitles)
                    {
                        // Create the information
                        var info = new SubtitleInformation
                        {
                            SubtitleStream = program.ElementaryPID,
                            Language = subTitle.Language,
                            SubtitleType = (SubtitleTypes)subTitle.Type,
                            CompositionPage = subTitle.CompositionPage,
                            AncillaryPage = subTitle.AncillaryPage
                        };

                        // Remember
                        source.Subtitles.Add(info);
                    }

                    // Done
                    return;
                }
            }
        }

        /// <summary>
        /// Ermittelt zu einer Sprachangabe die ISO Kurznotation, sofern möglich.
        /// </summary>
        /// <param name="language">Die gewünschte Sprache.</param>
        /// <returns>Die ISO Kurznotation.</returns>
        public static string ToISOLanguage(this string language)
        {
            // Not possible
            if (null == language)
                return null;

            // Use helper
            return EPG.LanguageItemExtensions.ToISOLanguage(language);
        }

        /// <summary>
        /// Ermittelt zu einer ISO Kurznotation die gewünschte Sprache.
        /// </summary>
        /// <param name="language">Eine ISO Kurznotation.</param>
        /// <returns>Die zugehörige Sprache, wie sie in DVB.NET verwendet wird.</returns>
        public static string FromISOLanguage(this string language)
        {
            // Not possible
            if (null == language)
                return null;

            // Use helper
            return EPG.ProgramEntry.GetLanguageFromISOLanguage(language);
        }
    }
}
