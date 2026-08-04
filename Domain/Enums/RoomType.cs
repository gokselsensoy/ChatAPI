namespace Domain.Enums
{
    public enum RoomType
    {
        /// <summary>1:1 geo'suz sohbet (şube dışı devam eder).</summary>
        Private,

        /// <summary>Şube public sohbet odası.</summary>
        Public,

        /// <summary>Geo-kilitli oda (1:1 veya masa). Check-in gerekir.</summary>
        Group
    }
}
