namespace BELibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using static System.Data.Entity.Infrastructure.Design.Executor;

    public partial class New_DetailRecord_table : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DetailRecord", "ReDate", c => c.DateTime(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.DetailRecord", "ReDate");
        }
    }
}
