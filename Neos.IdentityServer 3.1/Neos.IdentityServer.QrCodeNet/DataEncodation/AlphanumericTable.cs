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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation
{
	/// <summary>
	/// Table at chapter 8.4.3. P.21
	/// </summary>
	internal class AlphanumericTable
	{
		private static readonly Dictionary<char, int> s_AlphanumericTable = 
			new Dictionary<char, int>
		{
			{'0', 0},
			{'1', 1},
			{'2', 2},
			{'3', 3},
			{'4', 4},
			{'5', 5},
			{'6', 6},
			{'7', 7},
			{'8', 8},
			{'9', 9},
			{'A', 10},
			{'B', 11},
			{'C', 12},
			{'D', 13},
			{'E', 14},
			{'F', 15},
			{'G', 16},
			{'H', 17},
			{'I', 18},
			{'J', 19},
			{'K', 20},
			{'L', 21},
			{'M', 22},
			{'N', 23},
			{'O', 24},
			{'P', 25},
			{'Q', 26},
			{'R', 27},
			{'S', 28},
			{'T', 29},
			{'U', 30},
			{'V', 31},
			{'W', 32},
			{'X', 33},
			{'Y', 34},
			{'Z', 35},
			{'\x0020', 36},  //"SP"
			{'\x0024', 37},  //"$"
			{'\x0025', 38},  //"%" 
			{'\x002A', 39},  //"*"
			{'\x002B', 40},  //"+"
			{'\x002D', 41},  //"-"
			{'\x002E', 42},  //"."
			{'\x002F', 43}, //"/"
			{'\x003A', 44},	//":"
		};
		
		/// <summary>
		/// Convert char to int value
		/// </summary>
		/// <param name="inputChar">Alpha Numeric Char</param>
		/// <remarks>Table from chapter 8.4.3 P21</remarks>
		internal static int ConvertAlphaNumChar(char inputChar)
		{
	        int value;
	        if (!s_AlphanumericTable.TryGetValue(inputChar, out value))
	        {
	            throw new ArgumentOutOfRangeException(
                    "inputChar", 
	                "Not an alphanumeric character found. Only characters from table from chapter 8.4.3 P21 are supported in alphanumeric mode.");
	        }
		    return value;
		}
		
		internal static bool Contains(char inputChar)
		{
			return s_AlphanumericTable.ContainsKey(inputChar);
		}
	}
}
