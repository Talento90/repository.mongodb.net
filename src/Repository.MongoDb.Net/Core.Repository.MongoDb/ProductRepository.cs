using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repository.MongoDb
{
    public class ProductRepository : MongoRepository<Product>, IProductRepository
    {

        public ProductRepository(string connectionString): base(connectionString)
        {

        }
    }
}
