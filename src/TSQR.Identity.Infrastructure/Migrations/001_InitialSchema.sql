CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id TEXT PRIMARY KEY,
    UserName TEXT NOT NULL UNIQUE,
    NormalizedUserName TEXT NOT NULL UNIQUE,
    Email TEXT,
    NormalizedEmail TEXT,
    EmailConfirmed BOOLEAN DEFAULT FALSE,
    PasswordHash TEXT,
    SecurityStamp TEXT,
    ConcurrencyStamp TEXT,
    PhoneNumber TEXT,
    PhoneNumberConfirmed BOOLEAN DEFAULT FALSE,
    TwoFactorEnabled BOOLEAN DEFAULT FALSE,
    LockoutEnd TIMESTAMPTZ,
    LockoutEnabled BOOLEAN DEFAULT FALSE,
    AccessFailedCount INTEGER DEFAULT 0,
    FirstName TEXT,
    LastName TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    NormalizedName TEXT NOT NULL UNIQUE,
    ConcurrencyStamp TEXT
);

CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId TEXT NOT NULL REFERENCES AspNetUsers(Id),
    RoleId TEXT NOT NULL REFERENCES AspNetRoles(Id),
    PRIMARY KEY (UserId, RoleId)
);

CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id SERIAL PRIMARY KEY,
    UserId TEXT NOT NULL REFERENCES AspNetUsers(Id),
    ClaimType TEXT,
    ClaimValue TEXT
);

CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL REFERENCES AspNetUsers(Id),
    TokenHash TEXT NOT NULL,
    ExpiresAt TIMESTAMPTZ NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    RevokedAt TIMESTAMPTZ
);
