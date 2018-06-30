using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGPatcher
{
    public class CrapLogger
    {
        public string Name { get; private set; }
        
        public CrapLogger(string name)
        {
            Name = name;
        }
        private void SetTitle(string format, object[] args)
        {
            Console.Title = $"TABGPatcher - {string.Format(format, args)}";
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine("[{0}] >>> {1} <<<", Name, string.Format(format, args));
            SetTitle(format, args);
        }
        public void Log(string format, params object[] args)
        {
            Console.WriteLine("[{0}] #> {1}", Name, string.Format(format, args));
            SetTitle(format, args);
        }
        public void Info(string format, params object[] args)
        {
            Console.WriteLine("[{0}] !> {1}", Name, string.Format(format, args));
            SetTitle(format, args);
        }
    }
}
