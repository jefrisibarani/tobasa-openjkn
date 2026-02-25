/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Text;
using System.Text.Json;

namespace Tobasa.App
{
    // https://github.com/J0rgeSerran0/JsonNamingPolicy/blob/master/JsonSnakeCaseNamingPolicy.cs
    
    
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private readonly string _separator = "_";
    
        public override string ConvertName(string name)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name)) return String.Empty;

            ReadOnlySpan<char> spanName = name.Trim();

            var stringBuilder = new StringBuilder();
            var addCharacter = true;

            var isPreviousSpace = false;
            var isPreviousSeparator = false;
            var isCurrentSpace = false;
            var isNextLower = false;
            var isNextUpper = false;
            var isNextSpace = false;

            for (int position = 0; position < spanName.Length; position++)
            {
                if (position != 0)
                {
                    isCurrentSpace = spanName[position] == 32;
                    isPreviousSpace = spanName[position - 1] == 32;
                    isPreviousSeparator = spanName[position - 1] == 95;

                    if (position + 1 != spanName.Length)
                    {
                        isNextLower = spanName[position + 1] > 96 && spanName[position + 1] < 123;
                        isNextUpper = spanName[position + 1] > 64 && spanName[position + 1] < 91;
                        isNextSpace = spanName[position + 1] == 32;
                    }

                    if ((isCurrentSpace) &&
                        ((isPreviousSpace) || 
                        (isPreviousSeparator) || 
                        (isNextUpper) || 
                        (isNextSpace)))
                        addCharacter = false;
                    else
                    {
                        var isCurrentUpper = spanName[position] > 64 && spanName[position] < 91;
                        var isPreviousLower = spanName[position - 1] > 96 && spanName[position - 1] < 123;
                        var isPreviousNumber = spanName[position - 1] > 47 && spanName[position - 1] < 58;

                        if ((isCurrentUpper) &&
                        ((isPreviousLower) || 
                        (isPreviousNumber) || 
                        (isNextLower) || 
                        (isNextSpace) || 
                        (isNextLower && !isPreviousSpace)))
                            stringBuilder.Append(_separator);
                        else
                        {
                            if ((isCurrentSpace && 
                                !isPreviousSpace && 
                                !isNextSpace))
                            {
                                stringBuilder.Append(_separator);
                                addCharacter = false;
                            }
                        }
                    }
                }

                if (addCharacter)
                    stringBuilder.Append(spanName[position]);
                else
                    addCharacter = true;
            }

            return stringBuilder.ToString().ToLower();
        }
    }
}