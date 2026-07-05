using System;
/*

 */

namespace CastIt.GoogleCast.Zeroconf.Dns
{
    class RecordNSEC : Record
	{
		public byte[] RDATA;

		public RecordNSEC(RecordReader rr)
		{
			// re-read length
			var RDLENGTH = rr.ReadUInt16(-2);

			RDATA = rr.ReadBytes(RDLENGTH);
		}

		public override string ToString()
		{
			return string.Format("not-used");
		}

	}
}
