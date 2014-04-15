

namespace JMS.TV.Core
{
    /// <summary>
    /// Die Steuereinheit für die Fernsehanzeige.
    /// </summary>
    public class TvController
    {
        /// <summary>
        /// Erstellt eine neue Steuereinheit.
        /// </summary>
        private TvController()
        {
        }

        /// <summary>
        /// Erstellt eine neue Steuereinheit.
        /// </summary>
        /// <returns>Die gewünschte Steuereinheit.</returns>
        public static TvController Create()
        {
            return new TvController();
        }
    }
}
