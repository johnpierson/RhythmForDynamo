using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythm.About
{
    /// <summary>
    /// Wrapper for about class.
    /// </summary>
    public class About
    {
        private About()
        {
        }

        /// <summary>
        /// This node is primarily to show the icon in Dynamo 2.0
        /// </summary>
        /// <returns></returns>
        public static string AboutRhythm()
        {
            string aboutRhythm = "Rhythm is a set of useful nodes to help your Revit project maintain a good rhythm with Dynamo.";

            List<string> quoteList = new List<string>();

            quoteList.Add("I've been imitated so well I've heard people copy my mistakes. - Jimi Hendrix");
            quoteList.Add(
                "\"I do the very best I know how, the very best I can, and I mean to keep on doing so until the end.\" - Abraham Lincoln");
            quoteList.Add(
                "\"I have not failed. I’ve just found 10,000 ways that won’t work.\" - Thomas A.Edison");
            quoteList.Add(
                "\"If you can dream, you can do it.\" - Walt Disney");
            quoteList.Add(
                "\"Where there is a will, there is a way. If there is a chance in a million that you can do something, anything, to keep what you want from ending, do it. Pry the door open or, if need be, wedge your foot in that door and keep it open.\" - Pauline Kael");
            quoteList.Add(
                "\"The future belongs to those who believe in the beauty of their dreams.\" - Eleanor Roosevelt");
            quoteList.Add(
                "\"You just can’t beat the person who never gives up.\" - Babe Ruth");
            quoteList.Add(
                "\"Start where you are. Use what you have. Do what you can.\" - Arthur Ashe");
            quoteList.Add(
                "\"The best revenge is massive success.\" - Frank Sinatra");
            quoteList.Add(
                "\"If you can't explain it simply, you don't understand it well enough.\" - Albert Einstein");
            quoteList.Add(
                "\"You can't connect the dots looking forward; you can only connect them looking backward. So you have to trust that the dots will somehow connect in your future. You have to trust in something--your gut, destiny, life, karma, whatever. This approach has never let me down, and it has made all the difference in my life.\" - Steve Jobs");
            quoteList.Add(
                "\"No work is ever wasted. If it’s not working, let go and move on – it’ll come back around to be useful later.\" - Pixar");

            Random rnd = new Random();
            int r = rnd.Next(quoteList.Count);

            return aboutRhythm + "\n\n" + quoteList[r];
        }
    }
}
