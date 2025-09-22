namespace Campaign.Watch.Domain.Enums
{
    /// <summary>
    /// Define os tipos de canais de comunicação disponíveis.
    /// </summary>
    public enum TypeChannels
    {
        /// <summary>
        /// Canal de E-mail.
        /// </summary>
        EffectiveMail = 1,

        /// <summary>
        /// Canal de SMS (Short Message Service).
        /// </summary>
        EffectiveSms = 2,

        /// <summary>
        /// Canal de  Push.
        /// </summary>
        EffectivePush = 3,

        /// <summary>
        /// Canal de Social (Facebook - Custom Audience).
        /// </summary>
        EffectiveSocial = 4,

        /// <summary>
        /// Canal de WhatsApp.
        /// </summary>
        EffectiveWhatsApp = 5,

        /// <summary>
        /// Canal de Landing Pages.
        /// </summary>
        EffectivePages = 6
    }
}