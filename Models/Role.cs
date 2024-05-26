using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab01.Models
{
    public class Role
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<User> users { get; set; }//! muon keo khoa cho ai thi dan cai nay cua nguoi do vao
    }
}