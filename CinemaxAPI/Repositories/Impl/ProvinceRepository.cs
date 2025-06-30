using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
	public class ProvinceRepository : Repository<Province>, IProvinceRepository
	{
		public ProvinceRepository(CinemaxServerDbContext context) : base(context)
		{

		}
	}
}
