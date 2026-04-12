using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketStat.Database.Context.Migrations
{
    /// <inheritdoc />
    public partial class GrantUsersTablePrivilegesToAnalyst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
GRANT INSERT, UPDATE ON TABLE marketstat.users TO marketstat_analyst;

DO $$
DECLARE
    seq_name text;
BEGIN
    SELECT pg_get_serial_sequence('marketstat.users', 'user_id') INTO seq_name;

    IF seq_name IS NOT NULL THEN
        EXECUTE format(
            'GRANT USAGE, SELECT ON SEQUENCE %s TO marketstat_analyst',
            seq_name
        );
    END IF;
END
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
REVOKE INSERT, UPDATE ON TABLE marketstat.users FROM marketstat_analyst;

DO $$
DECLARE
    seq_name text;
BEGIN
    SELECT pg_get_serial_sequence('marketstat.users', 'user_id') INTO seq_name;

    IF seq_name IS NOT NULL THEN
        EXECUTE format(
            'REVOKE USAGE, SELECT ON SEQUENCE %s FROM marketstat_analyst',
            seq_name
        );
    END IF;
END
$$;
");
        }
    }
}