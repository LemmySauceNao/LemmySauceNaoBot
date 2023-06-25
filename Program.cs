// The Lemmy.NET version we are using is custom as the current Lemmy.NET API is a very work in progress piece ( although its awsome and im super greatfull for it)
// but this means it is missing a decent amount of functionality we need. Since its in very active development i dont want to step on their  toes and im just going to use
// a forked version for right now
using Lemmy.Net.Client.Models;
using SauceNET;

internal class SauceNao
{
    private HashSet<string> alreadyTaggedIds;
    private string SauceNaoAPIKey = "";
    private int sauceNaoBotId = 182352;
    private string strWorkPath;

    public static void Main(String[] args)
    {
        SauceNao s = new SauceNao();
        s.Init();
        if (args.Length == 0)
            args = new string[1] { "AllTime" };
        if (args[0] == "Upkeep")
        {
            s.Upkeep();
        }
        else if (args[0] == "AllTime")
        {
            s.RunForAllTime();
            s.Upkeep();
        }
    }

    private void IdentifyAndTagPost(Post post)
    {
        //Get the sauce and comment it
        try
        {
            string image = post.Url;
            if (image == null)
            {
                // no post url, maybe its in the body?
                string bodyContent = post.Body;
                if (bodyContent.Contains("http"))
                {// pretty sure theres a image in there lets grab the first one.
                    int startindex = bodyContent.IndexOf("http");
                    string bodycontent2 = bodyContent.Substring(startindex);
                    string hopefullLink = bodycontent2.Substring(0, bodycontent2.IndexOf(')'));
                    image = hopefullLink;
                }
            }
            string message = "";
            try
            {
                message = SauceNaoImage(image);
                if (message.Contains("hard time finding your image") && image != null)
                {
                    Console.WriteLine("Unsupported File Type like a video:" + image);
                    alreadyTaggedIds.Add(post.Id.ToString());
                    File.WriteAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds"), alreadyTaggedIds);
                    message = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception hit trying to read post ? unsure why");
                alreadyTaggedIds.Add(post.Id.ToString());
                File.WriteAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds"), alreadyTaggedIds);
            }
            if (message != "")
            {
                LemmySauceNao.Models.LemmyService.CommentOnPost(post.Id, message);
                alreadyTaggedIds.Add(post.Id.ToString());
                File.WriteAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds"), alreadyTaggedIds);
            }
        }
        catch (Exception ex)
        {
        }
    }

    private void Init()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        SauceNaoAPIKey = File.ReadAllText(Path.Combine(strWorkPath, "SauceNaoAPIKey.txt"));
        LemmySauceNao.Models.LemmyService.Init();
        //ResyncTaggedFile();
    }

    private void ResyncTaggedFile()
    {
        // this sucks. i cant find any way to get comments by user... so we are just getting all comments on subs it moderates, then sorting it by user here... only call if the tagged file gets out of sync somehow.
        alreadyTaggedIds = new HashSet<string>();
        List<Community> communities = LemmySauceNao.Models.LemmyService.GetCommunitiesModdedBy(sauceNaoBotId);
        foreach (var community in communities)
        {
            var comments = LemmySauceNao.Models.LemmyService.GetCommentsBySub(community.Name);
            foreach (var comment in comments)
            {
                if (comment.CreatorId == sauceNaoBotId && !comment.Deleted)
                    alreadyTaggedIds.Add(comment.PostId.ToString());
            }
        }
        File.WriteAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds"), alreadyTaggedIds);
    }

