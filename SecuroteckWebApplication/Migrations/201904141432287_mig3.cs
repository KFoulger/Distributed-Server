namespace SecuroteckWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchivedLogs",
                c => new
                    {
                        LogKey = c.String(nullable: false, maxLength: 128),
                        LogString = c.String(),
                        UserKey = c.String(),
                        DateTime = c.String(),
                    })
                .PrimaryKey(t => t.LogKey);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ArchivedLogs");
        }
    }
}
