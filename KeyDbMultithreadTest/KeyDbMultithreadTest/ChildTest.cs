using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyDbMultithreadTest
{
    public class ChildTest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public string Address { get; set; }
        public List<string> DataStrings { get; set; }
    }
}
