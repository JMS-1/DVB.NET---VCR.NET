using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// The class is used to describe a <i>Program Map Table</i> which can
    /// be found on various PIDs in a transport stream.
    /// </summary>
    public class PMT : Table
    {
        /// <summary>
        /// The PCR for this program.
        /// </summary>
        public readonly ushort PCRPID;

        /// <summary>
        /// The program number of this program.
        /// </summary>
        public readonly ushort ProgramNumber;

        /// <summary>
        /// Descriptors for this program.
        /// </summary>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// 
        /// </summary>
        public readonly ProgramEntry[] ProgramEntries;

        /// <summary>
        /// Set for table identifiers <i>0x42</i> and <i>0x46</i>.
        /// </summary>
        /// <param name="tableIdentifier">The table identifier for which this <see cref="Type"/>
        /// should report its responsibility.</param>
        /// <returns>Set for table identifier <i>0x42</i> and <i>0x46</i>.</returns>
        public static bool IsHandlerFor( byte tableIdentifier )
        {
            // Check all
            return (0x02 == tableIdentifier);
        }

        /// <summary>
        /// Create a new <i>Program Map Table</i> instance.
        /// </summary>
        /// <param name="section">The section which is currently parsed.</param>
        public PMT( Section section )
            : base( section )
        {
            // Special recommendation
            if ((0 != SectionNumber) || (0 != LastSectionNumber)) return;

            // Get the overall length
            int offset = 9, length = section.Length - 3 - offset - 4;

            // Not possible
            if (length < 0) return;

            // Read statics
            ProgramNumber = Tools.MergeBytesToWord( Section[1], Section[0] );
            PCRPID = (ushort) (0x1fff & Tools.MergeBytesToWord( Section[6], Section[5] ));

            // Length
            int infoLength = 0xfff & Tools.MergeBytesToWord( Section[8], Section[7] );

            // Validate
            if (length < infoLength) return;

            // Create my descriptors
            Descriptors = Descriptor.Load( this, offset, infoLength );

            // Adjust
            offset += infoLength;
            length -= infoLength;

            // Result
            List<ProgramEntry> entries = new List<ProgramEntry>();

            // Fill
            while (length > 0)
            {
                // Create next
                ProgramEntry entry = ProgramEntry.Create( this, offset, length );

                // Failed
                if ((null == entry) || !entry.IsValid) return;

                // Remember
                entries.Add( entry );

                // Adjust
                offset += entry.Length;
                length -= entry.Length;
            }

            // Use it
            ProgramEntries = entries.ToArray();

            // Usefull
            m_IsValid = true;
        }

        /// <summary>
        /// Erzeugt aus einer SI Tabelle (PMT) die Instanz einer Senderinformation.
        /// </summary>
        /// <param name="services">Optional eine Liste von Dienstkennungen, die aus SI Tabellen (SDT)
        /// ermittelt wurden.</param>
        /// <param name="transponder">Der Transponder, in dem der Sender registriert werden soll.</param>
        /// <returns>Ein neuer Sender oder <i>null</i>.</returns>
        public Station CreateStation( Dictionary<ushort, ServiceEntry> services, Transponder transponder )
        {
            // Information needed
            List<AudioInfo> mp2Audio = new List<AudioInfo>(), ac3Audio = new List<AudioInfo>();
            List<DVBSubtitleInfo> subtitles = new List<DVBSubtitleInfo>();
            ushort videoPID = 0, TTXPID = 0;
            bool encrypted = false;
            int videoType = -1;

            // Check for CA descriptor
            foreach (Descriptor descriptor in Descriptors)
            {
                // Test
                encrypted = (DescriptorTags.CA == descriptor.Tag);

                // Done
                if (encrypted) break;
            }

            // Process all streams
            foreach (ProgramEntry programme in ProgramEntries)
                if ((StreamTypes.Video13818 == programme.StreamType) || (StreamTypes.H264 == programme.StreamType))
                {
                    // Remember video
                    videoType = (int) programme.StreamType;
                    videoPID = programme.ElementaryPID;
                }
                else if ((StreamTypes.Audio13818 == programme.StreamType) || (StreamTypes.Audio11172 == programme.StreamType))
                {
                    // Remember audio
                    mp2Audio.Add( AudioInfo.Create( programme.ElementaryPID, programme.ProgrammeName.Trim(), false ) );
                }
                else if (StreamTypes.PrivateData == programme.StreamType)
                {
                    // Scan descriptors
                    foreach (Descriptor descriptor in programme.Descriptors)
                    {
                        // AC3
                        Descriptors.AC3 ac3 = descriptor as Descriptors.AC3;

                        // Test
                        if (null != ac3)
                        {
                            // Add to list
                            ac3Audio.Add( AudioInfo.Create( programme.ElementaryPID, programme.ProgrammeName.Trim(), true ) );

                            // Done
                            break;
                        }

                        // Teletext
                        Descriptors.Teletext ttx = descriptor as Descriptors.Teletext;

                        // Test
                        if (null != ttx)
                        {
                            // Remember
                            TTXPID = programme.ElementaryPID;

                            // Done
                            break;
                        }

                        // Sub-titles
                        Descriptors.Subtitle sub = descriptor as Descriptors.Subtitle;

                        // Test
                        if (null != sub)
                        {
                            // Process all languages and pages in this stream
                            foreach (SubtitleInfo info in sub.Subtitles)
                            {
                                // Create entry
                                subtitles.Add( new DVBSubtitleInfo( programme.ElementaryPID, info.ISOLanguage, (byte) info.Type, info.CompositionPage, info.AncillaryPage ) );
                            }

                            // Done
                            break;
                        }
                    }
                }

            // Find audio PIDs
            ushort audioPID = (mp2Audio.Count > 0) ? mp2Audio[0].PID : (ushort) 0;
            ushort AC3PID = (ac3Audio.Count > 0) ? ac3Audio[0].PID : (ushort) 0;

            // Check for valid station
            if ((0 == videoPID) && (0 == audioPID) && (0 == AC3PID)) return null;

            // Derived data
            ushort networkIdentifier = 0, transportStreamIdentifier = 0;
            string providerName = null, serviceName = null;

            // See if there is service entry
            ServiceEntry service;
            if (null != services)
                if (services.TryGetValue( ProgramNumber, out service ))
                {
                    // Override flag
                    encrypted = service.Scrambled;

                    // Attach to table
                    SDT sdtTable = (SDT) service.Table;

                    // Load
                    transportStreamIdentifier = sdtTable.TransportStreamIdentifier;
                    networkIdentifier = sdtTable.OriginalNetworkIdentifier;

                    // Try to find the service descriptor
                    foreach (Descriptor serviceDescriptor in service.Descriptors)
                    {
                        // Load
                        Descriptors.Service serviceInfo = serviceDescriptor as Descriptors.Service;

                        // Skip
                        if (null == serviceInfo) continue;

                        // Load
                        providerName = serviceInfo.ProviderName;
                        serviceName = serviceInfo.ServiceName;

                        // Done
                        break;
                    }
                }

            // Use defaults
            if (string.IsNullOrEmpty( providerName ) || (string.IsNullOrEmpty( serviceName )) || (0 == networkIdentifier) || (0 == transportStreamIdentifier))
            {
                // Use common notation
                serviceName = string.Format( "Service {0}", ProgramNumber );
                transportStreamIdentifier = 0xffff;
                networkIdentifier = ProgramNumber;
                providerName = "Service";
            }

            // Create the station - will automatically register with the transponder
            Station station = new Station( transponder, networkIdentifier, transportStreamIdentifier, ProgramNumber, serviceName, providerName, videoPID, audioPID, null, PCRPID, AC3PID, TTXPID, encrypted );

            // Load audio map 
            station.AudioInfos = mp2Audio.ToArray();
            station.AC3Infos = ac3Audio.ToArray();

            // Load sub-titles
            station.DVBSubtitles.AddRange( subtitles );

            // Update the stream type
            if (videoType >= 0) station.VideoType = (byte) videoType;

            // Report
            return station;
        }

        /// <summary>
        /// Erzeugt aus einer SI Tabelle (PMT) die Instanz einer Senderinformation.
        /// </summary>
        /// <returns>Ein neuer Sender oder <i>null</i>.</returns>
        public Station CreateStation()
        {
            // Forward
            return CreateStation( null, new JMS.DVB.Satellite.SatelliteChannel() );
        }
    }
}
