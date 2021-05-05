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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.ReedSolomon
{
	/// <summary>
	/// Description of GeneratorPolynomial.
	/// </summary>
	internal sealed class GeneratorPolynomial
	{
		private readonly GaloisField256 m_gfield;
		
		private List<Polynomial> m_cacheGenerator;
		
		/// <summary>
		/// After create GeneratorPolynomial. Keep it as long as possible. 
		/// Unless QRCode encode is done or no more QRCode need to generate.
		/// </summary>
		internal GeneratorPolynomial(GaloisField256 gfield)
		{
			m_gfield = gfield;
			m_cacheGenerator = new List<Polynomial>(10);
			m_cacheGenerator.Add(new Polynomial(m_gfield, new int[]{1}));
		}
		
		/// <summary>
		/// Get generator by degree. (Largest degree for that generator)
		/// </summary>
		/// <returns>Generator</returns>
		internal Polynomial GetGenerator(int degree)
		{
			if(degree >= m_cacheGenerator.Count)
				BuildGenerator(degree);
			return m_cacheGenerator[degree];
		}
		
		/// <summary>
		/// Build Generator if we can not find specific degree of generator from cache
		/// </summary>
		private void BuildGenerator(int degree)
		{
			lock(m_cacheGenerator)
			{
				int currentCacheLength = m_cacheGenerator.Count;
				if(degree >= currentCacheLength)
				{
					Polynomial lastGenerator = m_cacheGenerator[currentCacheLength - 1];
				
					for(int d = currentCacheLength; d <= degree; d++)
					{
						Polynomial nextGenerator = lastGenerator.Multiply(new Polynomial(m_gfield, new int[]{1, m_gfield.Exponent(d - 1)}));
						m_cacheGenerator.Add(nextGenerator);
						lastGenerator = nextGenerator;
					}
				}
			}
		}
		
	}
}
