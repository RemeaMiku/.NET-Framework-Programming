using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceFileFormatter;

public partial class CSharpSourceFileFormatter
{
    #region Public Properties

    public string Text
    {
        get
        {
            if (_lines is null)
                throw new InvalidOperationException("The current formatter has not been initialized");
            var builder = new StringBuilder();
            _lines.ForEach((line) => builder.AppendLine(line));
            return builder.ToString();
        }
    }

    public ReadOnlyDictionary<string, int> WordCountDic
    {
        get
        {
            if (_wordCountDic is null)
                throw new InvalidOperationException("Word counts are not yet available");
            return _wordCountDic.AsReadOnly();
        }
    }

    public int LineCount
    {
        get
        {
            if (_lines is null)
                throw new InvalidOperationException("The current formatter has not been initialized");
            return _lines.Count;
        }
    }

    #endregion Public Properties

    #region Public Methods

    public static CSharpSourceFileFormatter FromPath(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        var formatter = new CSharpSourceFileFormatter
        {
            _lines = new()
        };
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is not null)
                formatter._lines.Add(line);
        }
        return formatter;
    }

    public int CountAllWords()
    {
        if (_lines is null)
            throw new InvalidOperationException("The current formatter has not been initialized");
        _wordCountDic = new();
        var count = 0;
        _lines.ForEach((line) =>
        {
            var words = WordRegex().Matches(line);
            count += words.Count;
            foreach (var match in words.Cast<Match>())
            {
                var word = match.Value;
                if (!_wordCountDic.ContainsKey(word))
                    _wordCountDic.Add(word, 0);
                _wordCountDic[word]++;
            }
        });
        return count;
    }

    public void RemoveEmptyLinesAndSingleLineComments()
    {
        if (_lines is null)
            throw new InvalidOperationException("The current formatter has not been initialized");
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            var line = _lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                _lines.RemoveAt(i);
                continue;
            }
            var position = line.IndexOf(@"//");
            if (position != -1)
            {
                line = line.Remove(position);
                if (string.IsNullOrWhiteSpace(line))
                    _lines.RemoveAt(i);
                else
                    _lines[i] = line;
            }
        }
    }

    #endregion Public Methods

    #region Private Fields

    static Regex _wordRegex = WordRegex();

    private List<string>? _lines;
    private Dictionary<string, int>? _wordCountDic;

    #endregion Private Fields

    #region Private Constructors

    private CSharpSourceFileFormatter() { }

    #endregion Private Constructors

    #region Private Methods

    [GeneratedRegex("\\b\\w+\\b")]
    private static partial Regex WordRegex();

    #endregion Private Methods
}
