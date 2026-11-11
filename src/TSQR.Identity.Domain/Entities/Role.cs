using TSQR.Common.Extensions;

namespace TSQR.Identity.Domain.Entities;

public sealed class Role : IAggregateRoot
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> Permissions => _permissions;

    public static Result<Role> Create(string name, IEnumerable<string>? permissions = null)
    {
        var nameResult = ValidateName(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = nameResult.Value,
            NormalizedName = nameResult.Value.ToUpperInvariant(),
        };

        if (permissions is not null)
        {
            foreach (var permission in permissions)
            {
                var permissionResult = role.AddPermission(permission);
                if (permissionResult.IsFailure)
                    return permissionResult.Error;
            }
        }

        return role;
    }

    public Result Rename(string name)
    {
        var nameResult = ValidateName(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;
        NormalizedName = nameResult.Value.ToUpperInvariant();
        return Result.Success();
    }

    public Result AddPermission(string permission)
    {
        var permissionResult = permission.Validate(nameof(permission));
        if (permissionResult.IsFailure)
            return permissionResult.Error;

        _permissions.Add(permissionResult.Value);
        return Result.Success();
    }

    public Result RemovePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission) || !_permissions.Remove(permission))
            return new DomainError(nameof(permission), "Permission is not assigned to this role.");

        return Result.Success();
    }

    private static Result<string> ValidateName(string name)
    {
        var nameResult = name.Validate(nameof(Name));
        if (nameResult.IsFailure)
            return nameResult.Error;

        var trimmedName = nameResult.Value.Trim();
        if (trimmedName.Length > 100)
            return new ValidationError(nameof(Name), "Role name cannot exceed 100 characters.");

        return trimmedName;
    }
}
