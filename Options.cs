using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace multi_start
{
    class Options
    {
        [Option('f', "commandfile", Required = true, HelpText = "Command to be started.")]
        public string Command { get; set; }

        [Option('p', "parameters", Required = false, HelpText = "Command Parameters.")]
        public string Parameters { get; set; }
        
        [Option('c', "count", Required = true, HelpText = "Number of commands to be executed.")]
        public int Count { get; set; }

        [Option('s', "screen", DefaultValue = -1, Required = false, HelpText = "Screen to display processes.")]
        public int Screen { get; set; }

        
    }
}
