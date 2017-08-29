using System;
using System.Collections;
using System.IO.Ports;

namespace ETC.Platforms
{
	/// <summary>
	/// A simple interface the Enttec DMX USB Pro and DMX USB Pro Mk2.
	/// </summary>
	/// <remarks>
	/// Author: Bryan Maher (bm3n@andrew.cmu.edu) 25-Feb-2015
	/// 
	/// The following are some of the reference materials used to create this library:
	/// 
	///   DMX512 Primer: 
	///       http://www.dfd.com/primer.html
	/// 
	///   THE DMX512 PACKET: 
	///       http://www.dmx512-online.com/packt.html
	/// 
	///   API for DMX USB PRO:
	///       http://www.enttec.com/docs/dmx_usb_pro_api_spec.pdf
	/// 
	///   And prior work by ETC alum, Gordon "Jake" Jeffery.
	/// 
	/// Some Considerations:
	///   1. All 512 channels are sent in every DMX message frame which limits refresh rates to roughly 44hz.
	///   2. The USB Pro MK2 is accessed in a legacy fashion which only allows access to the DMX1 universe.
	///   3. Uses System.IO.Ports namespace which requires Player Settings | Optimizations | API Compatibility Level
	///      be set to ".NET 2.0".  The default setting is ".NET 2.0 subset" which does not suppport serial IO.
    ///
    /// History:
    /// 2015-12-11: bm3n    : Added destructor to release managed resources.
    /// 2015-02-25: bm3n    : Created.
	/// </remarks>
	public class DMX 
	{
		#region ===== INTERNAL CLASSES
		/// <summary>
		/// Allows DMX channel data to be accessed as an array with bounds of 1 to 512 inclusive.
		/// </summary>
		/// <remarks>
		/// Based on Indexed Properties Tutorial:
		///     https://msdn.microsoft.com/en-us/library/aa288464(v=vs.71).aspx	
		/// </remarks>
		public class DmxChannelCollection
		{
			/// <summary>
			/// The DMX instance that parents this collection.
			/// </summary>
			readonly DMX dmx; 

			/// <summary>
			/// Initializes a new instance of the <see cref="DMX+DmxChannelCollection"/> class.
			/// </summary>
			/// <param name="dmx">Dmx.</param>
			internal DmxChannelCollection(DMX dmx)
			{
				this.dmx = dmx;
			}

			/// <summary>
			/// Gets or sets the <see cref="DMX+DmxChannelCollection"/> at the specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			public byte this[int index]
			{
				get 
				{
					if (index < 1 || index > 512)
						throw new IndexOutOfRangeException();
					else
						return this.dmx.buffer[index + DMX.INDEX_OFFSET];
				}
				set
				{
					if (index < 1 || index > 512)
						throw new IndexOutOfRangeException();
					else
						this.dmx.buffer[index + DMX.INDEX_OFFSET] = value;
				}
			}
		}
		#endregion

		#region ===== PRIVATE DECLARATIONS
		/// <summary>
		/// The length of the DMX buffer.
		/// </summary>
		/// Refer to <see cref="buffer"/> for details of buffer layout.
		private const int BUFFER_LENGTH = 518;

		/// <summary>
		/// DMX message buffer.  
		/// 
		/// Bytes  Index  Description
		/// ------+------+--------------------------------------------------
		///    1     0    0x7E - Start of USB Pro Message
		///    1     1    0x06 - Instructs USB Pro to send DMX Data
		///    1     2    0x01 - LSB of DMX data length (513 = 512 + SC)
		///    1     3    0x10 - MSB of DMX data length (513 = 512 + SC)
		///    1     4    0x00 - DMX Start Code (SC) 
		///   512  5-516  DMX Channel Data
		///    1    517   0xE7 - End of USB Pro Message
		/// ------+----------------------------------------------------------
		///  518  = 512 channels + 6 bytes message overhead.
		/// </summary>
		private byte[] buffer = new byte[BUFFER_LENGTH];

		/// <summary>
		/// The offset between a DMX channel number and it's corresponding
		/// index in the buffer.
		/// </summary>
		private const int INDEX_OFFSET = 4;

		/// <summary>
		/// The instance of the serial port used to communicate with the DMX controller.
		/// </summary>
		public SerialPort dmxPort;
		#endregion

		/// <summary>
		/// Gets or sets the data for a given DMX channel in the range of 1 to 512 inclusive.
		/// </summary>
		public readonly DmxChannelCollection Channels;

		/// <summary>
		/// Initializes a new instance of the Enttec <see cref="DMX"/> USB Pro interface.
		/// </summary>
		public DMX(string COMport)
		{            
			this.Channels = new DmxChannelCollection(this);

			// Initialize the DMX Message Array with zeros
			for(int i=0; i<BUFFER_LENGTH; i++) this.buffer[i] = (byte)0x00;

			// Initialize pre and post amble.
			this.buffer[000] = (byte)0x7E;   // ENTTEC Start Of Message delimiter
			this.buffer[001] = (byte)0x06;   // ENTTEC Message Label
			this.buffer[002] = (byte)0x01;   // Data Length / LSB of 513
			this.buffer[003] = (byte)0x02;   // Data Length / MSB of 513
			this.buffer[004] = (byte)0x00;   // DMX Start Code
			this.buffer[517] = (byte)0xE7;   // ENTTEC End Of Message delimiter

			// HOWTO: Specify Serial Ports Larger than COM9: 
			//     http://support.microsoft.com/kb/115831
			this.dmxPort = new SerialPort(@"\\.\" + COMport);

			this.dmxPort.Open();
		}

        /// <summary>
        /// Release unmanaged resources.
        /// </summary>
        ~DMX()
        {
            dmxPort.Close();
            dmxPort.Dispose();
        }

		/// <summary>
		/// Send the DMX data.
		/// </summary>
		public void Send()
		{
			this.dmxPort.Write(buffer, 0, BUFFER_LENGTH);
		}
	}
}