using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Interfaces;

public interface IStackable
{
    bool CanStack(Cargo cargo);
}