using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositorys
{
    public class UsersRepository : Repository<user>, IUsersRepository
    {
        private readonly DataContext _dbContext;
        public UsersRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Object deleteuseer()
        {
            return _dbContext.users.Include(r => r.id).Where(r=>r.id==1);
        }
    }
}
