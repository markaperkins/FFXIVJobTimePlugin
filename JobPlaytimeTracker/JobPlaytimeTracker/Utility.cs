namespace JobPlaytimeTracker.JobPlaytimeTracker
{
    internal class Utility
    {
        public static string CapitalizeString(string input)
        {
            // Gate function to return empty strings
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Split into tokens and capitalize the first letter of each token.
            // Return the merging of the tokens with space as a delimiter.
            string[] tokens = input.Split(' ');
            for(int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Length > 0)
                {
                    tokens[i] = char.ToUpper(tokens[i][0]) + tokens[i][1..];
                }
            }
            return string.Join(" ", tokens);
        }
    }
}
