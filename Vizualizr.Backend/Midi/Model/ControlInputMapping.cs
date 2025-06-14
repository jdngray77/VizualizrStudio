using System.Xml.Serialization;
using Vizualizr.Backend.Utility;

namespace Vizualizr.Backend.Midi.FileModel
{
    public class ControlInputMapping : BaseMidiInputMapping
    {
        /// <summary>
        /// True Indicates that the control has a maximum and minimum physical position.
        /// i.e it does not continually rotate.
        ///
        /// Indicates that the control continually rotates, and the signals
        /// declare relative movement in one direction or another.
        /// </summary>
        [XmlAttribute]
        public bool Absolute { get; set; } = false;

        // /// <summary>
        // /// When relative (Absolute = false) indicates the control value that indicates
        // /// a positive relative change.
        // ///
        // /// i.e set to 127 if 127 is sent whilst turning the control in the direction that should
        // ///     mean 'increase'
        // /// </summary>
        // [XmlAttribute]
        // public byte IncrementValue { get; set; } = 127;
        //
        // /// <summary>
        // /// When relative (Absolute = true) indicates the control value that indicates
        // /// a negative relative change.
        // ///
        // /// i.e set to 127 if 127 is sent whilst turning the control in the direction that should
        // ///     mean 'decrease'
        // /// </summary>
        // [XmlAttribute]
        // public byte DecrementValue { get; set; } = 1;

        [XmlAttribute]
        public byte MinimumValue { get; set; } = 0;

        [XmlAttribute]
        public byte MaximumValue { get; set; } = 127;

        public byte CoerceValue(byte value)
        {
            return Math.Clamp(value, MinimumValue, MaximumValue);   
        }

        public void Validate()
        {
            Absolute.ThrowIfNull();
            MinimumValue.ThrowIfOutsideRange(0, 127);
            MaximumValue.ThrowIfOutsideRange(0, 127);
            MinimumValue.ThrowIfGreaterThan(MaximumValue);
            //IncrementValue.ThrowIfOutsideRange(MinimumValue, MaximumValue);
        }
    }
}