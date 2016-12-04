using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Emitter;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;

namespace Server
{
    public class Program
    {
        private const string Key = "PXZDPR3qZY7XsZ_AYX2sAACo2sBwS5Ip";

        public static void Main(string[] args)
        {
            // Connect to emitter
            var emitter = Connection.Establish();

            // Set up your credentials (https://apps.twitter.com)
            //Auth.SetUserCredentials("CONSUMER_KEY", "CONSUMER_SECRET", "ACCESS_TOKEN", "ACCESS_TOKEN_SECRET");
            Auth.SetUserCredentials(
                Environment.GetEnvironmentVariable("CONSUMER_KEY"),
                Environment.GetEnvironmentVariable("CONSUMER_SECRET"),
                Environment.GetEnvironmentVariable("ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("ACCESS_TOKEN_SECRET")
                );

            // The data we are going to measure
            var original = 0;
            var retweets = 0;
            var total = 0;
            var history = new Queue<int>();

            // Using the sample stream
            var stream = Stream.CreateSampleStream();
            stream.AddTweetLanguageFilter(LanguageFilter.English);
            stream.FilterLevel = Tweetinvi.Streaming.Parameters.StreamFilterLevel.Low;
            stream.TweetReceived += (sender, t) =>
            {
                // Increment the number of tweets
                Interlocked.Increment(ref total);

                // If it's a retweet, count a retweet
                if (t.Tweet.IsRetweet)
                    Interlocked.Increment(ref retweets);

                // If it's an original tweet, count it
                if (!t.Tweet.IsRetweet)
                    Interlocked.Increment(ref original);
            };

            // Start a periodic task, which fires every 500 milliseconds. This will be used
            // to send the updates to the client.
            PeriodicTask.Start(() =>
            {
                // Publish the data to the broker
                emitter.Publish(
                    Key,
                    "dashboard-updates/counts",
                    JsonConvert.SerializeObject(new
                    {
                        retweets = retweets,
                        original = original
                    }));

                retweets = 0;
                original = 0;
            }, intervalInMilliseconds: 5000);

            // Start a periodic task, which fires every 500 milliseconds. This will be used
            // to send the updates to the client.
            PeriodicTask.Start(() =>
            {
                history.Enqueue(total);
                if (history.Count > 6)
                    history.Dequeue();

                // Publish the data to the broker
                emitter.Publish(
                   Key,
                   "dashboard-updates/history",
                   JsonConvert.SerializeObject(history.ToArray())
                   );

                total = 0;
            }, intervalInMilliseconds: 1000);

            // Start
            stream.StartStream();
        }
    }
}