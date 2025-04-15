using DeadWallet.DAL.Models;

namespace DeadWallet.PL.Models
{
    public class AdminViewModel
    {
        public IEnumerable<Tag> tags { get; set; }
        public IEnumerable<DeadWalletUser> users { get; set; }
    }
}
