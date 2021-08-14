using System.Collections.Generic;
using System.Linq;

public static class Helpers
{
  public static T PickOne<T>(this IEnumerable<T> col)
  {
    var enumerable = col as T[] ?? col.ToArray();
    return enumerable[RandomService.GetRandom(0, enumerable.Count())];
  }

  public static bool IsEmpty<T>(this IEnumerable<T> col) => !col.Any();
}