    private void RunForAllTime()
    {
        alreadyTaggedIds = new HashSet<string>(File.ReadAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds")));
        List<Community> communities = LemmySauceNao.Models.LemmyService.GetCommunitiesModdedBy(sauceNaoBotId);
        var messages = LemmySauceNao.Models.LemmyService.GetMessages();
        List<string> removedAlready = new List<string>();
        foreach (var message in messages)
        {
            try
            {
                int indexOfSeperator = message.content.IndexOf(":");
                string sub = message.content.Substring(0, indexOfSeperator);
                bool enable = message.content.Substring(indexOfSeperator + 1).ToLower() == "enable";
                bool disable = message.content.Substring(indexOfSeperator + 1).ToLower() == "disable";

                if (enable)
                {
                    bool found = false;
                    foreach (var comunity in communities)
                    {
                        if (comunity.Name.ToLower() == sub.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found && !removedAlready.Contains(sub))
                        communities.Add(new Community() { Name = sub });
                    else if (!found)
                        removedAlready.Remove(sub);// if we found it after removing it, remove it from the removed already list incase we get another enable.
                }
                else if (disable)
                {
                    removedAlready.Add(sub);
                }
            }
            catch (Exception ex)
            {
            }
        }
        foreach (var community in communities)
        {
            var posts = LemmySauceNao.Models.LemmyService.GetAllPostsForCommunityName(community.Name);
            foreach (var post in posts)
            {
                if (!alreadyTaggedIds.Contains(post.Id.ToString()))
                {
                    Console.WriteLine($"Checking a post in {community.Name} on post {post.Name}");
                    IdentifyAndTagPost(post);
                }
            }
        }
    }

    private string SauceNaoImage(string image)
    {
        var client = new SauceNETClient(SauceNaoAPIKey);
        Task<SauceNET.Model.Sauce> sauceTask = client.GetSauceAsync(image);
        sauceTask.Wait();
        SauceNET.Model.Sauce sauce = sauceTask.Result;
        string message = "";
        if (sauce.Message.Contains("Too many failed search"))
        {
            Console.WriteLine("Too many failed searches, waiting a while");
            Thread.Sleep(30000);
        }
        else if (sauce.Message.Contains("Specified file does not seem to be an image"))
        {
            Console.WriteLine("Specified File was not an image (hit gallery?)");
            message = "Im sorry, this post doesnt seem to be an image (It may be something like a reddit gallery?) I can not currently check these :/";
        }
        else if (sauce.Message.Contains("Search Rate Too High"))
        {
            Console.WriteLine("Searching too fast, waiting the 30 seconds");
            Thread.Sleep(30000);
        }
        else if (sauce.Message.Contains("Problem with remote server"))
        {
            message = "Im sorry, but i cant seem to access that link? due to a problem on the remote server?";
        }
        else if (sauce.Message.Contains("You need an Image"))
        {
            message = "Im sorry ive let ya down, Im having a hard time finding your image. Please send this post to u/paddedperson if you think this is an error so he can fix his code!";
        }
        else if (sauce.Message.Contains("Specified file no longer exists on the remote server"))
        {
            message = "Hey i got a 404 trying to access that image, are you sure it still exists?";
        }
        else if (sauce.Message != "")
        {
            Console.WriteLine("Some unknown error has occured with saucenao:" + sauce.Message);
        }
        else if (sauce.Results.Count > 0)
        {
            List<Tuple<string, string>> properties = new List<Tuple<string, string>>();
            properties.Add(new Tuple<string, string>("SauceNao SourceURL", sauce.Results[0].SourceURL));
            foreach (var prop in sauce.Results[0].Properties)
            {
                properties.Add(new Tuple<string, string>(prop.Name, prop.Value));
            }
            properties.Add(new Tuple<string, string>("Similarity", sauce.Results[0].Similarity));

            if (double.Parse(sauce.Results[0].Similarity) > 80)
            {
                Console.WriteLine("Found a solid match!  \r\n \r\n");
                message = $"Im pretty sure I found the sauce! \r\n \r\n";
                foreach (var prop in properties)
                {
                    message += $"- {prop.Item1}:{prop.Item2}\r\n";
                }
            }
            else if (double.Parse(sauce.Results[0].Similarity) > 60)
            {
                Console.WriteLine("Found a  match!  \r\n \r\n");
                message = $"I think I found the sauce!  \r\n \r\n";
                foreach (var prop in properties)
                {
                    message += $"- {prop.Item1}:{prop.Item2}\r\n";
                }
            }
            else
            {
                Console.WriteLine("Found a possible match! \r\n \r\n");
                message = $"I might have found the sauce, but im unsure.  \r\n \r\n";
                foreach (var prop in properties)
                {
                    message += $"- {prop.Item1}:{prop.Item2}\r\n";
                }
            }
        }
        else
        {
            Console.WriteLine("didnt find a match :/");
            message = "Im sorry, I could not find a sauce for this post using SauceNao. I'll try to do better next time ;-;";
        }
        return message;
    }

    private void Upkeep()
    {
        Console.WriteLine("Moving Into Upkeep Mode");
        while (true)
        {
            Console.WriteLine("Beginging a scan");
            alreadyTaggedIds = new HashSet<string>(File.ReadAllLines(Path.Combine(strWorkPath, "AlreadyTaggedIds")));
            List<Community> communities = LemmySauceNao.Models.LemmyService.GetCommunitiesModdedBy(sauceNaoBotId); var messages = LemmySauceNao.Models.LemmyService.GetMessages();
            List<string> removedAlready = new List<string>();
            foreach (var message in messages)
            {
                try
                {
                    int indexOfSeperator = message.content.IndexOf(":");
                    string sub = message.content.Substring(0, indexOfSeperator).Trim();
                    bool enable = message.content.Substring(indexOfSeperator + 1).Trim().ToLower() == "enable";
                    bool disable = message.content.Substring(indexOfSeperator + 1).Trim().ToLower() == "disable";

                    if (enable)
                    {
                        bool found = false;
                        foreach (var comunity in communities)
                        {
                            if (comunity.Name.ToLower() == sub.ToLower())
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found && !removedAlready.Contains(sub))
                            communities.Add(new Community() { Name = sub });
                        else if (!found)
                            removedAlready.Remove(sub);// if we found it after removing it, remove it from the removed already list incase we get another enable.
                    }
                    else if (disable)
                    {
                        removedAlready.Add(sub);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            foreach (var community in communities)
            {
                var posts = LemmySauceNao.Models.LemmyService.GetNewest50PostsByCommunityName(community.Name);
                foreach (var post in posts)
                {
                    if (!alreadyTaggedIds.Contains(post.Id.ToString()))
                    {
                        Console.WriteLine($"Checking a post in {community.Name} on post {post.Name}");
                        IdentifyAndTagPost(post);
                    }
                }
                Thread.Sleep(10000);
            }
        }
    }
}