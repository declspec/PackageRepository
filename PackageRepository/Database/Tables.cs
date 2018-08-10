namespace PackageRepository.Database {
    public static class Tables {
        public const string PackageVersions = "package_versions";
        public const string PackageTarballs = "package_tarballs";
        public const string DistTags = "dist_tags";

        public const string Organisations = "organisations";
        public const string OrganisationTeams = "organisation_teams";
        public const string OrganisationUsers = "organisation_users";
        public const string OrganisationTeamMembers = "organisation_team_members";

        public const string Objects = "objects";
        public const string ObjectOrganisationPermissions = "object_organisation_permissions";
        public const string ObjectTeamPermissions = "object_team_permissions";
        public const string ObjectUserPermissions = "object_user_permissions";
    }
}
