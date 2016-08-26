using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace RestAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            string details = GethackerNewsTopStories();

            // Print the raw result from the webRequest
            Console.WriteLine(details);

            // Call the api (compare results : https://news.ycombinator.com/news) and deserialise the result
            int[] topStoriesIds = JsonConvert.DeserializeObject<int[]>(details);
            GethackerNewsItem(topStoriesIds);

            // Call a method to print "Job, Show and Ask" Items
            JobAskShow.PrintAll();

            Console.ReadLine();
        }

        // Get IDs of top stories
        public static string GethackerNewsTopStories()
        {
            string url = "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty";
            return CallRestMethod(url);
        }

        public static string CallRestMethod(string url)
        {
            //https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest%28v=vs.110%29.aspx
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);

            // Request method to use to contact the Internet resource. The default value is GET
            webrequest.Method = "GET";

            // Set the content type of the data being posted.
            webrequest.ContentType = "application/x-www-form-urlencoded";

            /* https://msdn.microsoft.com/en-us/library/system.net.httpwebresponse%28v=vs.110%29.aspx
             * https://msdn.microsoft.com/en-gb/library/system.net.webrequest.getresponse%28v=vs.110%29.aspx
             * This is a synchronous call to the API
             * Check this link out for asynchronous call : 
             * https://msdn.microsoft.com/en-gb/library/system.net.webrequest.begingetresponse%28v=vs.110%29.aspx
            */

            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

            // Specify which encoding format to be used
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");

            string result = string.Empty;

            // Get the stream that is used to read the body of the response from the server
            using (StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc))
            {
                // Do one Read operation, release the resources of the Stream
                result = responseStream.ReadToEnd();
            }

            // Close the stream and release the connection for reuse
            webresponse.Close();
            return result;
        }
        
        public static void GethackerNewsItem(int[] topStoriesIds)
        {
            //Take() Method is a linq extension method that allows to query data on collections in a similar way to SQL data queries
            foreach (var story in topStoriesIds.Take(1))
            {
                string url = string.Format("https://hacker-news.firebaseio.com/v0/item/{0}.json?print=pretty", story);
                Console.WriteLine(CallRestMethod(url));

                // Deserialise custom object
                BuildObjectFromJsonData(CallRestMethod(url));
            }
        }

        // Deserialisation
        public static object BuildObjectFromJsonData(string jsonData)
        {
            HackerNewsItem item = JsonConvert.DeserializeObject<HackerNewsItem>(jsonData);
            Console.WriteLine(item);

            // Get comments
            item.GetComments();
            return item;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Convert a Unix timestamp to a DateTime object
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
    
    public class HackerNewsItem
    {
        // Object attributes of an Item
        public string by { get; set; }
        public int descendants { get; set; }
        public int id { get; set; }
        public int[] kids { get; set; }
        public decimal score { get; set; }
        public double time { get { return this.time; } set { this.datePosted = Program.UnixTimeStampToDateTime(value); } }
        public DateTime datePosted { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }

        // Get associated comments 
        public void GetComments()
        {
            // Print the 5 first comments
            foreach (var kid in this.kids.Take(5))
            {
                string url = string.Format("https://hacker-news.firebaseio.com/v0/item/{0}.json?print=pretty", kid);
                // Uncomment below to print raw results of the comment
                //Console.WriteLine(Program.CallRestMethod(url));
                HackerNewsComment.DeserialiseComments(Program.CallRestMethod(url));
            }
        }

        // Build the override of the toString method to print the object
        public override string ToString()
        {
            return string.Format("[{9}HackerNewsItem: {9} by= {0},{9} descendants= {1},{9} id= {2},{9} kids= there are {3} comments,{9} score= {4},{9} datePosted= {5},{9} title= {6},{9} type= {7},{9} url= {8} {9}]", by, descendants, id, kids.Length.ToString(), score, datePosted, title, type, url, Environment.NewLine);
        }
    }
    
    public class HackerNewsComment
    {
        // Object attributes of a Comment
        public string by { get; set; }
        public int id { get; set; }
        public int parent { get; set; }
        public string text { get; set; }
        public double time { get { return this.time; } set { this.datePosted = Program.UnixTimeStampToDateTime(value); } }
        public DateTime datePosted { get; set; }
        public string type { get; set; }

        // Deserialise the comments
        public static object DeserialiseComments(string comments)
        {
            HackerNewsComment comment = JsonConvert.DeserializeObject<HackerNewsComment>(comments);
            Console.WriteLine(comment);
            return comment;
        }

        // Build the override of the toString method to print the object
        public override string ToString()
        {
            return string.Format("[{6}HackerNewsComment: {6} by= {0},{6} id= {1},{6} parent= {2},{6} text= {3},{6} datePosted= {4},{6} type= {5}{6}]", by, id, parent, text, datePosted, type, Environment.NewLine);
        }
    }

    class JobAskShow
    {
        public static void PrintAll()
        {
            // Print IDs of Job stories
            string job = GethackerNewsJobs();
            Console.WriteLine("Jobs:\n");
            Console.WriteLine(job);
            Console.WriteLine("First:\n");
            // Print first item of job stories
            int[] jobIds = JsonConvert.DeserializeObject<int[]>(job);
            GethackerNewsJobShowAsk(jobIds);

            // Print IDs of Ask stories
            string ask = GethackerNewsAsk();
            Console.WriteLine("Ask stories:\n");
            Console.WriteLine(ask);
            Console.WriteLine("First:\n");
            // Print first item of ask stories
            int[] askIds = JsonConvert.DeserializeObject<int[]>(ask);
            GethackerNewsJobShowAsk(askIds);

            // Print IDs of Show stories
            string show = GethackerNewsShow();
            Console.WriteLine("Show stories:\n");
            Console.WriteLine(show);
            Console.WriteLine("First:\n");
            // Print first item of show stories
            int[] showIds = JsonConvert.DeserializeObject<int[]>(show);
            GethackerNewsJobShowAsk(showIds);
        }

        // Get IDs of Job stories
        public static string GethackerNewsJobs()
        {
            string job = "https://hacker-news.firebaseio.com/v0/jobstories.json?print=pretty";
            return Program.CallRestMethod(job);
        }

        // Get IDs of Ask stories
        public static string GethackerNewsAsk()
        {
            string ask = "https://hacker-news.firebaseio.com/v0/askstories.json?print=pretty";
            return Program.CallRestMethod(ask);
        }

        // Get IDs of Show stories
        public static string GethackerNewsShow()
        {
            string show = "https://hacker-news.firebaseio.com/v0/showstories.json?print=pretty";
            return Program.CallRestMethod(show);
        }

        public static void GethackerNewsJobShowAsk(int[] topStoriesIds)
        {
            //Take() Method is a linq extension method that allows to query data on collections in a similar way to SQL data queries
            foreach (var story in topStoriesIds.Take(1))
            {
                string url = string.Format("https://hacker-news.firebaseio.com/v0/item/{0}.json?print=pretty", story);
                // Print the raw result from the webRequest
                Console.WriteLine(Program.CallRestMethod(url));
            }
        }
    }
}
