namespace SecuroteckWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogKey = c.String(nullable: false, maxLength: 128),
                        LogString = c.String(),
                        LogDateTime = c.String(),
                        User_ApiKey = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.LogKey)
                .ForeignKey("dbo.Users", t => t.User_ApiKey)
                .Index(t => t.User_ApiKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "User_ApiKey", "dbo.Users");
            DropIndex("dbo.Logs", new[] { "User_ApiKey" });
            DropTable("dbo.Logs");
        }
    }
}
