using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    public static class Tasks
    {
        /// <summary>
        /// Returns the content of required uri's.
        /// Method has to use the synchronous way and can be used to compare the
        ///  performace of sync/async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris)
        {
            WebClient wc = new WebClient();
            return uris.Select(item => wc.DownloadString(item)).ToList();
            
        }

        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace 
        /// of sync \ async approaches. 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams 
        /// that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {           
                List<string> result = new List<string>();
                int length = uris.Count();
                Func<Uri, string>[] arrOfDownloads = new Func<Uri, string>[Math.Min(maxConcurrentStreams, length)];
                for (int i = 0; i < arrOfDownloads.Length; i++)
                    arrOfDownloads[i] = new WebClient().DownloadString;
                int k;
                IAsyncResult[] results = new IAsyncResult[arrOfDownloads.Length];
                for (int i = 0; i < length; i++)
                {
                    if (i < maxConcurrentStreams)
                        results[i] = arrOfDownloads[i].BeginInvoke(uris.ElementAt(i), null, null);
                    else
                    {
                        k = i%maxConcurrentStreams;
                        if (results[k].IsCompleted)
                        {
                            result.Add(arrOfDownloads[k].EndInvoke(results[k]));
                            results[k] = arrOfDownloads[k].BeginInvoke(uris.ElementAt(i), null, null);
                        }
                        else
                            i--;
                    }
                }
                return result;      
        }

        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            Stream stream;            
            if (resource.Scheme == "ftp")
            {
                FtpWebRequest request = (FtpWebRequest) WebRequest.Create(resource);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                stream = response.GetResponseStream();
            }
            else
            {
                stream = await new HttpClient().GetStreamAsync(resource);
            }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] res = md5.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < res.Length; i++)
            {
                sb.Append(res[i].ToString("X"));
            }
            return sb.ToString();
        }
    }
}
