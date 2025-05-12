using DeadWallet.DAL.Models;

namespace DeadWallet.PL.Models
{

    public class AdminViewModel
    {
        public IEnumerable<Transaction> transactions { get; set; }

        public IEnumerable<Tag> tags { get; set; }
        public IEnumerable<DeadWalletUser> users { get; set; }
    }
}
