using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// The base class for any descriptor inside a <see cref="Table"/>.
    /// </summary>
    /// <remarks>
    /// The physical coding of a descriptor starts with a <see cref="byte"/>
    /// holding the tag as defined in <see cref="DescriptorTags"/> followed
    /// by a <see cref="byte"/> with the length of the descriptor data. The
    /// data immediatly follows. Since the total length of a descriptor is 
    /// limited to 255 bytes some types as <see cref="Descriptors.ExtendedEvent"/>
    /// allow joining multiple instances.
    /// </remarks>
    public abstract class Descriptor
    {
        /// <summary>
        /// This map will be created once when this <see cref="Type"/> is accessed
        /// for the first time. For each <see cref="byte"/> of a descriptor tag
        /// the corresponding handler class is added.
        /// </summary>
        /// <remarks>
        /// If there is not yet a handler <see cref="Type"/> for a table identifier
        /// <see cref="Descriptors.Generic"/> will be used.
        /// </remarks>
        private static readonly Type[] m_HandlerForTag = new Type[256];

        /// <summary>
        /// Must include all handler classes.
        /// </summary>
        /// <remarks>
        /// To allow a correct build of the lookup map <see cref="m_HandlerForTag"/>
        /// the handler <see cref="Type"/> must provide a class method <i>IsHandlerFor</i>
        /// of type <see cref="bool"/> with a single <see cref="byte"/> parameter. This method
        /// has to report <i>true</i> for any descriptor tag it is responsible for.
        /// </remarks>
        private static readonly Type[] m_Handlers =
        {
            typeof(Descriptors.ShortEvent),
            typeof(Descriptors.Content),
            typeof(Descriptors.Component),
            typeof(Descriptors.ExtendedEvent),
            typeof(Descriptors.PrivateData),
            typeof(Descriptors.PDCDescriptor),
            typeof(Descriptors.ParentalRating),
            typeof(Descriptors.Linkage),
            typeof(Descriptors.StreamIdentifier),
            typeof(Descriptors.Service),
            typeof(Descriptors.ISOLanguage),
            typeof(Descriptors.DataBroadcast),
            typeof(Descriptors.Teletext),
            typeof(Descriptors.AC3),
            typeof(Descriptors.AAC),
            typeof(Descriptors.NetworkName),
            typeof(Descriptors.CableDelivery),
            typeof(Descriptors.SatelliteDelivery),
            typeof(Descriptors.TerrestrialDelivery),
            typeof(Descriptors.ServiceList),
            typeof(Descriptors.Subtitle),
            typeof(Descriptors.ContentTransmissionPremiere),
            typeof(Descriptors.CellList),
            typeof(Descriptors.FrequencyList),
            typeof(Descriptors.CellFrequencyLink),
            typeof(Descriptors.AncillaryData),
            typeof(Descriptors.ApplicationSignalling),
            typeof(Descriptors.DataBroadastId),
            typeof(Descriptors.CarouselIdentifier)
        };

        /// <summary>
        /// Populate the descriptor tag lookup map.
        /// </summary>
        static Descriptor()
        {
            // Use helper
            Tools.InitializeDynamicCreate(m_Handlers, m_HandlerForTag, typeof(Descriptors.Generic));
        }

        /// <summary>
        /// Set by derived classes in the constructor as soon as the table
        /// contents is validated.
        /// <seealso cref="IsValid"/>
        /// </summary>
        protected bool m_Valid = false;

        /// <summary>
        /// The tag which can not be served.
        /// </summary>
        public readonly DescriptorTags Tag;

        /// <summary>
        /// The corresponding event instance. 
        /// </summary>
        /// <remarks>
        /// The current implementation of the <see cref="Parser"/> has its focus
        /// on the <see cref="Tables.EIT"/> table of the <i>Electronic Program Guide (EPG)</i>.
        /// Future versions of this implementation will not be strongly bind <see cref="Descriptor"/>
        /// instances to <see cref="IDescriptorContainer"/> instances.
        /// </remarks>
        public readonly IDescriptorContainer Container = null;

        /// <summary>
        /// The overall length of the raw data in bytes.
        /// </summary>
        public readonly int Length = -1;

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="container">The corresponding event instance.
        /// <seealso cref="EntryBase"/>
        /// </param>
        /// <param name="offset">The first byte of the descriptor raw data in
        /// the related <see cref="Section"/>.</param>
        /// <param name="length">The number of bytes for this instance. Since this
        /// does not include the tag and the length <see cref="byte"/> <see cref="Length"/>
        /// will be two greater than the value of this parameter.</param>
        protected Descriptor(IDescriptorContainer container, int offset, int length)
        {
            // Set
            Tag = (DescriptorTags)container.Section[offset - 2];

            // Remember
            Container = container;
            Length = 2 + length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        protected Descriptor(DescriptorTags tag)
        {
            // Remember
            Tag = tag;
        }

        /// <summary>
        /// Locate the related descriptor tag and create a new instance.
        /// </summary>
        /// <remarks>
        /// The tag can be found at the offset <i>- 2</i>.
        /// </remarks>
        /// <param name="container">The corresponding event instance.
        /// <seealso cref="EntryBase"/>
        /// </param>
        /// <param name="offset">The first byte of the descriptor raw data in
        /// the related <see cref="Section"/>.</param>
        /// <param name="length">The number of bytes for this instance. Since this
        /// does not include the tag and the length <see cref="byte"/> <see cref="Length"/>
        /// will be two greater than the value of this parameter.</param>
        /// <returns>A newly created <see cref="Descriptor"/> instance.</returns>
        static public Descriptor Create(IDescriptorContainer container, int offset, int length)
        {
            // Attach to the type
            Type pHandler = (Type)m_HandlerForTag[container.Section[offset - 2]];

            // Create
            return (Descriptor)Activator.CreateInstance(pHandler, new object[] { container, offset, length });
        }

        /// <summary>
        /// Load all descriptors for an event instance.
        /// </summary>
        /// <remarks>
        /// No error will be reported if there is space left after the last decoded
        /// <see cref="Descriptor"/>. The returned <see cref="Array"/> may include instance
        /// with <see cref="IsValid"/> unset.
        /// </remarks>
        /// <param name="container">The corresponding event instance.
        /// <seealso cref="EntryBase"/>
        /// </param>
        /// <param name="offset">The first byte of the first descriptors raw data in
        /// the related <see cref="Section"/>.</param>
        /// <param name="length">The total number of bytes available in the
        /// event. New <see cref="Descriptor"/> instances are created until
        /// there is no space left.</param>
        /// <returns>All <see cref="Descriptor"/> instances for an event.</returns>
        static public Descriptor[] Load(IDescriptorContainer container, int offset, int length)
        {
            // Attach to data
            Section section = container.Section;

            // Helper
            ArrayList all = new ArrayList();

            // Process as long as possible
            for (; ; )
            {
                // No space for header
                if (length < 2) break;

                // Read tag and length
                int bytes = section[offset + 1];

                // Check space
                if ((2 + bytes) > length) break;

                // Create instance
                Descriptor pNew = Create(container, offset + 2, bytes);

                // Remember
                all.Add(pNew);

                // Adjust
                offset += pNew.Length;
                length -= pNew.Length;
            }

            // Create
            return (Descriptor[])all.ToArray(typeof(Descriptor));
        }

        /// <summary>
        /// Report if this instance is valid.
        /// </summary>
        public bool IsValid => m_Valid;

        /// <summary>
        /// The related <see cref="EPG.Table"/>.
        /// </summary>
        /// <remarks>
        /// This is the <see cref="EntryBase.Table"/> of our <see cref="EntryBase"/>.
        /// </remarks>
        public Table Table => Container.Container;

        /// <summary>
        /// The related <see cref="Section"/>.
        /// </summary>
        /// <remarks>
        /// This is the <see cref="EPG.Table.Section"/> of our <see cref="Table"/>.
        /// </remarks>
        public Section Section => Table.Section;

        /// <summary>
        /// Append the binary formatted descriptor to the buffer provided.
        /// </summary>
        /// <param name="buffer">Some buffer.</param>
        internal void CreateDescriptor(TableConstructor buffer)
        {
            // Write the tag
            buffer.Add((byte)Tag);

            // Position of length
            int lengthPos = buffer.CreateDynamicLength();

            // Add payload
            CreatePayload(buffer);

            // Update the length
            buffer.SetDynamicLength(lengthPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected virtual void CreatePayload(TableConstructor buffer)
        {
            // Must be implemented by derived classes
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Erweiterungsmethoden zum einfacheren Umgang mit SI Beschreibungen.
    /// </summary>
    public static class DescriptorExtensions
    {
        /// <summary>
        /// Sucht in einer Liste von SI Beschreibungen eine bestimmte Beschreibungsart.
        /// </summary>
        /// <typeparam name="T">Die Art der gesuchten Beschreibung.</typeparam>
        /// <param name="descriptors">Die Liste der zu durchsuchenden Beschreibungen.</param>
        /// <returns>Die gewünschte Beschreibung oder <i>null</i>, wenn keine Beschreibung
        /// der gesuchten Art in der Liste vorhanden ist.</returns>
        public static T Find<T>(this Descriptor[] descriptors) where T : Descriptor
        {
            // Not possible
            if (null == descriptors)
                return null;

            // Lookup
            return (T)Array.Find(descriptors, d => typeof(T) == d.GetType());
        }

    }
}
