#nullable disable

namespace MarketStat.Database.Context.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "marketstat");

            migrationBuilder.CreateTable(
                name: "dim_date",
                schema: "marketstat",
                columns: table => new
                {
                    date_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_date = table.Column<DateOnly>(type: "date", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_date", x => x.date_id);
                    table.CheckConstraint("CK_dim_date_month", "\"month\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_dim_date_quarter", "\"quarter\" BETWEEN 1 AND 4");
                });

            migrationBuilder.CreateTable(
                name: "dim_education",
                schema: "marketstat",
                columns: table => new
                {
                    education_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    specialty_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    specialty_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    education_level_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_education", x => x.education_id);
                });

            migrationBuilder.CreateTable(
                name: "dim_industry_field",
                schema: "marketstat",
                columns: table => new
                {
                    industry_field_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    industry_field_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    industry_field_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_industry_field", x => x.industry_field_id);
                });

            migrationBuilder.CreateTable(
                name: "dim_location",
                schema: "marketstat",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    city_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    oblast_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    district_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_location", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "marketstat",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "dim_employee",
                schema: "marketstat",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_ref_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    career_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    education_id = table.Column<int>(type: "integer", nullable: true),
                    graduation_year = table.Column<short>(type: "smallint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_employee", x => x.employee_id);
                    table.CheckConstraint("ck_career_after_birth", "\"career_start_date\" > \"birth_date\"");
                    table.CheckConstraint("ck_career_min_age", "\"career_start_date\" >= \"birth_date\" + INTERVAL '16 years'");
                    table.CheckConstraint("ck_dim_emp_birth_date", "\"birth_date\" <= CURRENT_DATE");
                    table.CheckConstraint("ck_dim_emp_career_start", "\"career_start_date\" <= CURRENT_DATE");
                    table.ForeignKey(
                        name: "fk_dim_employee_education",
                        column: x => x.education_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_education",
                        principalColumn: "education_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "dim_employer",
                schema: "marketstat",
                columns: table => new
                {
                    employer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    ogrn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    kpp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    registration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    legal_address = table.Column<string>(type: "text", nullable: false),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    industry_field_id = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_employer", x => x.employer_id);
                    table.ForeignKey(
                        name: "fk_dim_employer_industry",
                        column: x => x.industry_field_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_industry_field",
                        principalColumn: "industry_field_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dim_job",
                schema: "marketstat",
                columns: table => new
                {
                    job_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_role_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    standard_job_role_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    hierarchy_level_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    industry_field_id = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_job", x => x.job_id);
                    table.ForeignKey(
                        name: "fk_dim_job_industry",
                        column: x => x.industry_field_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_industry_field",
                        principalColumn: "industry_field_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fact_salaries",
                schema: "marketstat",
                columns: table => new
                {
                    salary_fact_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date_id = table.Column<int>(type: "integer", nullable: false),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    employer_id = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    salary_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fact_salaries", x => x.salary_fact_id);
                    table.ForeignKey(
                        name: "fk_fact_date",
                        column: x => x.date_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_date",
                        principalColumn: "date_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fact_employee",
                        column: x => x.employee_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_employee",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fact_employer",
                        column: x => x.employer_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_employer",
                        principalColumn: "employer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fact_job",
                        column: x => x.job_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_job",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fact_location",
                        column: x => x.location_id,
                        principalSchema: "marketstat",
                        principalTable: "dim_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dim_date_full_date",
                schema: "marketstat",
                table: "dim_date",
                column: "full_date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_education",
                schema: "marketstat",
                table: "dim_education",
                columns: new[] { "specialty_name", "education_level_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dim_employee_education_id",
                schema: "marketstat",
                table: "dim_employee",
                column: "education_id");

            migrationBuilder.CreateIndex(
                name: "uq_dim_employee_ref_id",
                schema: "marketstat",
                table: "dim_employee",
                column: "employee_ref_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dim_employer_industry_field_id",
                schema: "marketstat",
                table: "dim_employer",
                column: "industry_field_id");

            migrationBuilder.CreateIndex(
                name: "uq_dim_employer_inn",
                schema: "marketstat",
                table: "dim_employer",
                column: "inn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_employer_name",
                schema: "marketstat",
                table: "dim_employer",
                column: "employer_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_employer_ogrn",
                schema: "marketstat",
                table: "dim_employer",
                column: "ogrn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_industry_field_code",
                schema: "marketstat",
                table: "dim_industry_field",
                column: "industry_field_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_industry_field_name",
                schema: "marketstat",
                table: "dim_industry_field",
                column: "industry_field_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dim_job_industry_field_id",
                schema: "marketstat",
                table: "dim_job",
                column: "industry_field_id");

            migrationBuilder.CreateIndex(
                name: "uq_dim_job",
                schema: "marketstat",
                table: "dim_job",
                columns: new[] { "job_role_title", "standard_job_role_title", "hierarchy_level_name", "industry_field_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dim_location",
                schema: "marketstat",
                table: "dim_location",
                columns: new[] { "city_name", "oblast_name", "district_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_fact_date",
                schema: "marketstat",
                table: "fact_salaries",
                column: "date_id");

            migrationBuilder.CreateIndex(
                name: "idx_fact_employee",
                schema: "marketstat",
                table: "fact_salaries",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "idx_fact_employer",
                schema: "marketstat",
                table: "fact_salaries",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "idx_fact_job",
                schema: "marketstat",
                table: "fact_salaries",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "idx_fact_location",
                schema: "marketstat",
                table: "fact_salaries",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                schema: "marketstat",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_username",
                schema: "marketstat",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fact_salaries",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "users",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_date",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_employee",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_employer",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_job",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_location",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_education",
                schema: "marketstat");

            migrationBuilder.DropTable(
                name: "dim_industry_field",
                schema: "marketstat");
        }
    }
}
