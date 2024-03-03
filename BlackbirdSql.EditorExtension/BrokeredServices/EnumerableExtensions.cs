// Microsoft.VisualStudio.LiveShare.VslsFileSystemProvider.VSCore, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.LiveShare.FileSystemProvider.EnumerableExtensions
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;


namespace BlackbirdSql.BrokeredServices;

internal static class EnumerableExtensions
{
	public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
	{
		return Requires.NotNull(items, "items").FlattenInternal(Requires.NotNull(childSelector, "childSelector"));
	}

	private static IEnumerable<T> FlattenInternal<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
	{
		Func<T, IEnumerable<T>> childSelector2 = childSelector;
		return items.Concat(items.SelectMany(delegate (T c)
		{
			IEnumerable<T> enumerable = childSelector2(c);
			return enumerable != null ? enumerable.FlattenInternal(childSelector2) : Enumerable.Empty<T>();
		}));
	}

	public static async IAsyncEnumerable<R> AsAsync<T, R>(Func<Task<IEnumerable<T>>> items, Func<T, CancellationToken, Task<R>> selector, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		Requires.NotNull(items, "items");
		Requires.NotNull(selector, "selector");
		foreach (T item in await items())
		{
			yield return await selector(item, cancellationToken);
		}
	}

	public static IAsyncEnumerable<T> AsAsync<T>(Func<Task<IEnumerable<T>>> items, CancellationToken cancellationToken)
	{
		return AsAsync(items, (item, _) => Task.FromResult(item), cancellationToken);
	}
}