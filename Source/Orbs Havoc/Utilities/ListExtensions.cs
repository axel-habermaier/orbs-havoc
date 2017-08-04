namespace OrbsHavoc.Utilities
{
	using System.Collections.Generic;

	public static class ListExtensions
	{
		public static void AddRange<T>(this List<T> list, T item1)
		{
			Assert.ArgumentNotNull(list, nameof(list));
			list.Add(item1);
		}

		public static void AddRange<T>(this List<T> list, T item1, T item2)
		{
			Assert.ArgumentNotNull(list, nameof(list));

			list.Add(item1);
			list.Add(item2);
		}

		public static void AddRange<T>(this List<T> list, T item1, T item2, T item3)
		{
			Assert.ArgumentNotNull(list, nameof(list));

			list.Add(item1);
			list.Add(item2);
			list.Add(item3);
		}

		public static void AddRange<T>(this List<T> list, T item1, T item2, T item3, T item4)
		{
			Assert.ArgumentNotNull(list, nameof(list));

			list.Add(item1);
			list.Add(item2);
			list.Add(item3);
			list.Add(item4);
		}

		public static void AddRange<T>(this List<T> list, params T[] items)
		{
			Assert.ArgumentNotNull(list, nameof(list));
			list.AddRange(items);
		}
	}
}