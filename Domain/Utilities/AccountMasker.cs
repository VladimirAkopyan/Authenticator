using System;
using System.Linq;
using System.Text;

namespace Domain.Utilities
{
    public abstract class AccountMasker
    {
        public static string Mask(string id)
        {
            char[] fromCharacters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] toCharacters = new char[] { 'e', '2', 'x', 't', '7', '9', 'q', 's', 'd', 'j', '8', '3', 'm', 'p', 'l', 'g', 'c', '6', 'w', 'z', '5', 'a', 'h', 'k', 'n', '1', 'i', 'o', 'b', 'y', '0', 'r', 'f', 'u', 'v', '4' };

            StringBuilder builder = new StringBuilder(id.Length);

            for (int i = 0; i < id.Length; i++)
            {
                char currentCharacter = id[i];
                int characterIndex = Array.IndexOf(fromCharacters, currentCharacter);

                builder.Append(toCharacters[characterIndex]);
            }

            return builder.ToString();
        }
    }
}
