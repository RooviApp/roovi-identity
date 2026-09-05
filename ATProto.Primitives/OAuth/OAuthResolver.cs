using System.Text.RegularExpressions;

namespace ATProto.OAuth;

public sealed partial class OAuthResolver
{
    [GeneratedRegex(@"^https?:\/\/", RegexOptions.Compiled)]
    private static partial Regex HttpsPattern();
    public async Task ResolveAsync(string input, bool noCache, CancellationToken ct = default)
    {
        //return HttpsPattern().IsMatch(input)
        //    ? await ResolveFromServiceAsync()
        //    : await ResolveFromIdentity();
    }

    private async Task ResolveFromServiceAsync()
    {

    }

    private async Task ResolveFromIdentity()
    {

    }
}
