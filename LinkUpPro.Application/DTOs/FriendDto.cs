namespace LinkUpPro.Application.DTOs
{
    public class FriendDto
    {
        public int FriendshipId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        public int MutualFriendsCount { get; set; }
    }
}
