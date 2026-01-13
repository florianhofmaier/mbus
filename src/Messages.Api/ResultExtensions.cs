using Microsoft.FSharp.Core;

namespace Mbus;

public static class ResultExtensions
{
    public static FSharpResult<TNew, TError> Map<T, TError, TNew>(
        this FSharpResult<T, TError> result,
        Func<T, TNew> mapper)
    {
        return result.IsError
            ? FSharpResult<TNew, TError>.NewError(result.ErrorValue)
            : FSharpResult<TNew, TError>.NewOk(mapper(result.ResultValue));
    }

    public static T ValueOrThrow<T, TError>(this FSharpResult<T, TError> result, Func<TError, Exception> exceptionFactory)
    {
        if (result.IsError)
        {
            throw exceptionFactory(result.ErrorValue);
        }
        return result.ResultValue;
    }

    public static TNew MapOrThrow<T, TError, TNew>(
        this FSharpResult<T, TError> result,
        Func<T, TNew> mapper,
        Func<TError, Exception> exceptionFactory)
    {
        if (result.IsError)
        {
            throw exceptionFactory(result.ErrorValue);
        }
        return mapper(result.ResultValue);
    }
}
