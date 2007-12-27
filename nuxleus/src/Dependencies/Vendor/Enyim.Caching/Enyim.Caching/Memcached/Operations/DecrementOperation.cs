﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading;

namespace Enyim.Caching.Memcached
{
	internal sealed class DecrementOperation : ItemOperation
	{
		private uint amount;
		private uint result;

		internal DecrementOperation(string key, uint amount)
			: base(key)
		{
			this.amount = amount;
		}

		protected override bool ExecuteAction()
		{
			this.Socket.SendCommand(String.Concat("decr ", this.HashedKey, " ", this.amount.ToString(CultureInfo.InvariantCulture)));

			string response = this.Socket.ReadResponse();

			//maybe we should throw an exception when the item is not found?
			if (String.Compare(response, "NOT_FOUND", StringComparison.Ordinal) == 0)
				return false;

			return UInt32.TryParse(response, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture,  out this.result);
		}

		public uint Result
		{
			get { return this.result; }
		}
	}
}

#region [ License information          ]
/* ************************************************************
 *
 * Copyright (c) Attila Kiskó, enyim.com, 2007
 *
 * This source code is subject to terms and conditions of 
 * Microsoft Permissive License (Ms-PL).
 * 
 * A copy of the license can be found in the License.html
 * file at the root of this distribution. If you can not 
 * locate the License, please send an email to a@enyim.com
 * 
 * By using this source code in any fashion, you are 
 * agreeing to be bound by the terms of the Microsoft 
 * Permissive License.
 *
 * You must not remove this notice, or any other, from this
 * software.
 *
 * ************************************************************/
#endregion