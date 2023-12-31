﻿using Lemmy.Net.Client.Components;


namespace Lemmy.Net.Client.Models
{
    public class LemmyService : ILemmyService
    {
        public CommunityComponent Community { get; }
        public PostComponent Post { get; }
        
        public CommentComponent Comment { get; }
        
        public UserComponent User { get; }
        public PrivateMessageComponent PrivateMessage { get; }
        public LemmyService(HttpClient client)
        {
            Community = new CommunityComponent(client);
            Post = new PostComponent(client);
            Comment = new CommentComponent(client);
            User = new UserComponent(client);
            PrivateMessage = new PrivateMessageComponent(client);
        }


        public async Task<PostEnvelope> DeletePostsAsync(int postId) =>
            await Post.Delete(postId);

        public async Task<PostEnvelope> CreatePostsAsync(CreatePost post) =>
            await Post.Create(post);

        public async Task<PostEnvelope> GetPostAsync(int postId) =>
            await Post.Get(postId);

        public async Task<PostsEnvelope> GetPostsAsync() =>
            await Post.List();

        public async Task<CommunitiesEnvelope> GetCommunitiesAsync() =>
            await Community.List(string.Empty);
    }

    public interface ILemmyService
    {
        public CommunityComponent Community { get; }
        public PostComponent Post { get; }
        
        public CommentComponent Comment { get; }
        
        public UserComponent User { get; }
        public PrivateMessageComponent PrivateMessage { get; }

        Task<PostEnvelope> DeletePostsAsync(int postId);
        Task<PostEnvelope> CreatePostsAsync(CreatePost post);
        Task<PostEnvelope> GetPostAsync(int postId);
        Task<PostsEnvelope> GetPostsAsync();
        Task<CommunitiesEnvelope> GetCommunitiesAsync();
    }
}