using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Chomsky.PageObjects.PageObjectGenerator;
using System.IO;
namespace Chomsky
{
    class Program
    {
        /// <summary>
        /// arg[0] : directory where .chom files are located
        /// arg[1] : output directory
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            ProgramOptions options = new ProgramOptions(args);
            foreach(string file in Directory.GetFileSystemEntries(options.InputDirectory))
            {
                string fileAsString = File.ReadAllText(file);
                Console.WriteLine(fileAsString);
                compiler.Compile(fileAsString, new System.IO.DirectoryInfo(options.OutputDirectory));
            }
            
        
        }
    }

    public class ProgramOptions
    {
        public string InputDirectory {get; set;}
        public string OutputDirectory {get; set;}
        public ProgramOptions(string[] args)
        {
            if (args.Length == 1 && args[0].ToLower().IndexOf(@".json") > -1)
            {
                UseOptionsFromConfig(args[0]);
            }
            List<string> argsList = new List<string>();
            argsList.AddRange(args);
            for (int i = 0; i < args.Length; i++)
            {
                if(args[i].Equals("-i"))
                {
                    i++;
                    InputDirectory = args[i];
                }
                if(args[i].Equals("-o"))
                {
                    i++;
                    OutputDirectory = args[i];
                }
            }           
        }

        public void UseOptionsFromConfig(string configFile)
        {
            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), configFile);
            Console.WriteLine(path);
            JObject json = JObject.Parse(File.ReadAllText(path));
            this.InputDirectory = json["InputDirectory"].Value<string>();
            this.OutputDirectory = json["OutputDirectory"].Value<string>();
        }
    }
}
