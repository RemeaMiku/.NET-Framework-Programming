using System;
using System.Collections.Generic;

namespace RandomQuestionGenerator;

public class ProblemService
{
    #region Public Methods

    public Question Initialize(int problemCount)
    {
        if (problemCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(problemCount));
        }
        _problems = new(problemCount);
        for (int i = 0; i < problemCount; i++)
            _problems.Add(Question.Create());
        return _problems[_index];
    }

    public Question GetNextProblem()
    {
        if (_problems is null)
        {
            throw new InvalidOperationException();
        }
        _index++;
        if (_index >= _problems.Count)
        {
            throw new InvalidOperationException();
        }
        return _problems[_index];
    }

    #endregion Public Methods

    #region Private Fields

    private List<Question>? _problems;
    private int _index = 0;

    #endregion Private Fields
}
