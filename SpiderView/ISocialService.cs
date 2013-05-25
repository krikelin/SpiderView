using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider.Media
{
    public abstract class SocialResource : Resource
    {
        new ISocialService Service { get; set; }
        public SocialResource(ISocialService service) :
            base((IOntologyService)service)
        {
        }
    }
    public class Tweet : Spider.Media.SocialResource
    {
        public Tweet(ISocialService service)
            : base(service)
        {

        }
        /// <summary>
        /// Are identical if it has the same identifier
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Identifier == (String)obj;
        }
        public String Message { get; set; }
        public Media.User User { get; set; }

    }
    /// <summary>
    /// A twitter feed, that can be listened to by the spider.
    /// </summary>
    public class Feed : Spider.Media.SocialResource 
    {
        public Feed(ISocialService service)
            : base(service)
        {
        }
    }
    public class UserFeed : Feed
    {
        public UserFeed(ISocialService service)
            : base(service)
        {
        }
    }
    /// <summary>
    /// Event args for tweet recieved.
    /// </summary>
    public class TweetReceivedEventArgs
    {

        /// <summary>
        /// Channel the new tweets belongs to
        /// </summary>
        public Feed Channel { get; set; }

        /// <summary>
        /// Tweets the 
        /// </summary>
        public Tweet[] NewTweets { get; set; }
    }
    public delegate void TweetReceivedEventHandler(object sender, TweetReceivedEventArgs e);
    public interface ISocialService
    {
        TweetReceivedEventHandler RecievedTweet { get; set; }
        void ListenToFeed(Feed feed);
        void StopListenToFeed(Feed feed);

        /// <summary>
        /// Notify the dispatcher about activity in a certain channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
    }
}
