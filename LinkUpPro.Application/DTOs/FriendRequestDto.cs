namespace LinkUpPro.Application.DTOs
{
    public class FriendRequestDto
    {
        public int Id { get; set; }

        public string OtherUserId { get; set; } = string.Empty;

        public string OtherUserName { get; set; } = string.Empty;

        public string OtherFullName { get; set; } = string.Empty;

        public string? OtherProfilePicture { get; set; }

        public int MutualFriendsCount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime SentDate { get; set; }

        public DateTime? RespondedDate { get; set; }

        public bool IsSentByCurrentUser { get; set; }
    }
}
