using System;
using System.Collections.Generic;

namespace RandomQuestionGenerator;
public readonly struct Question
{

    #region Public Properties

    public int Left { get; }

    public int Right { get; }

    public QuestionOperator Operator { get; }

    public readonly string Text
    {
        get
        {
            var op = '+';
            if (Operator == QuestionOperator.Sub)
                op = '-';
            return string.Join(' ', Left, op, Right, '=');
        }
    }

    #endregion Public Properties

    #region Public Methods

    public static Question Create()
    {
        var left = _random.Next(999);
        var right = _random.Next(999);
        var @operator = _random.Next(2) == 0 ? QuestionOperator.Add : QuestionOperator.Sub;
        return new(left, right, @operator);
    }

    public readonly bool Check(int userResult) => userResult == _operatorFuncDic[Operator](Left, Right);

    #endregion Public Methods

    #region Private Fields

    private readonly static Random _random = new();
    private readonly static Dictionary<QuestionOperator, Func<int, int, int>> _operatorFuncDic = new()
    {
        [QuestionOperator.Add] = (a, b) => a + b,
        [QuestionOperator.Sub] = (a, b) => a - b,
    };

    #endregion Private Fields

    #region Private Constructors

    private Question(int left, int right, QuestionOperator @operator)
    {
        Left = left;
        Right = right;
        Operator = @operator;
    }

    #endregion Private Constructors

}
