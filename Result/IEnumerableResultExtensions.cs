
namespace projecthelper.Result;

public static class IEnumerableResultExtensions
{
    public static IEnumerable<Result<T>> ThrowAnyFail<T>(this IEnumerable<Result<T>> result)
    {
        return result.Select(r => r.ThrowIfFail());
    }
}
