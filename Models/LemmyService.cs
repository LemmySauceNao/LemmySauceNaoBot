using Lemmy.Net.Client;
using Lemmy.Net.Client.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LemmySauceNao.Models
{
    public static class LemmyService
    {
        private static ILemmyService _lemmyClient;

        public static void CommentOnPost(int id, string text)
        {
            CreateComment comment = new CreateComment() { content = text, post_id = id };
            var task = _lemmyClient.Comment.Create(comment);
            task.Wait();
        }

        public static void DeleteComment(int id)
        {
            var deltask = _lemmyClient.Comment.Delete(id);
            deltask.Wait();
        }

        public static List<Post> GetAllPostsForCommunityName(string community)
        {
            var postsTask = _lemmyClient.Post.GetPostsForCommunity(community);
            postsTask.Wait();
            var posts = postsTask.Result;
            if (posts == null)
                return new List<Post>();
            List<Post> postsList = new List<Post>();
            foreach (var post in posts.Posts)
            {
                postsList.Add(post.Post);
            }
            return postsList;
        }

        public static List<Comment> GetCommentsByPost(int postid)
        {
            var task = _lemmyClient.Comment.GetByPost(postid);
            task.Wait();
            var envelope = task.Result;
            List<Comment> comments = new List<Comment>();
            foreach (var comment in envelope.Comments)
            {
                comments.Add(comment.Comment);
            }
            return comments;
        }

        public static List<Comment> GetCommentsBySub(string communityName)
        {
            var task = _lemmyClient.Comment.GetByCommmunity(communityName);
            List<Comment> comments = new List<Comment>();
            task.Wait();
            return task.Result;
        }

        public static Community GetCommunityByName(string communityName)
        {
            var comTask = _lemmyClient.Community.List(communityName);
            comTask.Wait();
            var com = comTask.Result;
            return com.Communities[0].Community;
        }

        public static void Init()
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            var services = new ServiceCollection();
            services.AddLemmyClient(
                "https://lemmynsfw.com/",
                File.ReadAllText($"{strWorkPath}/LemmyUsername.txt"),
                File.ReadAllText($"{strWorkPath}/LemmyPassword.txt"),
    async username =>
    {
        return File.Exists($"{strWorkPath}/{username}.txt") ? File.ReadAllText($"{strWorkPath}/{username}.txt") : "";
    },
    (username, jwtToken) =>
    {
        File.WriteAllText($"{strWorkPath}/{username}.txt", jwtToken); CustomAuthenticationHandler.auth = jwtToken;
    }
            );
            var provider = services.BuildServiceProvider();
            _lemmyClient = provider.GetRequiredService<ILemmyService>();
        }

        public static async void Post(string url, string title, int communityToPostTo)
        {
            CreatePost post = new CreatePost() { Nsfw = true, Url = url, Name = title, Body = "", CommunityId = communityToPostTo };
            await _lemmyClient.CreatePostsAsync(post);
        }

        internal static List<Comment> GetCommentsByUser(int sauceNaoBotId)
        {
            var task = _lemmyClient.Comment.GetByUser(sauceNaoBotId);
            task.Wait();
            return task.Result;
        }

        internal static List<Community> GetCommunitiesModdedBy(int sauceNaoBotId)
        {
            var task = _lemmyClient.Community.GetByMod(sauceNaoBotId);
            task.Wait();
            return task.Result;
        }

        internal static List<PrivateMessage> GetMessages()
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            string data = File.Exists($"{strWorkPath}/{File.ReadAllText($"{strWorkPath}/LemmyUsername.txt")}.txt") ? File.ReadAllText($"{strWorkPath}/{File.ReadAllText($"{strWorkPath}/LemmyUsername.txt")}.txt") : "";
            var task = _lemmyClient.PrivateMessage.List(50, 1, false, data);
            task.Wait();
            var result = task.Result;
            List<PrivateMessage> messages = new List<PrivateMessage>();
            var messages2 = (IEnumerable<dynamic>)(result.private_messages);
            foreach (var message in messages2)
            {
                dynamic mes = (dynamic)message.private_message;
                string content = (string)mes.content;
                messages.Add(new PrivateMessage() { content = mes.content });
            }
            return messages;
        }

        internal static List<Post> GetNewest50PostsByCommunityName(string name)
        {
            var task = _lemmyClient.Post.GetPostsForCommunity(name);
            task.Wait();
            var posts = task.Result;
            List<Post> newest = new List<Post>();
            foreach (var post in posts.Posts)
                newest.Add(post.Post);
            return newest;
        }
    }
}