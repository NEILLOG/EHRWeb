using BASE.Models.DB;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Checkin
    {
        public TbActivity Activity { get; set; }
        public TbActivitySection Section { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
    }
}
