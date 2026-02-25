/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace Tobasa.Entities
{
    public partial class BaseUsers
    {
        public BaseUsers()
        {
            BaseUserRole = new HashSet<BaseUserRole>();
            BaseUserSite = new HashSet<BaseUserSite>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Image { get; set; } = null!;
        public bool? Enabled { get; set; }
        public byte[] PasswordSalt { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public bool? AllowLogin { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Expired { get; set; }
        public DateTime? LastLogin { get; set; }
        public string UniqueCode { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public string Phone { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Nik { get; set; } = null!;
        public virtual ICollection<BaseUserRole> BaseUserRole { get; set; }
        public virtual ICollection<BaseUserSite> BaseUserSite { get; set; }
    }
}