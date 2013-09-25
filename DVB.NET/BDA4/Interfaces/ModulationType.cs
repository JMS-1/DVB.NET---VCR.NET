using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die verschiedenen Modulationsarten.
    /// </summary>
    public enum ModulationType
    {
        /// <summary>
        /// Not defined.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM16 = 1,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM32 = 2,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM64 = 3,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM804 = 4,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM96 = 5,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM112 = 6,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM128 = 7,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM160 = 8,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM192 = 9,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM224 = 10,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM256 = 11,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM320 = 12,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM384 = 13,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM448 = 14,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM512 = 15,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM640 = 16,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM768 = 17,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM896 = 18,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QAM1024 = 19,

        /// <summary>
        /// (Don't know).
        /// </summary>
        QPSK = 20,

        /// <summary>
        /// (Don't know).
        /// </summary>
        BPSK = 21,

        /// <summary>
        /// (Don't know).
        /// </summary>
        OQPSK = 22,

        /// <summary>
        /// (Don't know).
        /// </summary>
        VSB8 = 23,

        /// <summary>
        /// (Don't know).
        /// </summary>
        VSB16 = 24,

        /// <summary>
        /// (Don't know).
        /// </summary>
        AnalogAmplitude = 25,

        /// <summary>
        /// (Don't know).
        /// </summary>
        AnalogFrequency = 26,

        /// <summary>
        /// (Don't know).
        /// </summary>        
        PSK8 = 27,

        /// <summary>
        /// (Don't know).
        /// </summary>
        RF = 28,

        /// <summary>
        /// (Don't know).
        /// </summary>
        APSK16 = 29,

        /// <summary>
        /// (Don't know).
        /// </summary>
        APSK32 = 30,

        /// <summary>
        /// (Don't know).
        /// </summary>
        NBCQPSK = 31,

        /// <summary>
        /// (Don't know).
        /// </summary>
        NBC8PSK = 32,

        /// <summary>
        /// (Don't know).
        /// </summary>
        DIRECTV = 33,

        /// <summary>
        /// (Don't know).
        /// </summary>
        ISDBTTMCC = 34,

        /// <summary>
        /// (Don't know).
        /// </summary>
        ISDBSTMCC = 35,

        /// <summary>
        /// Maximum allowed enumeration value.
        /// </summary>
        Maximum = 36,

        /// <summary>
        /// Mark parameter as not set.
        /// </summary>
        NotSet = -1
    }
}
