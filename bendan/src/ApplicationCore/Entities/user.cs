using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ApplicationCore.Entities
{
    [Table("users")]
    public class user : BaseEntity
    {
        [Column(TypeName = "varchar(32)")]
        public String name { get; set; }
        [Column(TypeName = "varchar(64)")]
        public String password { get; set; }
        [Column(TypeName = "varchar(150)")]
        public String mail { get; set; }
        [Column(TypeName = "varchar(150)")]
        public String url { get; set; }
        [Column(TypeName = "varchar(150)")]
        public String nickName { get; set; }
        [Column(TypeName = "int(10)")]
        public int created { get; set; } = 0;
        [Column(TypeName = "int(10)")]
        public int activated { get; set; } = 0;
        [Column(TypeName = "varchar(16)")]
        public String group { get; set; }
    }
}
