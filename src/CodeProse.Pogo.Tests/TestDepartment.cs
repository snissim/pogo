using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CodeProse.Pogo.Tests
{
    public class TestDepartment
    {
        public string Id { get; set; }

        [JsonIgnore]
        public int Code { get; set; }

        [JsonProperty("departmentName")]
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public TestPerson Director { get; set; }
    }
}
