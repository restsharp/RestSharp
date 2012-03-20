//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Xml;

namespace System.Xml.Linq
{
	public class XObjectChangeEventArgs : EventArgs
	{
		public XObjectChangeEventArgs (XObjectChange change)
		{
			this.type = change;
		}

		// Note that those fields cannot be directly referenced in
		// any object comparisons, as there could be other instances.
		public static readonly XObjectChangeEventArgs Add =
			new XObjectChangeEventArgs (XObjectChange.Add);
		public static readonly XObjectChangeEventArgs Name =
			new XObjectChangeEventArgs (XObjectChange.Name);
		public static readonly XObjectChangeEventArgs Remove =
			new XObjectChangeEventArgs (XObjectChange.Remove);
		public static readonly XObjectChangeEventArgs Value =
			new XObjectChangeEventArgs (XObjectChange.Value);

		public XObjectChange ObjectChange {
			get { return type; }
		}

		XObjectChange type;
	}
}
