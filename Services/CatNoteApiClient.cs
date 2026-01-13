using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CatNoteInput;

public sealed class CatNoteApiClient
{
    private readonly HttpClient _httpClient;

    public CatNoteApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<ApiResult> SendAsync(string secret, string content, CancellationToken cancellationToken)
    {
        var url = $"https://api.catnote.cn/sapi/{secret}";
        var payload = new Dictionary<string, string>
        {
            ["content"] = content
        };

        using var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return new ApiResult(response.IsSuccessStatusCode, (int)response.StatusCode, body);
    }
}

public sealed record ApiResult(bool IsSuccess, int StatusCode, string ResponseBody);
