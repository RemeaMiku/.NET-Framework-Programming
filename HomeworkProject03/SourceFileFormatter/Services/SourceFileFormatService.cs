using System;
using System.IO;
using System.Threading.Tasks;

namespace SourceFileFormatter;

public class SourceFileFormatService
{
    #region Public Constructors

    public SourceFileFormatService() { }

    #endregion Public Constructors

    #region Public Methods

    public async Task<CSharpSourceFileFormatter> GetFormatter(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("文件路径为空");
        if (Path.GetExtension(path).ToLower() != ".cs")
            throw new ArgumentException("文件路径扩展名应为\".cs\"");
        return await Task.Run(() =>
        {
            _fileFormatter = CSharpSourceFileFormatter.FromPath(path);
            return _fileFormatter;
        });
    }

    #endregion Public Methods

    #region Private Fields

    CSharpSourceFileFormatter? _fileFormatter;

    #endregion Private Fields
}
