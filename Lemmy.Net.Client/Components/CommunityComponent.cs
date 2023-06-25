using Lemmy.Net.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace Lemmy.Net.Client.Components;

public class CommunityComponent
{
    private readonly HttpClient _http;

    public CommunityComponent(HttpClient _http)
    {
        this._http = _http;
    }

    public async Task<PersonRoot> BanUser(BanUser ban)
    {
        var res = await _http.PostAsJsonAsync("/community/ban", ban);
        return await res.Content.ReadFromJsonAsync<PersonRoot>();
    }

    public async Task<BlockCommunity> Block(int communityId)
    {
        var res = await _http.PostAsJsonAsync("/community/block", new { block = true, community_id = communityId });
        return await res.Content.ReadFromJsonAsync<BlockCommunity>();
    }

    public async Task<CommunityEnvelope> Create(CreateCommunity create)
    {
        var res = await _http.PostAsJsonAsync("/community", create);
        return await res.Content.ReadFromJsonAsync<CommunityEnvelope>();
    }

    public async Task<CommunityModEnvelope> CreateMod(AddModToCommunity addMod)
    {
        var res = await _http.PostAsJsonAsync("/community/mod", addMod);
        return await res.Content.ReadFromJsonAsync<CommunityModEnvelope>();
    }

    public async Task<bool> Delete(int communityId)
    {
        var res = await _http.PostAsJsonAsync("/comment", new { community_id = communityId, delete = true });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> Edit(EditCommunity edit)
    {
        var res = await _http.PutAsJsonAsync("/community", edit);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<Community>> GetByMod(int sauceNaoBotId)
    {
        var res2 = await _http.GetAsync($"/user?person_id={sauceNaoBotId}");
        string userEncoded = await res2.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject(userEncoded);
        var moderates = ((IEnumerable<dynamic>)((dynamic)user).moderates);
        List<Community> communities = new List<Community>();
        foreach (dynamic com in moderates)
        {
            dynamic community = ((JObject)com).First;
            int id = community.Value.id;
            string name = community.Value.name;
            if (name != "SauceNaoBot")
            {
                communities.Add(new Community() { Id = id, Name = name });
            }
        }
        return communities;
    }

    public async Task<CommunitiesEnvelope> List(string? query = null)
    {
        var q = string.IsNullOrWhiteSpace(query) ? string.Empty : $"?{query}";
        var res = await _http.GetAsync($"/community/list{q}");
        return await res.Content.ReadFromJsonAsync<CommunitiesEnvelope>();
    }

    public async Task<BlockCommunity> UnBlock(int communityId)
    {
        var res = await _http.PostAsJsonAsync("/community/block", new { block = false, community_id = communityId });
        return await res.Content.ReadFromJsonAsync<BlockCommunity>();
    }
}