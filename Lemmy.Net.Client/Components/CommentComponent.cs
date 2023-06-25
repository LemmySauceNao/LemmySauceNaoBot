using Lemmy.Net.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace Lemmy.Net.Client.Components;

public class CommentComponent
{
    private readonly HttpClient _http;

    public CommentComponent(HttpClient _http)
    {
        this._http = _http;
    }

    public async Task<bool> Create(CreateComment createComment)
    {
        var res = await _http.PostAsJsonAsync("/comment", createComment);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> Delete(int commentId)
    {
        try
        {
            var res = await _http.PostAsJsonAsync($"/comment/delete", new { comment_id = commentId, deleted = true });
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
        }
        return false;
    }

    public async Task<bool> Dislike(int commentId)
    {
        var res = await _http.PostAsJsonAsync("/comment/like", new { score = -1, comment_id = commentId });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> Edit(EditComment edit)
    {
        var res = await _http.PutAsJsonAsync("/comment", edit);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<Comment>> GetByCommmunity(string name)
    {
        bool receiving = true;
        List<Comment> comments = new List<Comment>();
        int x = 1;
        while (receiving)
        {
            var res = await _http.GetAsync($"/comment/list?community_name={name}&limit={500}&page={x++}&sort=New");
            var res2 = await res.Content.ReadFromJsonAsync<CommentsEnvelope>();
            foreach (var comment in res2.Comments)
                comments.Add(comment.Comment);
            if (res2.Comments.Count == 0)
                receiving = false;
        }
        return comments;
    }

    public async Task<CommentsEnvelope> GetByPost(int postid, int page = 1, int limit = 500)
    {
        var res = await _http.GetAsync($"/comment/list?post_id={postid}&limit={limit}&page={page}");
        return await res.Content.ReadFromJsonAsync<CommentsEnvelope>();
    }

    public async Task<List<Comment>> GetByUser(int userId, int page = 1, int limit = 500)
    {
        var res2 = await _http.GetAsync($"/user?person_id={userId}");
        string userEncoded = await res2.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject(userEncoded);
        var comments = ((IEnumerable<dynamic>)((dynamic)user).comments);
        List<Comment> comments2 = new List<Comment>();
        foreach (dynamic com in comments)
        {
            dynamic comment = ((JObject)com).First;
            int id = comment.Value.id;
            int post_id = comment.Value.post_id;
            comments2.Add(new Comment() { Id = id, PostId = post_id });
        }
        return comments2;
    }

    public async Task<bool> Like(int commentId)
    {
        var res = await _http.PostAsJsonAsync("/comment/like", new { score = 1, comment_id = commentId });
        return res.IsSuccessStatusCode;
    }

    public async Task<CommentsEnvelope> List(string query)
    {
        var q = string.IsNullOrWhiteSpace(query) ? string.Empty : $"?{query}";
        var res = await _http.GetAsync($"/comment/list{q}");
        try
        {
            return await res.Content.ReadFromJsonAsync<CommentsEnvelope>();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<bool> Report(int postId, string reason_for_report)
    {
        var res = await _http.PostAsJsonAsync("/post/report", new { postid = postId, reason = reason_for_report });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> Reset(int commentId)
    {
        var res = await _http.PostAsJsonAsync("/comment/like", new { score = 0, comment_id = commentId });
        return res.IsSuccessStatusCode; ;
    }
}