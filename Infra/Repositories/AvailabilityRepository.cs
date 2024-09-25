using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;

namespace Infra.Repositories;

public class AvailabilityRepository(ApplicationDbContext context)
    : Repository<Availability>(context), IAvailabilityRepository;