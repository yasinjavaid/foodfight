using UnityEngine;

namespace Kit.Algorithms
{
	/// <summary>
	///     Computes the Levenshtein/Edit distance of two strings. It is the number of character edits needed to turn one string into another.
	///     See <see href="https://en.wikipedia.org/wiki/Edit_distance" /> and <see href="https://en.wikipedia.org/wiki/Levenshtein_distance" />.
	/// </summary>
	/// <remarks>In layman's terms, it tells how similar two strings are in terms of composition. Can be used to detect, and correct, typos.</remarks>
	public static class LevenshteinDistance
	{
		/// <summary>Computes the Levenshtein/Edit distance of two strings.</summary>
		/// <returns>0, if two strings are equal, or a positive number denoting the number of character edits required to make them.</returns>
		public static int Compute(string first, string second)
		{
			if (first.Length == 0)
				return second.Length;

			if (second.Length == 0)
				return first.Length;

			int[,] d = new int[first.Length + 1, second.Length + 1];
			for (int i = 0; i <= first.Length; i++)
				d[i, 0] = i;

			for (int j = 0; j <= second.Length; j++)
				d[0, j] = j;

			for (int i = 1; i <= first.Length; i++)
				for (int j = 1; j <= second.Length; j++)
				{
					int cost = second[j     - 1] == first[i     - 1] ? 0 : 1;
					d[i, j] = Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1, d[i - 1, j - 1] + cost);
				}

			return d[first.Length, second.Length];
		}
	}
}