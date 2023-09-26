using System.Diagnostics;
using System.Text.RegularExpressions;

string input = "    public partial class MainWindow : Window";
string pattern = @"\b\w+\b";

var regex = new Regex(pattern);
MatchCollection matches = regex.Matches(input);
foreach (Match match in matches.Cast<Match>())
{
    Console.WriteLine(match.Value);
}