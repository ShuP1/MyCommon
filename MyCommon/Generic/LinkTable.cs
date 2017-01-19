using System.Collections.Generic;

namespace MyCommon.Generic
{
	/// <summary>
	/// Two way dictionary
	/// </summary>
	public class LinkTable<TMain, TSecond>
	{
		private Dictionary<TMain, TSecond> mainTable;
		private Dictionary<TSecond, TMain> secondTable;
		public Dictionary<TMain, TSecond> Main { get { return mainTable; } }
		public Dictionary<TSecond, TMain> Second { get { return secondTable; } }
		public int Count { get { return mainTable.Count; } }

		public LinkTable()
		{
			mainTable = new Dictionary<TMain, TSecond>();
			secondTable = new Dictionary<TSecond, TMain>();
		}

		public void Add(TMain main, TSecond second)
		{
			mainTable.Add(main, second);
			secondTable.Add(second, main);
		}

		public bool TryAdd(TMain main, TSecond second)
		{
			try
			{
				Add(main, second);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Clear()
		{
			mainTable.Clear();
			secondTable.Clear();
		}

		public bool ContainsMain(TMain main)
		{
			return mainTable.ContainsKey(main);
		}

		public bool ContainsSecond(TSecond second)
		{
			return secondTable.ContainsKey(second);
		}

		public TMain GetMain(TSecond second)
		{
			return secondTable[second];
		}

		public TSecond GetSecond(TMain main)
		{
			return mainTable[main];
		}

		public bool TryGetMain(TSecond second, out TMain main)
		{
			return secondTable.TryGetValue(second, out main);
		}

		public bool TryGetSecond(TMain main, out TSecond second)
		{
			return mainTable.TryGetValue(main, out second);
		}

		public void RemoveMain(TMain main)
		{
			secondTable.Remove(mainTable[main]);
			mainTable.Remove(main);
		}

		public void RemoveSecond(TSecond second)
		{
			mainTable.Remove(secondTable[second]);
			secondTable.Remove(second);
		}

		public bool TryRemoveMain(TMain main)
		{
			if (!mainTable.ContainsKey(main))
				return false;

			if (!secondTable.ContainsKey(mainTable[main]))
				return false;

			try
			{
				RemoveMain(main);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool TryRemoveSecond(TSecond second)
		{
			if (!secondTable.ContainsKey(second))
				return false;

			if (!mainTable.ContainsKey(secondTable[second]))
				return false;

			try
			{
				RemoveSecond(second);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public Dictionary<TMain, TSecond>.Enumerator GetEnumerator()
		{
			return mainTable.GetEnumerator();
		}
	}
}
