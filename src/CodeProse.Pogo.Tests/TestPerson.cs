using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Newtonsoft.Json;

namespace CodeProse.Pogo.Tests
{
    public class TestPerson
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public DateTime? HireDate { get; set; }

        public bool? IsActive { get; set; }

        public double? HourlyRate { get; set; }

        public Nullable<Int32> DepartmentCode { get; set; }

        public long Face { get; set; }

        public string[] Skills { get; set; }

        public IList<string> PhoneNumbers { get; set; }

        public ICollection<int> PerformanceRatings { get; set; }

        public byte[] ProfileImage { get; set; }
    }
}
