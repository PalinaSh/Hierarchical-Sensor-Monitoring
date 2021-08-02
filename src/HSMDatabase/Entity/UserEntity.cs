﻿using System;
using System.Collections.Generic;

namespace HSMDatabase.Entity
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public bool IsAdmin { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CertificateThumbprint { get; set; }
        public string CertificateFileName { get; set; }
        public List<KeyValuePair<string, byte>> ProductsRoles { get; set; }
    }
}