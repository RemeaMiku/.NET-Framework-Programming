using System;
using System.Collections.Generic;

namespace ProblemMaker;

public enum ProblemOperator
{
    Add,
    Sub
}
public class Problem
{
    #region Public Properties

    public int Left { get; }

    public int Right { get; }

    public ProblemOperator Operator { get; }

    public string Text
    {
        get
        {
            var op = '?';
            switch (Operator)
            {
                case ProblemOperator.Add:
                    op = '+';
                    break;
                case ProblemOperator.Sub:
                    op = '-';
                    break;
            }
            return $"{Left}{op}{Right} = ";
        }
    }

    #endregion Public Properties

    #region Public Methods

    public static Problem CreateProblem()
    {
        var left = _random.Next(999);
        var right = _random.Next(999);
        var @operator = _random.Next(2) == 0 ? ProblemOperator.Add : ProblemOperator.Sub;
        return new(left, right, @operator);
    }

    public bool CheckAnswer(int userResult) => userResult == _operatorFuncDic[Operator](Left, Right);

    #endregion Public Methods

    #region Private Fields

    private readonly static Random _random = new();
    private readonly static Dictionary<ProblemOperator, Func<int, int, int>> _operatorFuncDic = new()
    {
        [ProblemOperator.Add] = (a, b) => a + b,
        [ProblemOperator.Sub] = (a, b) => a - b,
    };

    #endregion Private Fields

    #region Private Constructors

    private Problem(int left, int right, ProblemOperator @operator)
    {
        Left = left;
        Right = right;
        Operator = @operator;
    }

    #endregion Private Constructors
}
