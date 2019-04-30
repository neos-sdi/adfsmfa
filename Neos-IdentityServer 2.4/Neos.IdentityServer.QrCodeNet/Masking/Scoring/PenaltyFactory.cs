//******************************************************************************************************************************************************************************************//
// Copyright (c) 2011 George Mamaladze                                                                                                                                                      //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Masking.Scoring
{
	/// <summary>
	/// Description of PenaltyFactory.
	/// </summary>
	internal class PenaltyFactory
	{
		
		internal Penalty CreateByRule(PenaltyRules penaltyRule)
		{
			switch(penaltyRule)
			{
				case PenaltyRules.Rule01:
					return new Penalty1();
				case PenaltyRules.Rule02:
					return new Penalty2();
				case PenaltyRules.Rule03:
					return new Penalty3();
				case PenaltyRules.Rule04:
					return new Penalty4();
				default:
					throw new ArgumentException(string.Format("Unsupport penalty rule : {0}", penaltyRule), "penaltyRule");
					
			}
		}
		
		
		
		internal IEnumerable<Penalty> AllRules()
		{
			foreach(PenaltyRules penaltyRule in Enum.GetValues(typeof(PenaltyRules)))
			{
				yield return CreateByRule(penaltyRule);
			}
		}
	}
}
