using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BranchTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski string veriyi (örn: "1,2,3") integer dizisine çevirir.
            // Kolon 'nullable: false' olduğu için, veri boş string ('') ise hata vermemesi adına boş dizi ('{}') atanır.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Branches"" 
                ALTER COLUMN ""Tags"" TYPE integer[] 
                USING COALESCE(string_to_array(NULLIF(""Tags"", ''), ',')::integer[], '{}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma durumunda: integer dizisini tekrar virgülle ayrılmış string'e (text) çevirir.
            // Yine boş array durumunda NULL olmaması için COALESCE ile boş string ('') atanır.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Branches"" 
                ALTER COLUMN ""Tags"" TYPE text 
                USING COALESCE(array_to_string(""Tags"", ','), '');
            ");
        }
    }
}
