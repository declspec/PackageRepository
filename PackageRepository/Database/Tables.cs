namespace PackageRepository.Database {
    public static class Tables {
        public const string PackageVersions = "package_versions";
        public const string PackageTarballs = "package_tarballs";
        public const string DistTags = "dist_tags";

        public const string Organisations = "organisations";
        public const string OrganisationTeams = "organisation_teams";
        public const string OrganisationUsers = "organisation_users";
        public const string OrganisationTeamMembers = "organisation_team_members";

        public const string Things = "things";
        public const string ThingOrganisationPermissions = "thing_organisation_permissions";
        public const string ThingTeamPermissions = "thing_team_permissions";
        public const string ThingUserPermissions = "thing_user_permissions";

        public const string NpmPackages = "npm_package";
        public const string NpmTarballs = "npm_tarballs";
    }
}
