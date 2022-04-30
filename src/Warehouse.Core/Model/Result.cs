using LanguageExt;

namespace Warehouse.Core.Model;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ValueIsNotErrorException : Exception
{
    public ValueIsNotErrorException() : base("Value is not an error")
    {
    }

    public ValueIsNotErrorException(string? message) : base(message)
    {
    }

    public ValueIsNotErrorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class ValueIsNotSuccessException : Exception
{
    public ValueIsNotSuccessException() : base("Value is not an success")
    {
    }

    public ValueIsNotSuccessException(string? message) : base(message)
    {
    }

    public ValueIsNotSuccessException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public readonly struct Result<A> : IEquatable<Result<A>>
{
    internal enum ResultState : byte
    {
        Succeed,
        Error
    }

    internal readonly A? Success;
    internal readonly Exception? Error;

    internal readonly ResultState _state;

    public bool IsSucceed => _state == ResultState.Succeed;

    public Result(A success)
    {
        Success = success;
        Error = default;
        _state = ResultState.Succeed;
    }

    public Result(Exception error)
    {
        Success = default;
        Error = error;
        _state = ResultState.Error;
    }

    public static Result<A> Ok(A? success)
    {
        ArgumentNullException.ThrowIfNull(success, nameof(success));
        return new Result<A>(success);
    }

    public static Result<A> Failure(Exception error)
    {
        return new Result<A>(error);
    }

    public Result<C> Map<C>(Func<A, C> func)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(func(Success!));
    }

    public async Task<Result<C>> MapAsync<C>(Func<A, Task<C>> func)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(await func(Success!).ConfigureAwait(false));
    }

