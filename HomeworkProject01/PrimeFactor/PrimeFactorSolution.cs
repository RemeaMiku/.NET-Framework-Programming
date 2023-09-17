using static System.Math;
namespace PrimeFactor;

internal class PrimeFactorSolution
{
    #region Public Methods

    public static bool IsPrimeNumber(long num)
    {
        if (num < 2)
            return false;
        var res = true;
        if (num < 1E12)
        {
            for (var i = 2L; i * i <= num; i++)
                if (num % i == 0)
                {
                    res = false;
                    break;
                }
        }
        else
        {
            Parallel.For(2L, (long)Sqrt(num) + 1, (i, state) =>
            {
                if (num % i == 0)
                {
                    res = false;
                    state.Stop();
                }
            });
        }
        return res;
    }
    public static IEnumerable<long> FindPrimeFactors(long num)
    {
        for (var i = 2L; i * i <= num; i++)
        {
            if (num % i == 0)
            {
                if (IsPrimeNumber(i))
                    yield return i;
                if (num / i != i && IsPrimeNumber(num / i))
                    yield return num / i;
            }
        }
    }

    #endregion Public Methods

}
