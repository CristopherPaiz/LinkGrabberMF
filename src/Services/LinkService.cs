using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Runs link work in parallel, leaving each item's outcome in its LinkState.
    public static class LinkService
    {
        public static async Task ForEachParallel<T>(IEnumerable<T> source, Func<T, Task> work)
        {
            using (var gate = new SemaphoreSlim(Limits.MaxParallelRequests))
            {
                await Task.WhenAll(source.Select(async entry =>
                {
                    await gate.WaitAsync();
                    try { await work(entry); }
                    finally { gate.Release(); }
                }));
            }
        }

        public static async Task ResolveAll(List<FileItem> targets, Action<int, int> progress)
        {
            int done = 0;
            await ForEachParallel(targets, async item =>
            {
                await Resolve(item);
                done++;
                progress(done, targets.Count);
            });
        }

        public static async Task Resolve(FileItem item)
        {
            try
            {
                item.DirectUrl = await Task.Run(() => LinkResolver.Resolve(item.PageUrl));
                item.LinkState = LinkState.Ready;
            }
            catch (ChallengeException)
            {
                item.LinkState = LinkState.Challenge;
            }
            catch (Exception)
            {
                item.LinkState = LinkState.Error;
            }
        }

        public static Task VerifyAll(List<FileItem> targets)
        {
            return ForEachParallel(targets, async item =>
            {
                try
                {
                    ProbeResult result = await Task.Run(() => Http.Probe(item.DirectUrl));
                    item.LinkState = result == ProbeResult.FileAlive ? LinkState.Ready
                        : result == ProbeResult.Challenge ? LinkState.Challenge : LinkState.Expired;
                }
                catch (WebException)
                {
                    // Network failure says nothing about the link, so its state stays as it was.
                }
            });
        }
    }
}