    public Result<C> Map<C, CD>(Func<A, CD, C> func, CD param1)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(func(Success!, param1));
    }

    public Result<C> Map<C, CD, CD1>(Func<A, CD, CD1, C> func, CD param1, CD1 param2)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(func(Success!, param1, param2));
    }

    public async Task<Result<C>> MapAsync<C, CD>(Func<A, CD, Task<C>> func, CD param1)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(await func(Success!, param1).ConfigureAwait(false));
    }

    public async Task<Result<C>> MapAsync<C, CD, CD1>(Func<A, CD, CD1, Task<C>> func, CD param1, CD1 param2)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return Result<C>.Ok(await func(Success!, param1, param2).ConfigureAwait(false));
    }

    public Result<C> Bind<C>(Func<A, Result<C>> func)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return func(Success!);
    }

    public Result<C> Bind<C, CD>(Func<A, CD, Result<C>> func, CD param)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return func(Success!, param);
    }

    public async Task<Result<A>> BindErrorAsync(Func<Exception, Task<Result<A>>> func)
    {
        if (_state == ResultState.Succeed)
        {
            return Ok(Success!);
        }
        return await func(Error!).ConfigureAwait(false);
    }


    public Result<A> BindError(Func<Exception, Result<A>> func)
    {
        if (_state == ResultState.Succeed)
        {
            return Ok(Success!);
        }
        return func(Error!);
    }

    public async Task<Result<C>> BindAsync<C>(Func<A, Task<Result<C>>> func)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return await func(Success!).ConfigureAwait(false);
    }

    public async Task<Result<C>> BindAsync<C, CD>(Func<A, CD, Task<Result<C>>> func, CD param)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return await func(Success!, param).ConfigureAwait(false);
    }

    public async Task<Result<C>> BindAsync<C, CD, CD1>(Func<A, CD, CD1, Task<Result<C>>> func, CD param, CD1 param1)
    {
        if (_state == ResultState.Error)
        {
            return Result<C>.Failure(Error!);
        }
        return await func(Success!, param, param1).ConfigureAwait(false);
    }

    public A IfFailure(Func<Exception, A> f)
    {
        if (_state == ResultState.Error)
        {
            return f(Error!);
        }
        return Success!;
    }

    public A IfFailure(A val)
    {
        if (_state == ResultState.Error)
        {
            return val;
        }
        return Success!;
    }

    public async ValueTask IfFailureAsync(Func<Exception, ValueTask> f)
    {
        if (_state == ResultState.Error)
        {
            await f(Error!).ConfigureAwait(false);
        }
    }

    public void IfFailure(Action<Exception> f)
    {
        if (_state == ResultState.Error)
        {
            f(Error!);
        }
    }

    public void IfSucc(Action<A> f)
    {
        if (_state == ResultState.Succeed)
        {
            f(Success!);
        }
    }

    public Result<B> IfSucc<B>(B value)
    {
        return _state == ResultState.Succeed ? (Result<B>)value : Result<B>.Failure(Error!);
    }

    public Result<B> IfSucc<B>(Func<B> f)
    {
        if (_state == ResultState.Succeed)
        {
            return f();
        }
        return Result<B>.Failure(Error!);
    }

    public async Task IfSuccAsync(Func<A, Task> f)
    {
        if (_state == ResultState.Succeed)
        {
            await f(Success!).ConfigureAwait(false);
        }
    }

    public B Match<B>(Func<A, B> Succ, Func<Exception, B> Fail)
    {
        if (_state == ResultState.Succeed)
        {
            return Succ(Success!);
        }

        return Fail(Error!);
    }

    public void Match(Action<A> Succ, Action<Exception> Fail)
    {
        if (_state == ResultState.Succeed)
        {
            Succ(Success!);
        }
        else
        {
            Fail(Error!);
        }
    }

    public Task<B> MatchAsync<B>(Func<A, Task<B>> Succ, Func<Exception, Task<B>> Fail)
    {
        if (_state == ResultState.Succeed)
        {
            return Succ(Success!);
        }
        else
        {
            return Fail(Error!);
        }
    }

    public A ToSuccess()
    {
        if (IsSucceed)
        {
            return Success!;
        }
        else
        {
            throw new ValueIsNotSuccessException("value is not an success", Error);
        }
    }

    public Exception ToFailure()
    {
        if (IsSucceed)
        {
            return new ValueIsNotErrorException();
        }
        else
        {
            return Error!;
        }
    }

    public Task MatchAsync(Func<A, Task> Succ, Func<Exception, Task> Fail)
    {
        if (_state == ResultState.Succeed)
        {
            return Succ(Success!);
        }

        return Fail(Error!);
    }

    public bool Equals(Result<A> other)
    {
        return EqualityComparer<A>.Default.Equals(Success, other.Success) && Equals(Error, other.Error) && _state == other._state;
    }

    public override bool Equals(object obj)
    {
        return obj is not null && obj is Result<A> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = EqualityComparer<A>.Default.GetHashCode(Success);
            hashCode = (hashCode * 397) ^ ((Error?.GetHashCode()) ?? 0);
            hashCode = (hashCode * 397) ^ (int)_state;
            return hashCode;
        }
    }

    public static implicit operator Result<A>(A value) =>
        new Result<A>(value);
}

public static class Result
{
    public static readonly Result<Unit> UnitResult = Result<Unit>.Ok(Unit.Default);

    public static Result<TT> Select<T, TT>(this Result<T> self, Func<T, TT> selector)
    {
        if (self._state == Result<T>.ResultState.Error)
        {
            return Result<TT>.Failure(self.Error!);
        }
        return Result<TT>.Ok(selector(self.Success!));
    }

    public static async ValueTask<Result<TT>> Select<T, TT>(this Result<T> self, Func<T, ValueTask<TT>> selector)
    {
        if (self._state == Result<T>.ResultState.Error)
        {
            return Result<TT>.Failure(self.Error!);
        }
        return Result<TT>.Ok(await selector(self.Success!).ConfigureAwait(false));
    }

    public static async Task<Result<TT>> Select<T, TT>(this Result<T> self, Func<T, Task<TT>> selector)
    {
        if (self._state == Result<T>.ResultState.Error)
        {
            return Result<TT>.Failure(self.Error!);
        }
        return Result<TT>.Ok(await selector(self.Success!).ConfigureAwait(false));
    }
}
