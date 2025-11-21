using Datalake.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Shared.Infrastructure.Database.Interfaces;

public interface IUserAccessDbContext
{
	DbSet<CalculatedAccessRule> CalculatedAccessRules { get; }
}
