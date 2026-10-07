namespace StoreManagementSystem.Web.ViewModels.Sidebar
{
    public class SidebarArrangementRequest
    {
        public List<SidebarArrangementSectionDto> Sections { get; set; }
            = new();
    }


    public class SidebarArrangementSectionDto
    {
        public int Id { get; set; }

        public int DisplayOrder { get; set; }

        public List<SidebarArrangementItemDto> Items { get; set; }
            = new();
    }


    public class SidebarArrangementItemDto
    {
        public int Id { get; set; }

        public int SectionId { get; set; }

        public int DisplayOrder { get; set; }
    }
}