using System;

namespace ProducerInterfaceCommon.Models
{
    /// <summary>
    /// Interface for interval reports
    /// </summary>
    public interface IInterval
    {
        DateTime DateFrom { get; set; }

        DateTime DateTo { get; set; }

     }
}
