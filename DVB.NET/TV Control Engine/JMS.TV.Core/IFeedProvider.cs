using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core
{
    /// <summary>
    /// Wird von einer Komponente angeboten, die Sender zur Verfügung stellt
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    public interface IFeedProvider<TSourceType>
    {
    }
}
