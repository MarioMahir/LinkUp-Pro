namespace LinkUpPro.Core.Entities
{
    public class FriendRequest
    {
        public int Id { get; set; }

        public string SenderId { get; set; }

        public ApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }

        public ApplicationUser Receiver { get; set; }

        public string Status { get; set; } = "Pending";

        public string PairKey { get; set; } = string.Empty;

        public bool IsHiddenFromSender { get; set; }

        public bool IsHiddenFromReceiver { get; set; }

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedDate { get; set; }

        public static string MakePairKey(string userIdA, string userIdB) =>
            string.CompareOrdinal(userIdA, userIdB) < 0
                ? $"{userIdA}_{userIdB}"
                : $"{userIdB}_{userIdA}";
    }
}
