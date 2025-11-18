using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class change_geometry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                            @"ALTER TABLE ""Branches"" 
                  ALTER COLUMN ""Address_Location"" 
                  TYPE geometry(Point, 4326) 
                  USING ST_SetSRID(""Address_Location""::geometry, 4326);"
                        );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                            @"ALTER TABLE ""Branches"" 
                  ALTER COLUMN ""Address_Location"" 
                  TYPE geography(Point, 4326) 
                  USING ST_SetSRID(""Address_Location""::geometry, 4326)::geography;"
                        );
        }
    }
}
