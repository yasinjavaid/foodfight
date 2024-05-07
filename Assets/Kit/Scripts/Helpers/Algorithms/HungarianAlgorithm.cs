﻿using System;

namespace Kit.Algorithms
{
	/// <summary>Finds the optimal assignment of tasks when worker costs for doing each are given.</summary>
	/// <remarks>See <see href="https://en.wikipedia.org/wiki/Hungarian_algorithm" />.</remarks>
	// Copyright (c) 2010 Alex Regueiro
	// Licensed under MIT license, available at <http://www.opensource.org/licenses/mit-license.php>.
	// Published originally at <http://blog.noldorin.com/2009/09/hungarian-algorithm-in-csharp/>.
	// Based on implementation described at <http://www.public.iastate.edu/~ddoty/HungarianAlgorithm.html>.
	// Version 1.3, released 22nd May 2010.
	public static class HungarianAlgorithm
	{
		/// <summary>Given a matrix of worker costs and tasks, find the optimal allocation of workers such that the total cost is minimized.</summary>
		/// <param name="costs">A 2D-matrix where the element at [i, j] represents the cost of worker i doing task j.</param>
		/// <returns>An array where the value at index i is the index of the task assigned to worker i.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="costs" /> is <see langword="null" />.</exception>
		public static int[] FindAssignments(this int[,] costs)
		{
			if (costs == null)
				throw new ArgumentNullException(nameof(costs));

			int h = costs.GetLength(0);
			int w = costs.GetLength(1);

			for (int i = 0; i < h; i++)
			{
				int min = int.MaxValue;
				for (int j = 0; j < w; j++)
					min = Math.Min(min, costs[i, j]);
				for (int j = 0; j < w; j++)
					costs[i, j] -= min;
			}

			byte[,] masks = new byte[h, w];
			bool[] rowsCovered = new bool[h];
			bool[] colsCovered = new bool[w];
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
					{
						masks[i, j] = 1;
						rowsCovered[i] = true;
						colsCovered[j] = true;
					}

			ClearCovers(rowsCovered, colsCovered, w, h);

			var path = new Location[w * h];
			Location pathStart = default;
			int step = 1;
			while (step != -1)
				switch (step)
				{
					case 1:
						step = RunStep1(masks, colsCovered, w, h);
						break;
					case 2:
						step = RunStep2(costs, masks, rowsCovered, colsCovered, w, h, ref pathStart);
						break;
					case 3:
						step = RunStep3(masks, rowsCovered, colsCovered, w, h, path, pathStart);
						break;
					case 4:
						step = RunStep4(costs, rowsCovered, colsCovered, w, h);
						break;
				}

			int[] agentsTasks = new int[h];
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (masks[i, j] == 1)
					{
						agentsTasks[i] = j;
						break;
					}

			return agentsTasks;
		}

		private static int RunStep1(byte[,] masks, bool[] colsCovered, int w, int h)
		{
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (masks[i, j] == 1)
						colsCovered[j] = true;
			int colsCoveredCount = 0;
			for (int j = 0; j < w; j++)
				if (colsCovered[j])
					colsCoveredCount++;

			return colsCoveredCount == h ? -1 : 2;
		}

		private static int RunStep2(int[,] costs,
									byte[,] masks,
									bool[] rowsCovered,
									bool[] colsCovered,
									int w,
									int h,
									ref Location pathStart)
		{
			while (true)
			{
				Location loc = FindZero(costs, rowsCovered, colsCovered, w, h);
				if (loc.Row == -1)
					return 4;

				masks[loc.Row, loc.Column] = 2;
				int starCol = FindStarInRow(masks, w, loc.Row);
				if (starCol != -1)
				{
					rowsCovered[loc.Row] = true;
					colsCovered[starCol] = false;
				}
				else
				{
					pathStart = loc;
					return 3;
				}
			}
		}

		private static int RunStep3(byte[,] masks,
									bool[] rowsCovered,
									bool[] colsCovered,
									int w,
									int h,
									Location[] path,
									Location pathStart)
		{
			int pathIndex = 0;
			path[0] = pathStart;
			while (true)
			{
				int row = FindStarInColumn(masks, h, path[pathIndex].Column);
				if (row == -1)
					break;
				pathIndex++;
				path[pathIndex] = new Location(row, path[pathIndex - 1].Column);
				int col = FindPrimeInRow(masks, w, path[pathIndex].Row);
				pathIndex++;
				path[pathIndex] = new Location(path[pathIndex - 1].Row, col);
			}

			ConvertPath(masks, path, pathIndex + 1);
			ClearCovers(rowsCovered, colsCovered, w, h);
			ClearPrimes(masks, w, h);
			return 1;
		}

		private static int RunStep4(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
		{
			int minValue = FindMinimum(costs, rowsCovered, colsCovered, w, h);
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
				{
					if (rowsCovered[i])
						costs[i, j] += minValue;
					if (!colsCovered[j])
						costs[i, j] -= minValue;
				}

			return 2;
		}

		private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
		{
			for (int i = 0; i < pathLength; i++)
				if (masks[path[i].Row, path[i].Column] == 1)
					masks[path[i].Row, path[i].Column] = 0;
				else if (masks[path[i].Row, path[i].Column] == 2)
					masks[path[i].Row, path[i].Column] = 1;
		}

		private static Location FindZero(int[,] costs,
										 bool[] rowsCovered,
										 bool[] colsCovered,
										 int w,
										 int h)
		{
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
						return new Location(i, j);
			return new Location(-1, -1);
		}

		private static int FindMinimum(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
		{
			int minValue = int.MaxValue;
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (!rowsCovered[i] && !colsCovered[j])
						minValue = Math.Min(minValue, costs[i, j]);
			return minValue;
		}

		private static int FindStarInRow(byte[,] masks, int w, int row)
		{
			for (int j = 0; j < w; j++)
				if (masks[row, j] == 1)
					return j;
			return -1;
		}

		private static int FindStarInColumn(byte[,] masks, int h, int col)
		{
			for (int i = 0; i < h; i++)
				if (masks[i, col] == 1)
					return i;
			return -1;
		}

		private static int FindPrimeInRow(byte[,] masks, int w, int row)
		{
			for (int j = 0; j < w; j++)
				if (masks[row, j] == 2)
					return j;
			return -1;
		}

		private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
		{
			for (int i = 0; i < h; i++)
				rowsCovered[i] = false;
			for (int j = 0; j < w; j++)
				colsCovered[j] = false;
		}

		private static void ClearPrimes(byte[,] masks, int w, int h)
		{
			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					if (masks[i, j] == 2)
						masks[i, j] = 0;
		}

		private struct Location
		{
			public int Row;
			public int Column;

			public Location(int row, int col)
			{
				Row = row;
				Column = col;
			}
		}
	}
}