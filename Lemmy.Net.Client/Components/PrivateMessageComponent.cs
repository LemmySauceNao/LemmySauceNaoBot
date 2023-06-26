using Lemmy.Net.Client.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Lemmy.Net.Client.Components;

public class PrivateMessageComponent
{
    private readonly HttpClient _http;

    public PrivateMessageComponent(HttpClient _http)
    {
        this._http = _http;
    }

    public async Task<PrivateMessageRoot> Create(string recipient, string content)
    {
        var res = await _http.PostAsJsonAsync("/private_message", new { recipient = recipient, content = content });
        return await res.Content.ReadFromJsonAsync<PrivateMessageRoot>();
    }

    public async Task<PrivateMessageRoot> Delete(int privateMessageId)
    {
        var res = await _http.PostAsJsonAsync("/private_message/delete", new { private_message_id = privateMessageId, deleted = true });
        return await res.Content.ReadFromJsonAsync<PrivateMessageRoot>();
    }

    public async Task<PrivateMessageRoot> Edit(int privateMessageId, string content)
    {
        var res = await _http.PostAsJsonAsync("/private_message", new { private_message_id = privateMessageId, content = content });
        return await res.Content.ReadFromJsonAsync<PrivateMessageRoot>();
    }

    public async Task<dynamic> List(int limit = 100, int page = 1, bool unreadOnly = false, string auth = "")
    {
        string unreadonly = unreadOnly ? "true" : "false";
        string res = await _http.GetStringAsync($"/private_message/list?&auth={auth}&limit={limit}");
        return (dynamic)JsonConvert.DeserializeObject(res);
    }

    public async Task<PrivateMessageReportsEnvelope> ListReports(int limit = 10, int page = 0, bool unresolvedOnly = false)
    {
        var res = await _http.GetFromJsonAsync<PrivateMessageReportsEnvelope>($"/private_message/report/list?limit={limit}&page={0}&unresolved_only={unresolvedOnly}");
        return res;
    }

    public async Task<PrivateMessageEnvelope> MarkAsRead(int id)
    {
        var res = await _http.PostAsJsonAsync($"/private_message/mark_as_read", new { private_message_id = id, read = true });
        return await res.Content.ReadFromJsonAsync<PrivateMessageEnvelope>();
    }

    public async Task<PrivateMessageReportEnvelope> Report(int privateMessageId, string reason)
    {
        var res = await _http.PostAsJsonAsync("/private_message/report", new { private_message_id = privateMessageId, reason = reason });
        return await res.Content.ReadFromJsonAsync<PrivateMessageReportEnvelope>();
    }

    public async Task<PrivateMessageReportEnvelope> ResolveReport(int reportId)
    {
        var res = await _http.PutAsJsonAsync("/private_message/report/resolve", new { report_id = reportId, resolved = true });
        return await res.Content.ReadFromJsonAsync<PrivateMessageReportEnvelope>();
    }
}